using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great2.Models;
using Great2.Models.Database;
using Great2.Utils.Extensions;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Great2.ViewModels
{
    public class AddVirtualFDLViewModel : ViewModelBase
    {
        #region Properties        
        private string _Id;
        public string Id
        {
            get => _Id;
            set
            {
                Set(ref _Id, value);
                IsMonthEnabled = _Id != null && _Id.Length >= 4;
            }
        }

        private Month _month;
        public Month SelectedMonth
        {
            get => _month;
            set
            {
                Set(ref _month, value);

                if (Id != null && Id.Length >= 4)
                {
                    Weeks = new ObservableCollection<int>(TimesheetsViewModel.AllDatesInMonth(Convert.ToInt32(Id.Substring(0, 4)), (int)_month).Select(d => d.WeekNr()).Distinct());
                    IsWeekEnabled = true;
                }
                else
                    IsWeekEnabled = false;
            }
        }

        private int _SelectedWeekNr;
        public int SelectedWeekNr
        {
            get => _SelectedWeekNr;
            set => Set(ref _SelectedWeekNr, value);
        }

        private int _Order;
        public int Order
        {
            get => _Order;
            set => Set(ref _Order, value);
        }

        private bool _IsExtra;
        public bool IsExtra
        {
            get => _IsExtra;
            set => Set(ref _IsExtra, value);
        }

        private ObservableCollection<int> _Weeks;
        public ObservableCollection<int> Weeks
        {
            get => _Weeks;
            set => Set(ref _Weeks, value);
        }

        private bool _IsMonthEnabled;
        public bool IsMonthEnabled
        {
            get => _IsMonthEnabled;
            set
            {
                Set(ref _IsMonthEnabled, value);

                // hack for update the weeks list
                if (_IsMonthEnabled)
                    SelectedMonth = SelectedMonth; 
            }
        }

        private bool _IsWeekEnabled;
        public bool IsWeekEnabled
        {
            get => _IsWeekEnabled;
            set => Set(ref _IsWeekEnabled, value);
        }

        private FDLManager _fdlManager;

        public Action OnFDLSaved { get; set; }
        #endregion

        #region Commands Definitions
        public RelayCommand AddCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public AddVirtualFDLViewModel(FDLManager manager)
        {
            _fdlManager = manager;
            AddCommand = new RelayCommand(AddVirtualFDL);

            ResetFields();
        }

        private void AddVirtualFDL()
        {
            if (Id == null || Id.Length != 10)
            {
                MetroMessageBox.Show("Invalid FDL number!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(Convert.ToInt32(Id.Substring(0,4)) < ApplicationSettings.Timesheets.MinYear)
            {
                MetroMessageBox.Show($"Invalid FDL year! The year must be greater or equal than {ApplicationSettings.Timesheets.MinYear}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedWeekNr < 1 || SelectedWeekNr > 52)
            {
                MetroMessageBox.Show($"Invalid week number! Please select a valid week.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (DBArchive db = new DBArchive())
            {
                if(db.FDLs.Any(fdl => fdl.Id == Id))
                {
                    MetroMessageBox.Show($"The inserted FDL number already exist in the database! Operation cancelled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            long StartDay = TimesheetsViewModel.AllDatesInMonth(Convert.ToInt32(Id.Substring(0, 4)), (int)SelectedMonth).Where(d => d.WeekNr() == SelectedWeekNr).Select(d => d.ToUnixTimestamp()).FirstOrDefault();

            if (_fdlManager.CreateVirtualFdl(Id, StartDay, SelectedWeekNr, Order, IsExtra))
            {                
                OnFDLSaved?.Invoke();
                ResetFields();
            }
        }

        private void ResetFields()
        {
            Id = string.Empty;
            SelectedMonth = Month.January;
            Weeks = new ObservableCollection<int>();
            SelectedWeekNr = 0;
            Order = 0;
            IsExtra = false;
        }
    }
}