using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using System.ComponentModel;
using System.Linq;

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

        private DBEntities _db;
        #endregion

        /// <summary>
        /// Initializes a new instance of the NotificationsViewModel class.
        /// </summary>
        public NotificationsViewModel(DBEntities db)
        {
            _db = db;
            
            NewFactoriesCount = _db.Factories.Count(factory => factory.NotifyAsNew);
            NewFDLCount = _db.FDLs.Count(fdl => fdl.NotifyAsNew);
            NewExpenseAccountsCount = _db.ExpenseAccounts.Count(ea => ea.NotifyAsNew);
            
            MessengerInstance.Register(this, (PropertyChangedMessage<BindingList<Factory>> p) => { NewFactoriesCount = p.NewValue.Count(factory => factory.NotifyAsNew); });
            MessengerInstance.Register(this, (PropertyChangedMessage<BindingList<FDL>> p) => { NewFDLCount = p.NewValue.Count(fdl => fdl.NotifyAsNew); });
            MessengerInstance.Register(this, (PropertyChangedMessage<BindingList<ExpenseAccount>> p) => { NewExpenseAccountsCount = p.NewValue.Count(ea => ea.NotifyAsNew); });
        }

        //TODO: aggiungere le notifiche baloon
        // https://github.com/raflop/ToastNotifications
        // https://github.com/zachatrocity/netoaster
    }
}