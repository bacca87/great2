using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using Great.Models.Database;
using Great.Utils;
using Great.Utils.Extensions;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Great.ViewModels
{
    public class TimesheetsViewModel : ViewModelBase
    {
        #region Properties
        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set
            {
                Set(ref _isInputEnabled, value);

                SaveTimesheetCommand.RaiseCanExecuteChanged();
                ClearTimesheetCommand.RaiseCanExecuteChanged();
                DeleteTimesheetCommand.RaiseCanExecuteChanged();
            }
        }

        private int _currentYear = DateTime.Now.Year;
        public int CurrentYear
        {
            get => _currentYear;
            set
            {
                bool updateDays = _currentYear != value;
                int year = 0;

                if (value < ApplicationSettings.Timesheets.MinYear)
                    year = ApplicationSettings.Timesheets.MinYear;
                else if (value > ApplicationSettings.Timesheets.MaxYear)
                    year = ApplicationSettings.Timesheets.MaxYear;
                else
                    year = value;

                Set(ref _currentYear, year);

                if (updateDays)
                    UpdateWorkingDays();
            }
        }

        private int _currentMonth = DateTime.Now.Month;
        public int CurrentMonth
        {
            get => _currentMonth;
            set => Set(ref _currentMonth, value);
        }

        private ObservableCollectionEx<DayEVM> _WorkingDays;
        public ObservableCollectionEx<DayEVM> WorkingDays
        {
            get => _WorkingDays;
            set => Set(ref _WorkingDays, value);
        }

        private DayEVM _selectedWorkingDay;
        public DayEVM SelectedWorkingDay
        {
            get => _selectedWorkingDay;
            set
            {
                Set(ref _selectedWorkingDay, value);

                if (_selectedWorkingDay != null)
                {
                    SelectedTimesheet = null;
                    CurrentMonth = _selectedWorkingDay.Date.Month;
                    IsInputEnabled = _selectedWorkingDay.EType != EDayType.SickLeave && _selectedWorkingDay.EType != EDayType.VacationDay;

                    using (DBArchive db = new DBArchive())
                    {
                        string year = CurrentYear.ToString(); // hack for query
                        FDLs = new ObservableCollection<FDLEVM>(db.FDLs.Where(fdl => fdl.Id.Substring(0, 4) == year && fdl.WeekNr == SelectedWorkingDay.WeekNr).ToList().Select(fdl => new FDLEVM(fdl)));
                    }
                    RaisePropertyChanged(nameof(FDLs));
                }
                else
                {
                    IsInputEnabled = false;
                }
            }
        }

        private TimesheetEVM _selectedTimesheet;
        public TimesheetEVM SelectedTimesheet
        {
            get => _selectedTimesheet;
            set
            {
                if (value == null)
                    value = SelectedWorkingDay != null ? new TimesheetEVM() { Timestamp = SelectedWorkingDay.Timestamp } : null;

                Set(ref _selectedTimesheet, value);
                DeleteTimesheetCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<FDLEVM> FDLs { get; set; }

        public Action<DayEVM> OnSelectFirstDayInMonth;
        public Action<DayEVM> OnSelectToday;
        #endregion

        #region Commands Definitions
        public RelayCommand NextYearCommand { get; set; }
        public RelayCommand PreviousYearCommand { get; set; }
        public RelayCommand<int> SelectFirstDayInMonthCommand { get; set; }
        public RelayCommand SelectTodayCommand { get; set; }

        public RelayCommand<DayEVM> SetVacationDayCommand { get; set; }
        public RelayCommand<DayEVM> SetSickLeaveCommand { get; set; }
        public RelayCommand<DayEVM> SetWorkDayCommand { get; set; }
        public RelayCommand<DayEVM> SetHomeWorkDayCommand { get; set; }
        public RelayCommand<DayEVM> SetVacationPaidDayCommand { get; set; }

        public RelayCommand<DayEVM> ResetDayCommand { get; set; }
        public RelayCommand<DayEVM> CopyDayCommand { get; set; }
        public RelayCommand<DayEVM> CutDayCommand { get; set; }
        public RelayCommand<DayEVM> PasteDayCommand { get; set; }

        public RelayCommand ClearTimesheetCommand { get; set; }
        public RelayCommand<TimesheetEVM> SaveTimesheetCommand { get; set; }
        public RelayCommand<TimesheetEVM> DeleteTimesheetCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public TimesheetsViewModel()
        {
            NextYearCommand = new RelayCommand(() => CurrentYear++);
            PreviousYearCommand = new RelayCommand(() => CurrentYear--);
            SelectFirstDayInMonthCommand = new RelayCommand<int>(SelectFirstDayInMonth);
            SelectTodayCommand = new RelayCommand(SelectToday);

            SetVacationDayCommand = new RelayCommand<DayEVM>(SetVacationDay);
            SetSickLeaveCommand = new RelayCommand<DayEVM>(SetSickLeave);
            SetWorkDayCommand = new RelayCommand<DayEVM>(SetWorkDay);
            SetHomeWorkDayCommand = new RelayCommand<DayEVM>(SetHomeWorkingDay);
            SetVacationPaidDayCommand = new RelayCommand<DayEVM>(SetVacationPaidDay);

            ResetDayCommand = new RelayCommand<DayEVM>(ResetDay);
            CopyDayCommand = new RelayCommand<DayEVM>(CopyDay);
            CutDayCommand = new RelayCommand<DayEVM>(CutDay);
            PasteDayCommand = new RelayCommand<DayEVM>(PasteDay);

            ClearTimesheetCommand = new RelayCommand(ClearTimesheet, () => { return IsInputEnabled; });
            SaveTimesheetCommand = new RelayCommand<TimesheetEVM>(SaveTimesheet, (TimesheetEVM timesheet) => { return IsInputEnabled; });
            DeleteTimesheetCommand = new RelayCommand<TimesheetEVM>(DeleteTimesheet, (TimesheetEVM timesheet) => { return IsInputEnabled; });

            UpdateWorkingDays();
        }

        private void UpdateWorkingDays()
        {
            ObservableCollectionEx<DayEVM> days = new ObservableCollectionEx<DayEVM>();

            using (DBArchive db = new DBArchive())
            {
                for (int month = 1; month <= 12; month++)
                {
                    foreach (DateTime day in AllDatesInMonth(CurrentYear, month))
                    {
                        long timestamp = day.ToUnixTimestamp();
                        Day currentDay = db.Days.SingleOrDefault(d => d.Timestamp == timestamp);

                        if (currentDay != null)
                            days.Add(new DayEVM(currentDay));
                        else
                            days.Add(new DayEVM { Date = day });
                    }
                }
            }

            WorkingDays = days;
        }

        public static IEnumerable<DateTime> AllDatesInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(year, month, day);
            }
        }

        public void SelectFirstDayInMonth(int month)
        {
            if (month > 0 && month <= 12)
            {
                SelectedWorkingDay = WorkingDays.Where(day => day.Date.Month == month && day.Date.Day == 1).FirstOrDefault();
                OnSelectFirstDayInMonth?.Invoke(SelectedWorkingDay);
            }
        }

        public void SelectToday()
        {
            CurrentYear = DateTime.Now.Year;
            //SelectFirstDayInMonth(DateTime.Now.Month);            
            SelectedWorkingDay = WorkingDays.Where(day => day.Date.DayOfYear == DateTime.Now.DayOfYear).FirstOrDefault();
            OnSelectToday?.Invoke(SelectedWorkingDay);
        }

        public void ClearTimesheet() => SelectedTimesheet = null;
        public void SetVacationDay(DayEVM day) => SetDayType(day, EDayType.VacationDay);
        public void SetSickLeave(DayEVM day) => SetDayType(day, EDayType.SickLeave);
        public void SetWorkDay(DayEVM day) => SetDayType(day, EDayType.WorkDay);
        public void SetHomeWorkingDay(DayEVM day) => SetDayType(day, EDayType.HomeWorking);
        public void SetVacationPaidDay(DayEVM day) => SetDayType(day, EDayType.VacationPaidDay);

        private void SetDayType(DayEVM day, EDayType type)
        {
            bool cancel = false;

            if (day == null || day.EType == type)
                cancel = true;

            if (!cancel && type != EDayType.WorkDay && day.Timesheets.Count() > 0)
            {
                if (MessageBox.Show("The selected day contains some timesheets.\nAre you sure to change the day type?\n\nAll the existing timesheets will be deleted!", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (DBArchive db = new DBArchive())
                    {
                        db.Timesheets.RemoveRange(db.Timesheets.Where(t => t.Timestamp == day.Timestamp));
                    }

                    day.Timesheets.Clear();
                }

                else
                    cancel = true;
            }

            if (!cancel)
            {
                day.EType = type;
                day.Save();
                Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, day));
            }

            IsInputEnabled = day.EType != EDayType.SickLeave && day.EType != EDayType.VacationDay;
        }

        public void CopyDay(DayEVM day)
        {
            if (day == null)
                return;

            DayEVM dayClone = new DayEVM();
            Global.Mapper.Map(day, dayClone);

            ClipboardX.Clear();
            ClipboardX.AddItem("Day", dayClone);
        }

        public void CutDay(DayEVM day)
        {
            CopyDay(day);
            ResetDay(day);
        }

        public void PasteDay(DayEVM destinationDay)
        {
            if (destinationDay == null || !ClipboardX.Contains("Day"))
                return;

            DayEVM sourceDay = ClipboardX.GetItem<DayEVM>("Day");

            if (sourceDay == null)
                return;

            destinationDay.Type = sourceDay.Type;


            using (DBArchive db = new DBArchive())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var timesheet in destinationDay.Timesheets)
                            timesheet.Delete(db);

                        destinationDay.Timesheets.Clear();
                        destinationDay.Save(db);

                        foreach (var timesheet in sourceDay.Timesheets)
                        {
                            timesheet.Id = 0;
                            timesheet.Timestamp = destinationDay.Timestamp;
                            destinationDay.Timesheets.Add(timesheet);
                            timesheet.Save(db);
                        }

                        transaction.Commit();
                        SelectedWorkingDay.Refresh(db);
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        public void ResetDay(DayEVM day)
        {
            if (day.Delete())
                day.EType = EDayType.WorkDay;
        }

        public void DeleteTimesheet(TimesheetEVM timesheet)
        {
            if (timesheet == null)
                return;

            if (timesheet.Delete())
            {
                SelectedWorkingDay.Timesheets.Remove(timesheet);
                SelectedTimesheet = null;

                Messenger.Default.Send(new DeletedItemMessage<TimesheetEVM>(this, timesheet));
            }
        }

        public void SaveTimesheet(TimesheetEVM timesheet)
        {
            if (timesheet == null)
                return;

            if (!timesheet.IsValid)
            {
                MessageBox.Show("Each time period requires a beginning and an end, and these periods can't overlap between them!", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (timesheet.HasOverlaps(SelectedWorkingDay.Timesheets.Where(t => t.Id != timesheet.Id)))
            {
                MessageBox.Show("This timesheet is overlapping with the existing ones!", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // if FDL is empty, we need to reset the FDL1 nav prop for prevent validation errors
            if (timesheet.FDL == null)
                timesheet.FDL1 = null;

            using (DBArchive db = new DBArchive())
            {
                SelectedWorkingDay.Save(db);

                if (timesheet.Save(db))
                {
                    SelectedWorkingDay.Refresh(db);
                    SelectedTimesheet = null;

                    timesheet.FDL1?.Refresh(db);

                    //if (timesheet.FDL1 != null)
                    //    Messenger.Default.Send(new ItemChangedMessage<FDLEVM>(this, timesheet.FDL1));

                    Messenger.Default.Send(new ItemChangedMessage<TimesheetEVM>(this, timesheet));
                }
            }
        }
    }
}