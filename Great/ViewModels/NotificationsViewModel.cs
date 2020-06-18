using GalaSoft.MvvmLight;
using Great2.Models.Database;
using Great2.Models.Interfaces;
using Great2.Utils.Messages;
using Great2.Utils;
using Great2.ViewModels.Database;
using Great2.Views.Dialogs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Great2.Models.DTO;
using Great2.Models;

namespace Great2.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NotificationsViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// The <see cref="NewFactoriesCount" /> property's name.
        /// </summary>
        private int _newFactoriesCount = 0;

        /// <summary>
        /// Sets and gets the NewFactoriesCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int NewFactoriesCount
        {
            get => _newFactoriesCount;

            set
            {
                var oldValue = _newFactoriesCount;
                _newFactoriesCount = value;

                RaisePropertyChanged(nameof(NewFactoriesCount), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="NewFDLCount" /> property's name.
        /// </summary>
        private int _newFDLCount = 0;

        /// <summary>
        /// Sets and gets the NewFDLCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int NewFDLCount
        {
            get => _newFDLCount;

            set
            {
                var oldValue = _newFDLCount;
                _newFDLCount = value;

                RaisePropertyChanged(nameof(NewFDLCount), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="NewExpenseAccountsCount" /> property's name.
        /// </summary>
        private int _newExpenseAccountsCount = 0;

        /// <summary>
        /// Sets and gets the NewExpenseAccountsCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int NewExpenseAccountsCount
        {
            get => _newExpenseAccountsCount;

            set
            {
                var oldValue = _newExpenseAccountsCount;
                _newExpenseAccountsCount = value;

                RaisePropertyChanged(nameof(NewExpenseAccountsCount), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="ExchangeStatus" /> property's name.
        /// </summary>
        private EProviderStatus _exchangeStatus = 0;

        /// <summary>
        /// Sets and gets the ExchangeStatus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public EProviderStatus ExchangeStatus
        {
            get => _exchangeStatus;

            set
            {
                var oldValue = _exchangeStatus;
                _exchangeStatus = value;

                RaisePropertyChanged(nameof(ExchangeStatus), oldValue, value);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the NotificationsViewModel class.
        /// </summary>
        public NotificationsViewModel()
        {
            RefreshTotals();

            MessengerInstance.Register<NewItemMessage<EventEVM>>(this, OnEventImported);
            MessengerInstance.Register<NewItemMessage<FDLEVM>>(this, OnFDLReceived);
            MessengerInstance.Register<NewItemMessage<FactoryEVM>>(this, OnNewFactory);
            MessengerInstance.Register<NewItemMessage<ExpenseAccountEVM>>(this, OnEAReceived);// => { if (x.Content.NotifyAsNew) { NewExpenseAccountsCount++; } });

            MessengerInstance.Register<ItemChangedMessage<FDLEVM>>(this, OnFDLChanged);
            MessengerInstance.Register<ItemChangedMessage<ExpenseAccountEVM>>(this, OnEAChanged);
            MessengerInstance.Register<ItemChangedMessage<EventEVM>>(this, OnEventChanged);
            MessengerInstance.Register(this, (ItemChangedMessage<FactoryEVM> x) => { using (DBArchive db = new DBArchive()) { NewFactoriesCount = db.Factories.Count(factory => factory.NotifyAsNew); } });
            MessengerInstance.Register<ProviderEmailSentMessage<EmailMessageDTO>>(this, OnEmailSent);

            MessengerInstance.Register<StatusChangeMessage<EProviderStatus>>(this, OnExchangeStatusChange);
        }

        private void RefreshTotals()
        {
            using (DBArchive db = new DBArchive())
            {
                NewFactoriesCount = db.Factories.Count(factory => factory.NotifyAsNew);
                NewFDLCount = db.FDLs.Count(fdl => fdl.NotifyAsNew);
                NewExpenseAccountsCount = db.ExpenseAccounts.Count(ea => ea.NotifyAsNew);
            }
        }

        private void OnExchangeStatusChange(StatusChangeMessage<EProviderStatus> x)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ExchangeStatus = x.Content;

                // no more needed with OAuth2
                //if (ExchangeStatus == EProviderStatus.LoginError)
                //{
                //    ExchangeLoginView loginView = new ExchangeLoginView();
                //    loginView.ShowDialog();
                //}
            }));
        }

        private void OnFDLReceived(NewItemMessage<FDLEVM> fdl)
        {
            if (fdl.Content.NotifyAsNew)
                NewFDLCount++;

            if(!fdl.Content.IsVirtual)
                ToastNotificationHelper.SendToastNotification("New FDL Received", fdl.Content.Id, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04 );
        }

        private void OnFDLChanged(ItemChangedMessage<FDLEVM> fdl)
        {
            using (DBArchive db = new DBArchive()) 
                NewFDLCount = db.FDLs.Count(f => f.NotifyAsNew);

            if (!fdl.Content.IsVirtual && fdl.Sender is FDLManager)
                ToastNotificationHelper.SendToastNotification($"FDL {Enum.GetName(typeof(EFDLStatus), fdl.Content.EStatus)}", fdl.Content.Id, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }

        private void OnEAReceived(NewItemMessage<ExpenseAccountEVM> ea)
        {
            if (ea.Content.NotifyAsNew)
                NewExpenseAccountsCount++;

            if (!ea.Content.IsVirtual)
                ToastNotificationHelper.SendToastNotification("New Expense Account Received", ea.Content.FDL, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }

        private void OnEAChanged(ItemChangedMessage<ExpenseAccountEVM> ea)
        {
            using (DBArchive db = new DBArchive())
                NewExpenseAccountsCount = db.ExpenseAccounts.Count(e => e.NotifyAsNew);

            if (!ea.Content.IsVirtual && ea.Sender is FDLManager)
                ToastNotificationHelper.SendToastNotification($"Expense Account {Enum.GetName(typeof(EFDLStatus), ea.Content.EStatus)}", ea.Content.FDL, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }

        private void OnNewFactory(NewItemMessage<FactoryEVM> factory)
        {
            if (factory.Content.NotifyAsNew)
                NewFactoriesCount++;

            ToastNotificationHelper.SendToastNotification("New Factory Added", factory.Content.Name, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }

        private void OnEventImported(NewItemMessage<EventEVM> ev)
        {
            ToastNotificationHelper.SendToastNotification("Event Imported", ev.Content.Title, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }

        private void OnEventChanged(ItemChangedMessage<EventEVM> ev)
        {
            using (var db = new DBArchive())
            {
                ToastNotificationHelper.SendToastNotification($"Event {Enum.GetName(typeof(EEventStatus), ev.Content.EStatus)}", ev.Content.Title, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
            }
        }

        private void OnEmailSent(ProviderEmailSentMessage<EmailMessageDTO> mex)
        {
            ToastNotificationHelper.SendToastNotification("Email Sent", mex.Content.Subject, null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }
    }
}