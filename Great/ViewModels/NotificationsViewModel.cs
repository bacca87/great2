using GalaSoft.MvvmLight;
using Great.Models;
using Great.Models.Database;
using Great.Utils.Messages;
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
            get
            {
                return _newFactoriesCount;
            }

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
            get
            {
                return _newFDLCount;
            }

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
            get
            {
                return _newExpenseAccountsCount;
            }

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
        private EExchangeStatus _exchangeStatus = 0;

        /// <summary>
        /// Sets and gets the ExchangeStatus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public EExchangeStatus ExchangeStatus
        {
            get
            {
                return _exchangeStatus;
            }

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
            using (DBArchive db = new DBArchive())
            {
                NewFactoriesCount = db.Factories.Count(factory => factory.NotifyAsNew);
                NewFDLCount = db.FDLs.Count(fdl => fdl.NotifyAsNew);
                NewExpenseAccountsCount = db.ExpenseAccounts.Count(ea => ea.NotifyAsNew);
            }

            MessengerInstance.Register(this, (NewItemMessage<FDLEVM> x) => NewFDLCount++);
            MessengerInstance.Register(this, (NewItemMessage<Factory> x) => NewFactoriesCount++);
            MessengerInstance.Register(this, (NewItemMessage<ExpenseAccountEVM> x) => NewExpenseAccountsCount++);
            MessengerInstance.Register<StatusChangeMessage<EExchangeStatus>>(this, OnExchangeStatusChange);
        }

        private void OnExchangeStatusChange(StatusChangeMessage<EExchangeStatus> x)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background, new Action(() => 
            {
                ExchangeStatus = x.Content;

                if (ExchangeStatus == EExchangeStatus.LoginError)
                {
                    ExchangeLoginView loginView = new ExchangeLoginView();
                    loginView.ShowDialog();
                }
            }));
        }

        //TODO: aggiungere le notifiche baloon
        // https://github.com/raflop/ToastNotifications
        // https://github.com/zachatrocity/netoaster
    }
}