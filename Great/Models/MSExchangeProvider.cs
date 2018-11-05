using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Great.Models
{
    //La libreria Microsoft EWS (https://github.com/OfficeDev/ews-managed-api) è deprecata
    //il pacchetto Nuget non viene aggiornato e la versione su github è piu aggiornata e con parecchi bugs corretti, di conseguenza la libreria è stata ricompilata a mano e aggiunta alle reference del progetto.

    public class MSExchangeProvider
    {
        private ExchangeService exService = new ExchangeService();

        public event EventHandler<NewMessageEventArgs> OnNewMessage;
        public event EventHandler<EExchangeStatus> OnNewConnectionStatus;

        private Thread mainThread;
        private Thread emailSenderThread;
        private Thread subscribeThread;
        private Thread syncThread;

        private Uri exServiceUri;

        private ConcurrentQueue<EmailMessageDTO> emailQueue = new ConcurrentQueue<EmailMessageDTO>();

        private EExchangeStatus exchangeStatus = EExchangeStatus.Offline;
        public EExchangeStatus ExchangeStatus
        {
            get
            {
                lock (this)
                {
                    return exchangeStatus;
                }
            }
            internal set
            {
                lock (this)
                {
                    exchangeStatus = value;
                    NotifyNewConnectionStatus(exchangeStatus);
                }
            }
        }

        public MSExchangeProvider()
        {
            mainThread = new Thread(MainThread);
            mainThread.Name = "Exchange Autodiscover Thread";
            mainThread.IsBackground = true;
            mainThread.Start();
        }

        private void MainThread()
        {
            do
            {
                try
                {
                    exService.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    exService.AutodiscoverUrl(UserSettings.Email.EmailAddress, (string redirectionUrl) => {
                        // The default for the validation callback is to reject the URL.
                        bool result = false;
                        Uri redirectionUri = new Uri(redirectionUrl);

                        // Validate the contents of the redirection URL. In this simple validation
                        // callback, the redirection URL is considered valid if it is using HTTPS
                        // to encrypt the authentication credentials. 
                        if (redirectionUri.Scheme == "https")
                            result = true;

                        return result;
                    });
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (exService.Url == null);

            exServiceUri = exService.Url;

            emailSenderThread = new Thread(EmailSenderThread);
            emailSenderThread.Name = "Email Sender";
            emailSenderThread.IsBackground = true;
            emailSenderThread.Start();

            subscribeThread = new Thread(SubscribeNotificationsThread);
            subscribeThread.Name = "Exchange Subscription Thread";
            subscribeThread.IsBackground = true;
            subscribeThread.Start();

            syncThread = new Thread(ExchangeSync);
            syncThread.Name = "Exchange Sync";
            syncThread.IsBackground = true;
            syncThread.Start();
        }

        private void EmailSenderThread()
        {
            bool exit = false;
            ExchangeService service = new ExchangeService();
            service.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
            service.Url = exServiceUri;           

            while (!exit)
            {
                while (!emailQueue.IsEmpty)
                {
                    EmailMessageDTO message;
                    bool IsSent = false;

                    if (!emailQueue.TryDequeue(out message))
                        continue;

                    do
                    {
                        try
                        {
                            EmailMessage msg = new EmailMessage(service);

                            msg.Subject = message.Subject;
                            msg.Body = message.Body;
                            msg.Importance = message.Importance;

                            msg.ToRecipients.AddRange(message.ToRecipients);
                            msg.CcRecipients.AddRange(message.CcRecipients);

                            foreach (string file in message.Attachments)
                                msg.Attachments.AddFileAttachment(file);

                            msg.SendAndSaveCopy();
                            IsSent = true;
                        }
                        catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
                    }
                    while (!IsSent);
                }

                Thread.Sleep(ApplicationSettings.General.WaitForNextEmailCheck);
            }
        }

        private void SubscribeNotificationsThread()
        {
            ExchangeService service = new ExchangeService();
            service.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
            service.Url = exServiceUri;

            StreamingSubscriptionConnection connection = new StreamingSubscriptionConnection(service, 30);

            ExchangeStatus = EExchangeStatus.Connecting;

            do
            {
                try
                {
                    StreamingSubscription streamingSubscription = service.SubscribeToStreamingNotificationsOnAllFolders(EventType.NewMail);
                 
                    connection.AddSubscription(streamingSubscription);
                    connection.OnNotificationEvent += Connection_OnNotificationEvent;
                    connection.OnSubscriptionError += Connection_OnSubscriptionError;
                    connection.OnDisconnect += Connection_OnDisconnect;
                    connection.Open();
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (connection == null || !connection.IsOpen);

            ExchangeStatus = EExchangeStatus.Online;
        }

        private void ExchangeSync()
        {
            ExchangeService service = new ExchangeService();
            service.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
            service.Url = exServiceUri;

            bool IsSynced = false;

            do
            {
                try
                {
                    ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
                    FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };
                    folderView.PropertySet.Add(FolderSchema.WellKnownFolderName);

                    itemView.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Ascending);
                    
                    foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), "from:" + ApplicationSettings.EmailRecipients.FDLSystem, folderView, itemView))
                    {
                        if (!(item is EmailMessage))
                            continue;

                        EmailMessage message = EmailMessage.Bind(service, item.Id);
                        NotifyNewMessage(message);
                    }

                    IsSynced = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsSynced);
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
                    if (folder.WellKnownFolderName == WellKnownFolderName.DeletedItems ||
                        folder.WellKnownFolderName == WellKnownFolderName.SentItems ||
                        folder.WellKnownFolderName == WellKnownFolderName.Drafts ||
                        folder.WellKnownFolderName == WellKnownFolderName.JunkEmail ||
                        folder.WellKnownFolderName == WellKnownFolderName.ConversationHistory ||
                        folder.WellKnownFolderName == WellKnownFolderName.SearchFolders ||
                        folder.WellKnownFolderName == WellKnownFolderName.Calendar ||
                        folder.WellKnownFolderName == WellKnownFolderName.Contacts ||
                        folder.WellKnownFolderName == WellKnownFolderName.QuickContacts ||
                        folder.WellKnownFolderName == WellKnownFolderName.Tasks ||
                        folder.WellKnownFolderName == WellKnownFolderName.Contacts)
                        continue;

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

        public NameResolutionCollection ResolveName(string filter)
        {
            if (exService.Url != null)
                return exService.ResolveName(filter, ResolveNameSearchLocation.ContactsThenDirectory, true);
            else
                return null;
        }

        public void SendEmail(EmailMessageDTO message)
        {
            emailQueue.Enqueue(message);
        }

        protected void NotifyNewMessage(EmailMessage e)
        {
            OnNewMessage?.Invoke(this, new NewMessageEventArgs(e));
        }

        protected void NotifyNewConnectionStatus(EExchangeStatus e)
        {
            OnNewConnectionStatus?.Invoke(this, e);
        }

        #region Subscription Events Handling
        private void Connection_OnNotificationEvent(object sender, NotificationEventArgs args)
        {
            foreach (NotificationEvent e in args.Events)
            {
                var itemEvent = (ItemEvent)e;
                EmailMessage message = EmailMessage.Bind(args.Subscription.Service, itemEvent.ItemId);

                switch (e.EventType)
                {
                    case EventType.NewMail:
                        NotifyNewMessage(message);
                        break;

                    default:
                        break;
                }
            }
        }

        private void Connection_OnDisconnect(object sender, SubscriptionErrorEventArgs args)
        {
            if (ExchangeStatus != EExchangeStatus.Error)
                ExchangeStatus = EExchangeStatus.Offline;

            Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry);
            ExchangeStatus = EExchangeStatus.Reconnecting;

            try
            {
                (sender as StreamingSubscriptionConnection).Open();
                ExchangeStatus = EExchangeStatus.Online;
            }
            catch { }

            Debugger.Break();
        }

        private void Connection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
        {
            ExchangeStatus = EExchangeStatus.Error;
            Debugger.Break();
        }
        #endregion
    }

    public class NewMessageEventArgs : EventArgs
    {
        private EmailMessage _message;
        public EmailMessage Message { get { return _message; } }

        public NewMessageEventArgs(EmailMessage message)
        {
            _message = message;
        }
    }
}
