using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Utils;
using Great.Utils.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
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
    public class ExpenseAccountViewModel : ViewModelBase
    {
        #region Properties
        public int NotesMaxLength { get { return ApplicationSettings.ExpenseAccount.NotesMaxLength; } }

        private FDLManager _fdlManager;
        private DBArchive _db;

        /// <summary>
        /// The <see cref="IsInputEnabled" /> property's name.
        /// </summary>
        private bool _isInputEnabled = false;

        /// <summary>
        /// Sets and gets the IsInputEnabled property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public bool IsInputEnabled
        {
            get
            {
                return _isInputEnabled;
            }

            set
            {
                if (_isInputEnabled == value)
                {
                    return;
                }

                var oldValue = _isInputEnabled;
                _isInputEnabled = value;

                RaisePropertyChanged(nameof(IsInputEnabled), oldValue, value);
                SaveCommand.RaiseCanExecuteChanged();
                ClearCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="ExpenseAccounts" /> property's name.
        /// </summary>
        private ObservableCollectionEx<ExpenseAccount> _expenseAccounts;

        /// <summary>
        /// Sets and gets the ExpenseAccounts property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollectionEx<ExpenseAccount> ExpenseAccounts
        {
            get
            {
                return _expenseAccounts;
            }
            set
            {
                _expenseAccounts = value;
                RaisePropertyChanged(nameof(ExpenseAccounts), true);
            }
        }

        /// <summary>
        /// The <see cref="SelectedEA" /> property's name.
        /// </summary>
        private ExpenseAccount _selectedEA;

        /// <summary>
        /// Sets and gets the SelectedEA property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public ExpenseAccount SelectedEA
        {
            get
            {
                return _selectedEA;
            }

            set
            {
                var oldValue = _selectedEA;
                _selectedEA = value;

                RefreshExpenses();

                //SelectedEAClone = _selectedEA?.Clone();

                if (_selectedEA != null)
                {
                    SelectedExpense = null;
                    IsInputEnabled = true;
                }
                else
                    IsInputEnabled = false;

                RaisePropertyChanged(nameof(SelectedEA), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedEAClone" /> property's name.
        /// </summary>
        private ExpenseAccount _selectedEAClone;

        /// <summary>
        /// Sets and gets the SelectedEAClone property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public ExpenseAccount SelectedEAClone
        {
            get
            {
                return _selectedEAClone;
            }

            set
            {
                var oldValue = _selectedEAClone;
                _selectedEAClone = value;
                RaisePropertyChanged(nameof(SelectedEAClone), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedExpense" /> property's name.
        /// </summary>
        private Expense _selectedExpense;

        /// <summary>
        /// Sets and gets the SelectedExpense property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public Expense SelectedExpense
        {
            get
            {
                return _selectedExpense;
            }

            set
            {
                var oldValue = _selectedExpense;
                _selectedExpense = value;

                RaisePropertyChanged(nameof(SelectedExpense), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="Expenses" /> property's name.
        /// </summary>
        private IList<Expense> _expenses;

        /// <summary>
        /// Sets and gets the Timesheets property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public IList<Expense> Expenses
        {
            get
            {
                return _expenses;
            }
            internal set
            {
                _expenses = value;
                RaisePropertyChanged(nameof(Expenses));
            }
        }

        /// <summary>
        /// The <see cref="ExpenseTypes" /> property's name.
        /// </summary>
        public ObservableCollection<ExpenseType> ExpenseTypes { get; set; }        
        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<ExpenseAccount> SaveCommand { get; set; }

        public RelayCommand<ExpenseAccount> SendToSAPCommand { get; set; }
        public RelayCommand<ExpenseAccount> SendByEmailCommand { get; set; }
        public RelayCommand<ExpenseAccount> SaveAsCommand { get; set; }
        public RelayCommand<ExpenseAccount> OpenCommand { get; set; }
        public RelayCommand<ExpenseAccount> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<ExpenseAccount> MarkAsCancelledCommand { get; set; }
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the ExpenseAccountViewModel class.
        /// </summary>
        public ExpenseAccountViewModel(FDLManager manager, DBArchive db)
        {
            _db = db;
            _fdlManager = manager;

            ClearCommand = new RelayCommand(ClearEA, () => { return IsInputEnabled; });
            SaveCommand = new RelayCommand<ExpenseAccount>(SaveEA, (ExpenseAccount ea) => { return IsInputEnabled; });

            //SendToSAPCommand = new RelayCommand<FDL>(SendToSAP);
            //SendByEmailCommand = new RelayCommand<FDL>(SendByEmail);
            //SaveAsCommand = new RelayCommand<FDL>(SaveAs);
            //OpenCommand = new RelayCommand<FDL>(Open);
            //MarkAsAcceptedCommand = new RelayCommand<FDL>(MarkAsAccepted);
            //MarkAsCancelledCommand = new RelayCommand<FDL>(MarkAsCancelled);

            ExpenseTypes = new ObservableCollection<ExpenseType>(_db.ExpenseTypes);
            ExpenseAccounts = new ObservableCollectionEx<ExpenseAccount>(_db.ExpenseAccounts);

            ExpenseAccounts.ItemPropertyChanged += ExpenseAccounts_ItemPropertyChanged;

            MessengerInstance.Register<NewItemMessage<ExpenseAccount>>(this, NewEA);
            MessengerInstance.Register<ItemChangedMessage<ExpenseAccount>>(this, EAChanged);
        }

        private void ExpenseAccounts_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            if (SelectedEA != null && SelectedEA.Id == ExpenseAccounts[e.CollectionIndex].Id)
                RefreshExpenses();
        }

        private void RefreshExpenses()
        {
            if (SelectedEA != null && SelectedEA.Expenses != null)
                Expenses = SelectedEA.Expenses.ToList();//.OrderBy(t => t.Date).ToList();
            else
                Expenses = null;
        }

        public void NewEA(NewItemMessage<ExpenseAccount> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        ExpenseAccount ea = _db.ExpenseAccounts.SingleOrDefault(e => e.Id == item.Content.Id);

                        if (ea != null && !ExpenseAccounts.Contains(ea))
                            ExpenseAccounts.Add(ea);
                    }
                })
            );
        }

        public void EAChanged(ItemChangedMessage<ExpenseAccount> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        //_db.ExpenseAccounts.AddOrUpdate(item.Content);
                        //_db.SaveChanges();

                        //ExpenseAccounts.SingleOrDefault(e => e.Id == item.Content.Id)?.NotifyFDLPropertiesChanged();
                    }
                })
            );
        }

        public void ClearEA()
        {

        }

        public void SaveEA(ExpenseAccount ea)
        {

        }
    }
}