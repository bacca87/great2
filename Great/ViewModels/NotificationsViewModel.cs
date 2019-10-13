using GalaSoft.MvvmLight;
using Great.Models.Database;
using Great.Models.Interfaces;
using Great.Utils.Messages;
using Great.Utils;
using Great.ViewModels.Database;
using Great.Views.Dialogs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
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
            MessengerInstance.Register<NewItemMessage<FDLEVM>>(this,OnFdlReceived);
            MessengerInstance.Register(this, (NewItemMessage<FactoryEVM> x) => { if (x.Content.NotifyAsNew) { NewFactoriesCount++; } });
            MessengerInstance.Register(this, (NewItemMessage<ExpenseAccountEVM> x) => { if (x.Content.NotifyAsNew) { NewExpenseAccountsCount++; } });

            MessengerInstance.Register<ItemChangedMessage<FDLEVM>>(this, OnFdlChanged);
            MessengerInstance.Register<ItemChangedMessage<ExpenseAccountEVM>>(this, OnEaChanged);
            MessengerInstance.Register<ItemChangedMessage<EventEVM>>(this, OnEventChanged);
            MessengerInstance.Register(this, (ItemChangedMessage<FactoryEVM> x) => { using (DBArchive db = new DBArchive()) { NewFactoriesCount = db.Factories.Count(factory => factory.NotifyAsNew); } });

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

                if (ExchangeStatus == EProviderStatus.LoginError)
                {
                    ExchangeLoginView loginView = new ExchangeLoginView();
                    loginView.ShowDialog();
                }
            }));
        }

        private void OnFdlReceived(NewItemMessage<FDLEVM> fdl)
        {
            if (fdl.Content.NotifyAsNew)
                NewFDLCount++;

            ToastNotificationHelper.SendToastNotification("FDL received", $"FDL {fdl.Content.Id} received",null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04 );

        }

        private void OnFdlChanged(ItemChangedMessage<FDLEVM> fdl)
        {
            using (DBArchive db = new DBArchive()) 
                NewFDLCount = db.FDLs.Count(f => f.NotifyAsNew);

            if (fdl.Content.EStatus == Models.EFDLStatus.Accepted)
                ToastNotificationHelper.SendToastNotification("FDL Accepted", $"FDL {fdl.Content.Id} accepted", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);

            else if (fdl.Content.EStatus == Models.EFDLStatus.Rejected)
                ToastNotificationHelper.SendToastNotification("FDL Rejected", $"FDL {fdl.Content.Id} rejected", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);


        }
        private void OnEaChanged(ItemChangedMessage<ExpenseAccountEVM> fdl)
        {
            using (DBArchive db = new DBArchive())
                NewExpenseAccountsCount = db.ExpenseAccounts.Count(e => e.NotifyAsNew);

            if (fdl.Content.EStatus == Models.EFDLStatus.Accepted)
                ToastNotificationHelper.SendToastNotification("Expense Account accepted", $"Expense Account {fdl.Content.Id} received", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);

            else if (fdl.Content.EStatus == Models.EFDLStatus.Rejected)
                ToastNotificationHelper.SendToastNotification("Expense Account Rejected", $"Expense Account {fdl.Content.Id} rejected", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);

        }
        private void OnEventImported(NewItemMessage<EventEVM> ev)
        {
            ToastNotificationHelper.SendToastNotification("Event Imported", $"Event  {ev.Content.Title} imported", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);
        }
        private void OnEventChanged(ItemChangedMessage<EventEVM> ev)
        {
            if (ev.Content.EStatus == Models.EEventStatus.Accepted)
                ToastNotificationHelper.SendToastNotification("Event Approved", $"Event  {ev.Content.Title} Approved", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);

            else if (ev.Content.EStatus == Models.EEventStatus.Rejected)
                ToastNotificationHelper.SendToastNotification("Event Rejected", $"Event  {ev.Content.Title} Rejected", null, Windows.UI.Notifications.ToastTemplateType.ToastImageAndText04);


        }

    }
}