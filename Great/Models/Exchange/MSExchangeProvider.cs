﻿using GalaSoft.MvvmLight.Messaging;
using Great2.Models.DTO;
using Great2.Models.Interfaces;
using Great2.Utils;
using Great2.Utils.Messages;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Identity.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Linq;
using static Great2.Models.ExchangeTraceListener;
using NLog;

namespace Great2.Models
{
    //La libreria Microsoft EWS (https://github.com/OfficeDev/ews-managed-api) è deprecata
    //il pacchetto Nuget non viene aggiornato e la versione su github è piu aggiornata e con parecchi bugs corretti, di conseguenza la libreria è stata ricompilata a mano e aggiunta alle reference del progetto.

    public class MSExchangeProvider : IProvider
    {
        private NLog.Logger log = LogManager.GetCurrentClassLogger();

        private ExchangeService exService;
        private StreamingSubscriptionConnection subconn;
        private CancellationTokenSource exitToken;

        public event EventHandler<NewMessageEventArgs> OnNewMessage;
        public event EventHandler<MessageEventArgs> OnMessageSent;

        private Thread mainThread;

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
            exService.Url = new Uri(ApplicationSettings.General.Outlook365ewsEndpoint);

            // trick for display the Oauth login form after application startup
            Thread.Sleep(ApplicationSettings.General.WaitForApplicationStartup);

            string token = string.Empty;

            do
            {
                try
                {
                    if(IsServiceAvailable())
                        token = GetAuthenticationToken();                    
                }
                catch (Exception ex)
                {
                    Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                }
            } while (string.IsNullOrEmpty(token) && !exitToken.IsCancellationRequested);

            exService.Credentials = new OAuthCredentials(token);
            
            if (exitToken.IsCancellationRequested)
                return;

            Status = EProviderStatus.Connecting;

            // Cache user display name
            if (string.IsNullOrEmpty(UserSettings.Email.CachedDisplayName))
            {
                string name = GetUserDisplayName(UserSettings.Email.EmailAddress);

                if (name != null && name != string.Empty)
                    UserSettings.Email.CachedDisplayName = name;
            }

            SubscribeNotifications(exService, trace);

            ExchangeSync(exService, trace);

            while (!exitToken.IsCancellationRequested)
            {
                EmailSender(exService, trace);
                Wait(ApplicationSettings.General.WaitForNextEmailCheck);
            }
        }

        private void EmailSender(ExchangeService service, ExchangeTraceListener trace)
        {   
            lock(service)
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
                                Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                        }
                    }
                    while (!IsSent);
                }
            }
        }

        private void SubscribeNotifications(ExchangeService service, ExchangeTraceListener trace)
        {
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
                catch (Exception ex)
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        Status = EProviderStatus.LoginError;
                        return;
                    }
                    else
                        Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                }
            } while ((subconn == null || !subconn.IsOpen) && !exitToken.IsCancellationRequested);
        }

        private void ExchangeSync(ExchangeService service, ExchangeTraceListener trace)
        {
            bool IsSynced = false;
            Status = EProviderStatus.Syncronizing;

            do
            {
                try
                {
                    ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
                    itemView.PropertySet.Add(ItemSchema.DateTimeReceived);
                    itemView.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);                    

                    FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };
                    folderView.PropertySet.Add(FolderSchema.WellKnownFolderName);

                    SearchFilter.IsEqualTo f1 = new SearchFilter.IsEqualTo(EmailMessageSchema.From, new EmailAddress(ApplicationSettings.EmailRecipients.FDLSystem));
                    // In order to import FDL and EA files, the only way is to work around the "from" address check.
                    // To do this we check if in the recipient fdl_chk is present and, at a later time, we will check if the sender is the FDL System
                    SearchFilter.ContainsSubstring f2 = new SearchFilter.ContainsSubstring(ItemSchema.DisplayTo, ApplicationSettings.EmailRecipients.FDL_CHK_Display, ContainmentMode.Substring, ComparisonMode.IgnoreCase);
                    SearchFilter.SearchFilterCollection compoundFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, f1, f2);

                    lock(service)
                    {
                        foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), compoundFilter, folderView, itemView))
                        {
                            if (exitToken.IsCancellationRequested) break;

                            if (!(item is EmailMessage))
                                continue;

                            EmailMessage message = EmailMessage.Bind(service, item.Id);

                            // Double check in order to avoiding wrong fdl import (bugfix check the commment above) 
                            if (message.From.Address.ToLower() == ApplicationSettings.EmailRecipients.FDLSystem.ToLower())
                                NotifyNewMessage(message);
                        }
                    }

                    IsSynced = true;
                    Status = EProviderStatus.Syncronized;
                }
                catch (Exception ex)
                {
                    if (trace.Result == ETraceResult.LoginError)
                    {
                        Status = EProviderStatus.LoginError;
                        return;
                    }
                    else
                        Wait(ApplicationSettings.General.WaitForNextConnectionRetry);
                }
            } while (!IsSynced && !exitToken.IsCancellationRequested);
        }
        #endregion

        #region Subscription Events Handling
        private void Connection_OnNotificationEvent(object sender, NotificationEventArgs args)
        {
            lock(exService)
            {
                foreach (NotificationEvent e in args.Events)
                {
                    try
                    {
                        var itemEvent = (ItemEvent)e;
                        EmailMessage message = EmailMessage.Bind(args.Subscription.Service, itemEvent.ItemId);

                        switch (e.EventType)
                        {
                            case EventType.NewMail:
                                if (message.From.Address == ApplicationSettings.EmailRecipients.FDLSystem || message.DisplayTo == ApplicationSettings.EmailRecipients.FDL_CHK_Display)
                                    NotifyNewMessage(message);
                                break;

                            default:
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        log.Error(ex, "Connection_OnNotificationEvent()");
                    }
                }
            }
        }

        private void Connection_OnDisconnect(object sender, SubscriptionErrorEventArgs args)
        {
            lock(exService)
            {
                StreamingSubscriptionConnection connection = sender as StreamingSubscriptionConnection;

                try
                {
                    connection.Open();
                }
                catch (Exception)
                {
                    if (Status != EProviderStatus.Error)
                        Status = EProviderStatus.Offline;

                    connection.Dispose();
                    Reconnect();
                }
            }
        }

        private void Connection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
        {
            lock(exService)
            {
                Debugger.Break();

                StreamingSubscriptionConnection connection = sender as StreamingSubscriptionConnection;

                if (!connection.IsOpen)
                    connection.Close();

                connection.Dispose();
                Status = EProviderStatus.Error;
                Reconnect();
            }
        }
        #endregion

        #region Private Methods

        private string GetAuthenticationToken()
        {
            string token = string.Empty;

            var Authentication = System.Threading.Tasks.Task.Run(async () =>
            {
                IPublicClientApplication _clientApp = PublicClientApplicationBuilder
                        .Create(ApplicationSettings.General.MSALClientId)
                        .WithAuthority(AzureCloudInstance.AzurePublic, ApplicationSettings.General.MSALTenant)
                        .WithDefaultRedirectUri()
                        .Build();

                MsalTokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);

                #region Force Logoff (debug test)
                //var accounts = await _clientApp.GetAccountsAsync();
                //if (accounts.Any())
                //{
                //    try
                //    {
                //        await _clientApp.RemoveAsync(accounts.FirstOrDefault());
                //    }
                //    catch (MsalException ex)
                //    {
                //        Debug.WriteLine($"Error signing-out user: {ex.Message}");
                //    }
                //}
                #endregion

                List<string> scopes = new List<string>();
                scopes.Add("https://outlook.office.com/EWS.AccessAsUser.All");

                MsalAuthenticationProvider provider = new MsalAuthenticationProvider(_clientApp, scopes.ToArray());
                token = await provider.GetTokenAsync();
            });

            System.Threading.Tasks.Task.WaitAll(Authentication);

            return token;
        }

        private void Wait(int milliseconds)
        {
            try
            {
                System.Threading.Tasks.Task.Delay(milliseconds, exitToken.Token).Wait();
            }
            catch { }
        }

        private IEnumerable<Item> FindItemsInSubfolders(ExchangeService service, FolderId root, SearchFilter filters, FolderView folderView, ItemView itemView)
        {
            FindFoldersResults foldersResults;
            FindItemsResults<Item> itemsResults;

            do
            {
                if (exitToken.IsCancellationRequested) break;

                foldersResults = service.FindFolders(root, folderView);

                foreach (Folder folder in foldersResults)
                {
                    if (exitToken.IsCancellationRequested) break;

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
                        if (exitToken.IsCancellationRequested) break;

                        itemsResults = service.FindItems(folder.Id, filters, itemView);

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
                if (exitToken.IsCancellationRequested) break;

                itemsResults = service.FindItems(root, filters, itemView);

                foreach (Item item in itemsResults)
                    yield return item;

                if (itemsResults.MoreAvailable)
                    itemView.Offset += itemView.PageSize;

            } while (itemsResults.MoreAvailable);
        }

        private IEnumerable<Item> FindItemsInSubfolders(ExchangeService service, FolderId root, string AQSQuery, FolderView folderView, ItemView itemView)
        {
            FindFoldersResults foldersResults;
            FindItemsResults<Item> itemsResults;

            do
            {
                if (exitToken.IsCancellationRequested) break;

                foldersResults = service.FindFolders(root, folderView);

                foreach (Folder folder in foldersResults)
                {
                    if (exitToken.IsCancellationRequested) break;

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
                        if (exitToken.IsCancellationRequested) break;

                        itemsResults = service.FindItems(folder.Id, AQSQuery, itemView);

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
                if (exitToken.IsCancellationRequested) break;

                itemsResults = service.FindItems(root, AQSQuery, itemView);

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

        protected void NotifyMessageSent(EmailMessageDTO e)
        {
            OnMessageSent?.Invoke(this, new MessageEventArgs(e));
        }
        #endregion

        #region Public Methods
        public void Reconnect()
        {
            Disconnect();
            Connect();
        }
        
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
                        subconn.Close();

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

            mainThread = null;

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
            lock(exService)
            {
                if (exService.Url != null)
                    return exService.ResolveName(filter, ResolveNameSearchLocation.ContactsThenDirectory, true);
                else
                    return null;
            }
        }

        public string GetUserDisplayName(string email)
        {
            lock (exService)
            {
                try
                {
                    NameResolutionCollection ncCol = exService.ResolveName(email, ResolveNameSearchLocation.DirectoryOnly, true);
                    return ncCol[0].Contact.DisplayName;
                }
                catch { }

                return string.Empty;
            }
        }

        public bool SendEmail(EmailMessageDTO message)
        {
            emailQueue.Enqueue(message);
            return true;
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
            lock(exService)
            {
                try
                {
                    if (exService.Url == null)
                        return false;

                    var request = (HttpWebRequest)WebRequest.Create(exService.Url.Scheme + "://" + exService.Url.Host);
                    request.UserAgent = ApplicationSettings.General.UserAgent;
                    request.KeepAlive = false;
                    request.AllowAutoRedirect = true;
                    request.MaximumAutomaticRedirections = 100;
                    request.CookieContainer = new CookieContainer();
                    request.Method = "GET";

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                            return true;
                        else
                            return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
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
                Result = ETraceResult.LoginError;

            if (traceMessage.Contains("No matching Autodiscover DNS SRV records were found."))
                Result = ETraceResult.AutodiscoverError;
        }
    }
}
