using Microsoft.Exchange.WebServices.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public FDLManager()
        {
            _db = new DBEntities();

            StartBackgroundOperations();
            UserSettings.SettingsChanged += ooooo;
        }

        private void ooooo(string oooo)
        {
            string prop = oooo;
        }

        private void StartBackgroundOperations()
        {
            subscribeThread = new Thread(SubscribeNotifications);
            subscribeThread.Name = "Exchange Subscription Thread";
            subscribeThread.IsBackground = true;
            subscribeThread.Start();

            syncThread = new Thread(SyncFDLOnExchange);
            syncThread.Name = "Sync FDL Thread";
            syncThread.IsBackground = true;
            syncThread.Start();
        }

        private void SyncAllFDL(ExchangeService service)
        {
            ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
            FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };
            
            Directory.CreateDirectory(ApplicationSettings.Directories.FDL);

            foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), "from:" + ApplicationSettings.FDL.FDLEmailAddress, folderView, itemView))
            {
                if (!(item is EmailMessage))
                    continue;

                EmailMessage message = EmailMessage.Bind(service, item.Id);

                //check accepted and rejected FDL
                if (message.Subject.Contains("RECIVED"))
                    Debugger.Break();

                if (message.HasAttachments)
                {
                    foreach(Attachment attachment in message.Attachments)
                    {
                        if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                            continue;
                        
                        FileAttachment fileAttachment = message.Attachments[0] as FileAttachment;

                        if (!File.Exists(ApplicationSettings.Directories.FDL + "\\" + fileAttachment.Name))
                            fileAttachment.Load(ApplicationSettings.Directories.FDL + "\\" + fileAttachment.Name);
                    }
                }
            }
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
            bool IsValid = false;

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

                    IsValid = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsValid);
        }

        private void SyncFDLOnExchange()
        {
            bool IsValid = false;

            ExchangeService service = new ExchangeService();
            service.TraceEnabled = true;
            service.TraceFlags = TraceFlags.None;
            
            do
            {
                try
                {
                    service.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    service.AutodiscoverUrl(UserSettings.Email.EmailAddress, RedirectionUrlValidationCallback);
                    IsValid = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsValid);

            SyncAllFDL(service);
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

        }

        private void Connection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
        {

        }
        #endregion
    }

    public enum EFDLStatus
    {
        New = 0,
        Accepted = 1,
        Rejected = 2
    }
}
