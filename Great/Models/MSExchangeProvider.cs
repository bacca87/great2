using GalaSoft.MvvmLight.Messaging;
using Great.Models.DTO;
using Great.Utils.Messages;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using static Great.Models.ExchangeTraceListener;

namespace Great.Models
{
    //La libreria Microsoft EWS (https://github.com/OfficeDev/ews-managed-api) è deprecata
    //il pacchetto Nuget non viene aggiornato e la versione su github è piu aggiornata e con parecchi bugs corretti, di conseguenza la libreria è stata ricompilata a mano e aggiunta alle reference del progetto.

    public class MSExchangeProvider
    {
        private ExchangeService exService = new ExchangeService();

        public event EventHandler<NewMessageEventArgs> OnNewMessage;

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
                    Messenger.Default.Send(new StatusChangeMessage<EExchangeStatus>(this, exchangeStatus));
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

        #region Threads
        private void MainThread()
        {
            ExchangeTraceListener trace = new ExchangeTraceListener();
            exService.TraceListener = trace;
            exService.TraceFlags = TraceFlags.AutodiscoverConfiguration;
            exService.TraceEnabled = true;

            do
            {
                try
                {   
                    exService.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    //exService.UseDefaultCredentials = true;
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
                catch
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        ExchangeStatus = EExchangeStatus.LoginError;
                        WaitCredentialsChange();
                    }   
                    else
                        Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry);
                }
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
            ExchangeTraceListener trace = new ExchangeTraceListener();
            ExchangeService service = new ExchangeService
            {
                TraceListener = trace,
                TraceFlags = TraceFlags.AutodiscoverConfiguration,
                TraceEnabled = true,
                Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword),
                Url = exServiceUri
            };

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
                        catch
                        {
                            if (trace.Result == ETraceResult.LoginError)
                            {
                                ExchangeStatus = EExchangeStatus.LoginError;
                                WaitCredentialsChange();
                            }
                            else
                                Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry);
                        }
                    }
                    while (!IsSent);
                }

                Thread.Sleep(ApplicationSettings.General.WaitForNextEmailCheck);
            }
        }

        private void SubscribeNotificationsThread()
        {
            ExchangeTraceListener trace = new ExchangeTraceListener();
            ExchangeService service = new ExchangeService
            {
                TraceListener = trace,
                TraceFlags = TraceFlags.AutodiscoverConfiguration,
                TraceEnabled = true,
                Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword),
                Url = exServiceUri
            };

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
                catch
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        ExchangeStatus = EExchangeStatus.LoginError;
                        WaitCredentialsChange();
                    }
                    else
                        Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry);
                }
            } while (connection == null || !connection.IsOpen);

            ExchangeStatus = EExchangeStatus.Syncronizing;
        }

        private void ExchangeSync()
        {
            ExchangeTraceListener trace = new ExchangeTraceListener();
            ExchangeService service = new ExchangeService
            {
                TraceListener = trace,
                TraceFlags = TraceFlags.AutodiscoverConfiguration,
                TraceEnabled = true,
                Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword),
                Url = exServiceUri
            };

            bool IsSynced = false;
            ExchangeStatus = EExchangeStatus.Syncronizing;

            do
            {
                try
                {
                    ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
                    FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };
                    folderView.PropertySet.Add(FolderSchema.WellKnownFolderName);

                    itemView.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Ascending);

                    // try to get last week messages (high priority)
                    foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), "from:" + ApplicationSettings.EmailRecipients.FDLSystem + " received: last week", folderView, itemView))
                    {
                        if (!(item is EmailMessage))
                            continue;

                        EmailMessage message = EmailMessage.Bind(service, item.Id);
                        NotifyNewMessage(message);
                    }

                    // then all the other messages
                    foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), "from:" + ApplicationSettings.EmailRecipients.FDLSystem, folderView, itemView))
                    {
                        if (!(item is EmailMessage))
                            continue;

                        EmailMessage message = EmailMessage.Bind(service, item.Id);
                        NotifyNewMessage(message);
                    }

                    IsSynced = true;
                    ExchangeStatus = EExchangeStatus.Syncronized;

                }
                catch
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        ExchangeStatus = EExchangeStatus.LoginError;
                        WaitCredentialsChange();
                    }
                    else
                        Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry);
                }
            } while (!IsSynced);
        }
        #endregion

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
                ExchangeStatus = EExchangeStatus.Syncronizing;
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

        #region Private Methods
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

        protected void NotifyNewMessage(EmailMessage e)
        {
            OnNewMessage?.Invoke(this, new NewMessageEventArgs(e));
        }

        private void WaitCredentialsChange()
        {
            string LastAddress = UserSettings.Email.EmailAddress;
            string LastPassword = UserSettings.Email.EmailPassword;

            while(LastAddress == UserSettings.Email.EmailAddress && LastPassword == UserSettings.Email.EmailPassword)
                Thread.Sleep(ApplicationSettings.General.WaitForCredentialsCheck);
        }
        #endregion

        #region Public Methods
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

        public static bool CheckEmailAddress(string address, out string error)
        {
            error = string.Empty;

            try
            {
                MailAddress m = new MailAddress(address);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
        #endregion
    }

    public class NewMessageEventArgs : EventArgs
    {
        public EmailMessage Message { get; internal set; }

        public NewMessageEventArgs(EmailMessage message)
        {
            Message = message;
        }
    }

    public class ExchangeTraceListener : ITraceListener
    {
        public enum ETraceResult
        {
            Ok,
            LoginError,
            AutodiscoverError
        }

        public ETraceResult Result { get; internal set; }

        public ExchangeTraceListener()
        {
            Result = ETraceResult.Ok;
        }

        public void Trace(string traceType, string traceMessage)
        {
            if(traceMessage.Contains("(401)"))
                Result = ETraceResult.LoginError;

            if (traceMessage.Contains("No matching Autodiscover DNS SRV records were found."))
                Result = ETraceResult.AutodiscoverError;
        }
    }
}
