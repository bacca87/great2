using Microsoft.Exchange.WebServices.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Great.Models
{
    public class FDLManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private ExchangeService notificationsService { get; set; }
        private StreamingSubscription streamingSubscription { get; set; }
        private StreamingSubscriptionConnection connection { get; set; }

        private Thread subscribeThread;
        private Thread syncThread;

        private DBEntities _db;

        public EExchangeStatus ExchangeStatus { get; internal set; }
        
        public FDLManager()
        {
            _db = new DBEntities();
            
            StartBackgroundOperations();
        }
        
        private void StartBackgroundOperations()
        {
            subscribeThread = new Thread(SubscribeNotifications);
            subscribeThread.Name = "Exchange Subscription Thread";
            subscribeThread.IsBackground = true;
            subscribeThread.Start();

            syncThread = new Thread(ExchangeSync);
            syncThread.Name = "Exchange Sync";
            syncThread.IsBackground = true;
            syncThread.Start();
        }

        private void SyncAll(ExchangeService service)
        {
            DBEntities db = new DBEntities();
            ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
            FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };
            
            Directory.CreateDirectory(ApplicationSettings.Directories.FDL);

            foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), "from:" + ApplicationSettings.FDL.EmailAddress, folderView, itemView))
            {
                if (!(item is EmailMessage))
                    continue;

                EmailMessage message = EmailMessage.Bind(service, item.Id);
                
                if (message.Subject.Contains(ApplicationSettings.FDL.FDL_Accepted))
                {
                    long fdlNumber = ExtractFDLFromSubject(message.Subject, EMessageType.FDL_Accepted);
                    FDL acceptedFDL = db.FDLs.SingleOrDefault(fdl => fdl.Id == fdlNumber);

                    if(acceptedFDL != null)
                        acceptedFDL.Status = (long)EFDLStatus.Accepted;
                }
                else if(message.Subject.Contains(ApplicationSettings.FDL.FDL_Rejected))
                {
                    long fdlNumber = ExtractFDLFromSubject(message.Subject, EMessageType.FDL_Rejected);
                    FDL acceptedFDL = db.FDLs.SingleOrDefault(fdl => fdl.Id == fdlNumber);

                    if (acceptedFDL != null)
                        acceptedFDL.Status = (long)EFDLStatus.Rejected;
                }
                else if(message.Subject.Contains(ApplicationSettings.FDL.EA_Rejected))
                {

                }
                else if (message.Subject.Contains(ApplicationSettings.FDL.EA_RejectedResubmission))
                {

                }
                else if (message.HasAttachments)
                {
                    foreach(Attachment attachment in message.Attachments)
                    {
                        if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                            continue;
                        
                        FileAttachment fileAttachment = attachment as FileAttachment;

                        if (!File.Exists(ApplicationSettings.Directories.FDL + "\\" + fileAttachment.Name))
                            fileAttachment.Load(ApplicationSettings.Directories.FDL + "\\" + fileAttachment.Name);
                    }
                }
            }

            db.SaveChanges();
        }
        
        private long ExtractFDLFromSubject(string subject, EMessageType type)
        {
            long FDL = 0;
            string[] words;

            try
            {
                switch(type)
                {
                    case EMessageType.FDL_Accepted:
                    case EMessageType.FDL_Rejected:
                        // INVALID FDL (XXXXX)
                        // FDL RECEIVED (XXXXX)
                        Match match = Regex.Match(subject, @"\(([^)]*)\)");
                        if (match.Success || match.Groups.Count > 0)
                            FDL = long.Parse(match.Groups[1].Value);
                        break;
                    case EMessageType.EA_Rejected:
                        // FDL XXXXX NOTA SPESE RIFIUTATA
                        //  0    1    2     3       4
                        words = subject.Split(' ');
                        if(words.Length > 1)
                            FDL = long.Parse(words[1]);
                        break;
                    case EMessageType.EA_RejectedResubmission:
                        // Reinvio nota spese YYYY/XXXXX respinto
                        //    0      1    2       3         4
                        words = subject.Split(' ');
                        if (words.Length > 3)
                        {
                            words = words[3].Split('/');
                            if(words.Length > 1)
                                FDL = long.Parse(words[1]);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch { }

            return FDL;
        }
        
        private IEnumerable<Item> FindItemsInSubfolders(ExchangeService service, FolderId root, string query, FolderView folderView, ItemView itemView)
        {
            FindFoldersResults foldersResults;
            FindItemsResults<Item> itemsResults;

            do
            {
                foldersResults = service.FindFolders(root, folderView);

                foreach (Folder folder in foldersResults)
                {
                    do
                    {
                        itemsResults = service.FindItems(folder.Id, query, itemView);
                        
                        foreach (Item item in itemsResults)
                            yield return item;

                        if (itemsResults.MoreAvailable)
                            itemView.Offset += itemView.PageSize;

                    } while (itemsResults.MoreAvailable);
                }

                if (foldersResults.MoreAvailable)
                    folderView.Offset += folderView.PageSize;

            } while (foldersResults.MoreAvailable);

            // reset the offset for a new search in current folder
            itemView.Offset = 0;

            do
            {
                itemsResults = service.FindItems(root, query, itemView);

                foreach (Item item in itemsResults)
                    yield return item;

                if (itemsResults.MoreAvailable)
                    itemView.Offset += itemView.PageSize;

            } while (itemsResults.MoreAvailable);
        }
        
        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
                result = true;

            return result;
        }

        #region Threads
        private void SubscribeNotifications()
        {
            bool IsSubscribed = false;

            notificationsService = new ExchangeService();
            notificationsService.TraceEnabled = true;
            notificationsService.TraceFlags = TraceFlags.None;
            
            do
            {
                try
                {
                    notificationsService.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    notificationsService.AutodiscoverUrl(UserSettings.Email.EmailAddress, RedirectionUrlValidationCallback);

                    streamingSubscription = notificationsService.SubscribeToStreamingNotificationsOnAllFolders(EventType.NewMail);

                    connection = new StreamingSubscriptionConnection(notificationsService, 30);
                    connection.AddSubscription(streamingSubscription);
                    connection.OnNotificationEvent += Connection_OnNotificationEvent;
                    connection.OnSubscriptionError += Connection_OnSubscriptionError;
                    connection.OnDisconnect += Connection_OnDisconnect;
                    connection.Open();

                    IsSubscribed = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsSubscribed);
        }

        private void ExchangeSync()
        {
            bool IsSynced = false;

            ExchangeService service = new ExchangeService();
            service.TraceEnabled = true;
            service.TraceFlags = TraceFlags.None;
            
            do
            {
                try
                {
                    service.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    service.AutodiscoverUrl(UserSettings.Email.EmailAddress, RedirectionUrlValidationCallback);

                    SyncAll(service);

                    IsSynced = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsSynced);
        }
        #endregion

        #region Subscription Events Handling
        private void Connection_OnNotificationEvent(object sender, NotificationEventArgs args)
        {
            foreach (NotificationEvent e in args.Events)
            {
                switch (e.EventType)
                {
                    case EventType.NewMail:

                        break;

                    default:
                        break;
                }
            }
        }

        private void Connection_OnDisconnect(object sender, SubscriptionErrorEventArgs args)
        {
            connection.Open();
        }

        private void Connection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
        {

        }
        #endregion
    }

    public enum EMessageType
    {
        FDL_Accepted,
        FDL_Rejected,
        EA_Rejected,
        EA_RejectedResubmission
    }

    public enum EExchangeStatus
    {
        Disconnected,
        Syncing,
        Connected
    }

    public enum EFDLStatus
    {
        New = 0,
        Accepted = 1,
        Rejected = 2
    }
}
