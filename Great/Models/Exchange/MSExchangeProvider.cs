using GalaSoft.MvvmLight.Messaging;
using Great.Models.DTO;
using Great.Models.Interfaces;
using Great.Utils.Messages;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading;
using static Great.Models.ExchangeTraceListener;

namespace Great.Models
{
    //La libreria Microsoft EWS (https://github.com/OfficeDev/ews-managed-api) è deprecata
    //il pacchetto Nuget non viene aggiornato e la versione su github è piu aggiornata e con parecchi bugs corretti, di conseguenza la libreria è stata ricompilata a mano e aggiunta alle reference del progetto.

    public class MSExchangeProvider : IProvider
    {
        private ExchangeService exService;
        private StreamingSubscriptionConnection subconn;
        private CancellationTokenSource exitToken;

        public event EventHandler<NewMessageEventArgs> OnNewMessage;
        public event EventHandler<MessageEventArgs> OnMessageSent;

        private Thread mainThread;
        private Thread emailSenderThread;
        private Thread subscribeThread;
        private Thread syncThread;

        private Uri exServiceUri;

        private ConcurrentQueue<EmailMessageDTO> emailQueue = new ConcurrentQueue<EmailMessageDTO>();

        private EProviderStatus exchangeStatus = EProviderStatus.Offline;

        public EProviderStatus Status
        {
            get
            {
                lock (this)
                {
                    return exchangeStatus;
                }
            }
            set
            {
                lock (this)
                {
                    if (exchangeStatus != value)
                    {
                        exchangeStatus = value;
                        Messenger.Default.Send(new StatusChangeMessage<EProviderStatus>(this, exchangeStatus));
                    }
                }
            }
        }

        public MSExchangeProvider()
        {
            exitToken = new CancellationTokenSource();
            Connect();
        }

        #region Threads
        private void MainThread()
        {
            ExchangeTraceListener trace = new ExchangeTraceListener();

            exService = new ExchangeService();
            exService.TraceListener = trace;
            exService.TraceFlags = TraceFlags.AutodiscoverConfiguration;
            exService.TraceEnabled = true;

            do
            {
                try
                {
                    exService.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    //exService.UseDefaultCredentials = true;
                    exService.AutodiscoverUrl(UserSettings.Email.EmailAddress, (string redirectionUrl) =>
                    {
                        // The default for the validation callback is to reject the URL.
                        bool result = false;
                        Uri redirectionUri = new Uri(redirectionUrl);

                        // Validate the contents of the redirection URL. In this simple validation
                        // callback, the redirection URL is considered valid if it is using HTTPS
                        // to encrypt the authentication credentials. 
                        if (redirectionUri.Scheme == "https")
                        {
                            result = true;
                        }

                        return result;
                    });
                }
                catch
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        Status = EProviderStatus.LoginError;
                        return;
                    }
                    else
                    {
                        Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                    }
                }
            } while (exService.Url == null && !exitToken.IsCancellationRequested);

            if (exitToken.IsCancellationRequested)
            {
                return;
            }

            Status = EProviderStatus.Connecting;

            exServiceUri = exService.Url;

            if ((emailSenderThread == null || !emailSenderThread.IsAlive) && !exitToken.IsCancellationRequested)
            {
                emailSenderThread = new Thread(EmailSenderThread);
                emailSenderThread.Name = "Email Sender";
                emailSenderThread.IsBackground = true;
                emailSenderThread.Start();
            }

            if ((subscribeThread == null || !subscribeThread.IsAlive) && !exitToken.IsCancellationRequested)
            {
                subscribeThread = new Thread(SubscribeNotificationsThread);
                subscribeThread.Name = "Exchange Subscription Thread";
                subscribeThread.IsBackground = true;
                subscribeThread.Start();
            }

            if ((syncThread == null || !syncThread.IsAlive) && !exitToken.IsCancellationRequested)
            {
                syncThread = new Thread(ExchangeSync);
                syncThread.Name = "Exchange Sync";
                syncThread.IsBackground = true;
                syncThread.Start();
            }
        }

        private void EmailSenderThread()
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

            while (!exitToken.IsCancellationRequested)
            {
                while (!emailQueue.IsEmpty)
                {
                    EmailMessageDTO message;
                    bool IsSent = false;

                    if (!emailQueue.TryDequeue(out message))
                    {
                        continue;
                    }

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
                            {
                                msg.Attachments.AddFileAttachment(file);
                            }

                            msg.SendAndSaveCopy();
                            IsSent = true;

                            NotifyMessageSent(message);
                        }
                        catch
                        {
                            if (trace.Result == ETraceResult.LoginError)
                            {
                                Status = EProviderStatus.LoginError;
                                return;
                            }
                            else
                            {
                                Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                            }
                        }
                    }
                    while (!IsSent);
                }

                Wait(ApplicationSettings.General.WaitForNextEmailCheck);
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

            subconn = new StreamingSubscriptionConnection(service, 30);

            do
            {
                try
                {
                    StreamingSubscription streamingSubscription = service.SubscribeToStreamingNotificationsOnAllFolders(EventType.NewMail);

                    subconn.AddSubscription(streamingSubscription);
                    subconn.OnNotificationEvent += Connection_OnNotificationEvent;
                    subconn.OnSubscriptionError += Connection_OnSubscriptionError;
                    subconn.OnDisconnect += Connection_OnDisconnect;
                    subconn.Open();
                }
                catch
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        Status = EProviderStatus.LoginError;
                        return;
                    }
                    else
                    {
                        Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                    }
                }
            } while ((subconn == null || !subconn.IsOpen) && !exitToken.IsCancellationRequested);
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
            Status = EProviderStatus.Syncronizing;

            do
            {
                try
                {
                    ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
                    FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };
                    folderView.PropertySet.Add(FolderSchema.WellKnownFolderName);

                    itemView.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Ascending);

                    string aqsQuery = $"from:(" +
                        $"\"{ApplicationSettings.EmailRecipients.FDLSystem}\"" +
                        $" OR \"{ApplicationSettings.EmailRecipients.FDL_CHK}\"" +
                        $" OR \"{ApplicationSettings.EmailRecipients.SAP}\"" +
                        $" OR \"fdl\"" +
                        $" OR \"SAP MAIL\"" +
                        $" OR \"Sistema FDL\"" +
                        $" OR \"fdl_chk\"" +
                        ")";

                    // try to get last week messages (high priority)
                    foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), aqsQuery + " received:>=lastweek", folderView, itemView))
                    {
                        if (exitToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!(item is EmailMessage))
                        {
                            continue;
                        }

                        EmailMessage message = EmailMessage.Bind(service, item.Id);
                        NotifyNewMessage(message);
                    }

                    // then all the other messages
                    foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), aqsQuery, folderView, itemView))
                    {
                        if (exitToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!(item is EmailMessage))
                        {
                            continue;
                        }

                        EmailMessage message = EmailMessage.Bind(service, item.Id);
                        NotifyNewMessage(message);
                    }

                    IsSynced = true;
                    Status = EProviderStatus.Syncronized;
                }
                catch
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        Status = EProviderStatus.LoginError;
                        return;
                    }
                    else
                    {
                        Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                    }
                }
            } while (!IsSynced && !exitToken.IsCancellationRequested);
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
            StreamingSubscriptionConnection connection = sender as StreamingSubscriptionConnection;

            try
            {
                connection.Open();
            }
            catch (Exception)
            {
                if (Status != EProviderStatus.Error)
                {
                    Status = EProviderStatus.Offline;
                }

                connection.Dispose();
                Connect();
            }
        }

        private void Connection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
        {
            Debugger.Break();

            StreamingSubscriptionConnection connection = sender as StreamingSubscriptionConnection;

            if (!connection.IsOpen)
            {
                connection.Close();
            }

            connection.Dispose();
            Status = EProviderStatus.Error;
            Connect();
        }
        #endregion

        #region Private Methods
        private void Wait(int milliseconds)
        {
            try
            {
                System.Threading.Tasks.Task.Delay(milliseconds, exitToken.Token).Wait();
            }
            catch { }
        }

        private IEnumerable<Item> FindItemsInSubfolders(ExchangeService service, FolderId root, string query, FolderView folderView, ItemView itemView)
        {
            FindFoldersResults foldersResults;
            FindItemsResults<Item> itemsResults;

            do
            {
                if (exitToken.IsCancellationRequested)
                {
                    break;
                }

                foldersResults = service.FindFolders(root, folderView);

                foreach (Folder folder in foldersResults)
                {
                    if (exitToken.IsCancellationRequested)
                    {
                        break;
                    }

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
                    {
                        continue;
                    }

                    do
                    {
                        if (exitToken.IsCancellationRequested)
                        {
                            break;
                        }

                        itemsResults = service.FindItems(folder.Id, query, itemView);

                        foreach (Item item in itemsResults)
                        {
                            yield return item;
                        }

                        if (itemsResults.MoreAvailable)
                        {
                            itemView.Offset += itemView.PageSize;
                        }
                    } while (itemsResults.MoreAvailable);
                }

                if (foldersResults.MoreAvailable)
                {
                    folderView.Offset += folderView.PageSize;
                }
            } while (foldersResults.MoreAvailable);

            // reset the offset for a new search in current folder
            itemView.Offset = 0;

            do
            {
                if (exitToken.IsCancellationRequested)
                {
                    break;
                }

                itemsResults = service.FindItems(root, query, itemView);

                foreach (Item item in itemsResults)
                {
                    yield return item;
                }

                if (itemsResults.MoreAvailable)
                {
                    itemView.Offset += itemView.PageSize;
                }
            } while (itemsResults.MoreAvailable);
        }

        protected void NotifyNewMessage(EmailMessage e)
        {
            OnNewMessage?.Invoke(this, new NewMessageEventArgs(e));
        }

        protected void NotifyMessageSent(EmailMessageDTO e)
        {
            OnMessageSent?.Invoke(this, new MessageEventArgs(e));
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
            if (mainThread == null || !mainThread.IsAlive)
            {
                mainThread = new Thread(MainThread);
                mainThread.Name = "Exchange Autodiscover Thread";
                mainThread.IsBackground = true;
                mainThread.Start();
            }
        }

        public void Disconnect()
        {
            if (subconn != null)
            {
                try
                {
                    if (subconn.IsOpen)
                    {
                        subconn.Close();
                    }

                    subconn.Dispose();
                }
                catch { }
                finally
                {
                    subconn = null;
                }
            }

            if (exitToken != null)
            {
                try
                {
                    exitToken.Cancel(false);
                }
                catch { }
            }

            if (mainThread != null && !mainThread.Join(3000))
            {
                try
                {
                    Debugger.Break();
                    mainThread.Abort();
                }
                catch { }
            }

            if (emailSenderThread != null && !emailSenderThread.Join(3000))
            {
                try
                {
                    Debugger.Break();
                    emailSenderThread.Abort();
                }
                catch { }
            }

            if (subscribeThread != null && !subscribeThread.Join(3000))
            {
                try
                {
                    Debugger.Break();
                    subscribeThread.Abort();
                }
                catch { }
            }

            if (syncThread != null && !syncThread.Join(3000))
            {
                try
                {
                    Debugger.Break();
                    syncThread.Abort();
                }
                catch { }
            }

            mainThread = null;
            subscribeThread = null;
            syncThread = null;

            try
            {
                exitToken.Dispose();
            }
            catch { }
            finally
            {
                exitToken = new CancellationTokenSource();
            }
        }

        public NameResolutionCollection ResolveName(string filter)
        {
            if (exService.Url != null)
            {
                return exService.ResolveName(filter, ResolveNameSearchLocation.ContactsThenDirectory, true);
            }
            else
            {
                return null;
            }
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

        public bool IsServiceAvailable()
        {
            try
            {
                if (exServiceUri == null)
                {
                    return false;
                }

                var request = (HttpWebRequest)WebRequest.Create(exServiceUri.Scheme + "://" + exServiceUri.Host);
                request.UserAgent = ApplicationSettings.General.UserAgent;
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 100;
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
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

    public class MessageEventArgs : EventArgs
    {
        public EmailMessageDTO Message { get; internal set; }

        public MessageEventArgs(EmailMessageDTO message)
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
            if (traceMessage.Contains("(401)"))
            {
                Result = ETraceResult.LoginError;
            }

            if (traceMessage.Contains("No matching Autodiscover DNS SRV records were found."))
            {
                Result = ETraceResult.AutodiscoverError;
            }
        }
    }
}
