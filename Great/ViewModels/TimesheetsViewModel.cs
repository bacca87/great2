using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that the Timesheets View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class TimesheetsViewModel : ViewModelBase
    {
        public const int MIN_YEAR = 1900;
        public const int MAX_YEAR = 2100;

        public const int PERFORMANCE_DESCRIPTION_MAX_LENGTH = 575;
        public const int FINAL_TEST_RESULT_MAX_LENGTH = 495;
        public const int OTHER_NOTES_MAX_LENGTH = 595;

        #region Properties
        /// <summary>
        /// Gets the PerfDescMaxLength property.
        /// </summary>
        public int PerfDescMaxLength
        {
            get
            {
                return PERFORMANCE_DESCRIPTION_MAX_LENGTH;
            }
        }

        /// <summary>
        /// Gets the FinalTestResultMaxLength property.
        /// </summary>
        public int FinalTestResultMaxLength
        {
            get
            {
                return FINAL_TEST_RESULT_MAX_LENGTH;
            }
        }

        /// <summary>
        /// Gets the OtherNotesMaxLength property.
        /// </summary>
        public int OtherNotesMaxLength
        {
            get
            {
                return OTHER_NOTES_MAX_LENGTH;
            }
        }

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
                SaveTimesheetCommand.RaiseCanExecuteChanged();
                ClearTimesheetCommand.RaiseCanExecuteChanged();
                DeleteTimesheetCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="CurrentYear" /> property's name.
        /// </summary>
        private int _currentYear = DateTime.Now.Year;

        /// <summary>
        /// Sets and gets the CurrentYear property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public int CurrentYear
        {
            get
            {
                return _currentYear;
            }

            set
            {
                if (_currentYear == value)
                {
                    return;
                }
                
                var oldValue = _currentYear;

                if (value < MIN_YEAR)
                    _currentYear = MIN_YEAR;
                else if (value > MAX_YEAR)
                    _currentYear = MAX_YEAR;
                else
                    _currentYear = value;

                RaisePropertyChanged(nameof(CurrentYear), oldValue, value);

                UpdateWorkingDays();
            }
        }

        /// <summary>
        /// The <see cref="CurrentMonth" /> property's name.
        /// </summary>
        private int _currentMonth = DateTime.Now.Month;

        /// <summary>
        /// Sets and gets the CurrentMonth property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public int CurrentMonth
        {
            get
            {
                return _currentMonth;
            }

            set
            {
                if (_currentMonth == value)
                {
                    return;
                }

                var oldValue = _currentMonth;
                _currentMonth = value;

                RaisePropertyChanged(nameof(CurrentMonth), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="WorkingDays" /> property's name.
        /// </summary>
        private BindingList<Day> _workingDays;

        /// <summary>
        /// Sets and gets the WorkingDays property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public BindingList<Day> WorkingDays
        {
            get
            {
                return _workingDays;
            }
            internal set
            {
                _workingDays = value;
                RaisePropertyChanged(nameof(WorkingDays));
            }
        }

        /// <summary>
        /// The <see cref="Timesheets" /> property's name.
        /// </summary>
        private IList<Timesheet> _timesheets;

        /// <summary>
        /// Sets and gets the Timesheets property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public IList<Timesheet> Timesheets
        {
            get
            {
                return _timesheets;
            }
            internal set
            {
                _timesheets = value;
                RaisePropertyChanged(nameof(Timesheets));
            }
        }

        /// <summary>
        /// The <see cref="SelectedWorkingDay" /> property's name.
        /// </summary>
        private Day _selectedWorkingDay;

        /// <summary>
        /// Sets and gets the SelectedWorkingDay property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public Day SelectedWorkingDay
        {
            get
            {
                return _selectedWorkingDay;
            }

            set
            {
                var oldValue = _selectedWorkingDay;
                _selectedWorkingDay = value;

                if (_selectedWorkingDay != null)
                {
                    CurrentMonth = _selectedWorkingDay.Date.Month;
                    
                    Timesheets = _db.Timesheets.Where(t => t.Timestamp == _selectedWorkingDay.Timestamp).ToList();
                    SelectedTimesheet = null;
                    if (_selectedWorkingDay.Type != (long)EDayType.SickLeave && _selectedWorkingDay.Type != (long)EDayType.VacationDay)
                        IsInputEnabled = true;
                    else
                        IsInputEnabled = false;
                }
                else
                    IsInputEnabled = false;

                RaisePropertyChanged(nameof(SelectedWorkingDay), oldValue, value);
                RaisePropertyChanged(nameof(FDLs));
            }
        }

        /// <summary>
        /// The <see cref="SelectedTimesheet" /> property's name.
        /// </summary>
        private Timesheet _selectedTimesheet;

        /// <summary>
        /// Sets and gets the SelectedTimesheet property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public Timesheet SelectedTimesheet
        {
            get
            {
                return _selectedTimesheet;
            }

            set
            {
                var oldValue = _selectedTimesheet;
                _selectedTimesheet = value;

                TimesheetInfo = _selectedTimesheet != null ? _selectedTimesheet.Clone() : new Timesheet() { Timestamp = SelectedWorkingDay.Timestamp };

                RaisePropertyChanged(nameof(SelectedTimesheet), oldValue, value);
                DeleteTimesheetCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="TimesheetInfo" /> property's name.
        /// </summary>
        private Timesheet _timesheetInfo;

        /// <summary>
        /// Sets and gets the TimesheetInfo property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public Timesheet TimesheetInfo
        {
            get
            {
                return _timesheetInfo;
            }

            set
            {
                var oldValue = _timesheetInfo;
                _timesheetInfo = value;
                RaisePropertyChanged(nameof(TimesheetInfo), oldValue, value);
                SaveTimesheetCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Sets and gets the FDLs property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public IList<FDL> FDLs
        {
            get
            {
                if (SelectedWorkingDay != null)
                    return _db.FDLs.Where(fdl => fdl.WeekNr == SelectedWorkingDay.WeekNr).ToList();
                else
                    return null;
            }
        }
        
        /// <summary>
        /// Sets and gets the OnSelectFirstDayInMonth Action.        
        /// </summary>
        public Action<Day> OnSelectFirstDayInMonth;

        private DBEntities _db { get; set; }
        #endregion

        #region Commands Definitions
        public RelayCommand NextYearCommand { get; set; }
        public RelayCommand PreviousYearCommand { get; set; }
        public RelayCommand<int> SelectFirstDayInMonthCommand { get; set; }
        public RelayCommand SelectTodayCommand { get; set; }

        public RelayCommand<Day> SetVacationDayCommand { get; set; }
        public RelayCommand<Day> SetSickLeaveCommand { get; set; }
        public RelayCommand<Day> SetWorkDayCommand { get; set; }

        public RelayCommand<Day> ResetDayCommand { get; set; }
        public RelayCommand<Day> CopyDayCommand { get; set; }
        public RelayCommand<Day> CutDayCommand { get; set; }
        public RelayCommand<Day> PasteDayCommand { get; set; }

        public RelayCommand ClearTimesheetCommand { get; set; }
        public RelayCommand<Timesheet> SaveTimesheetCommand { get; set; }
        public RelayCommand<Timesheet> DeleteTimesheetCommand { get; set; }        
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public TimesheetsViewModel()
        {
            _db = new DBEntities();

            NextYearCommand = new RelayCommand(SetNextYear);
            PreviousYearCommand = new RelayCommand(SetPreviousYear);
            SelectFirstDayInMonthCommand = new RelayCommand<int>(SelectFirstDayInMonth);
            SelectTodayCommand = new RelayCommand(SelectToday);

            SetVacationDayCommand = new RelayCommand<Day>(SetVacationDay);
            SetSickLeaveCommand = new RelayCommand<Day>(SetSickLeave);
            SetWorkDayCommand = new RelayCommand<Day>(SetWorkDay);

            ResetDayCommand = new RelayCommand<Day>(ResetDay);
            CopyDayCommand = new RelayCommand<Day>(CopyDay);
            CutDayCommand = new RelayCommand<Day>(CutDay);
            PasteDayCommand = new RelayCommand<Day>(PasteDay);

            ClearTimesheetCommand = new RelayCommand(ClearTimesheet, () => { return IsInputEnabled; });
            SaveTimesheetCommand = new RelayCommand<Timesheet>(SaveTimesheet, (Timesheet timesheet) => { return IsInputEnabled && timesheet != null; });
            DeleteTimesheetCommand = new RelayCommand<Timesheet>(DeleteTimesheet, (Timesheet timesheet) => { return IsInputEnabled && timesheet != null; });

            UpdateWorkingDays();
            SelectToday();
        }
        
        private void UpdateWorkingDays()
        {
            BindingList<Day> days = new BindingList<Day>();

            for (int month = 1; month <= 12; month++)
            {
                foreach (DateTime day in AllDatesInMonth(CurrentYear, month))
                {
                    long timestamp = UnixTimestamp.GetTimestamp(day);

                    Day currentDay = _db.Days.Where(d => d.Timestamp == timestamp).FirstOrDefault();

                    if (currentDay != null)
                        days.Add(currentDay);
                    else
                        days.Add(new Day { Date = day });
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
        
        private void SetNextYear()
        {
            CurrentYear++;
        }

        private void SetPreviousYear()
        {
            CurrentYear--;
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
            SelectedWorkingDay = WorkingDays.Where(day => day.Date.DayOfYear == DateTime.Now.DayOfYear).FirstOrDefault();
        }

        public void ClearTimesheet()
        {
            SelectedTimesheet = null;
        }

        public void SetVacationDay(Day day)
        {
            SetDayType(day, EDayType.VacationDay);
        }

        public void SetSickLeave(Day day)
        {
            SetDayType(day, EDayType.SickLeave);
        }

        public void SetWorkDay(Day day)
        {
            SetDayType(day, EDayType.WorkDay);
        }

        private void SetDayType(Day day, EDayType type)
        {
            bool cancel = false;

            if (day == null || day.Type == (long)type)
                cancel = true;

            if (!cancel && type != EDayType.WorkDay && day.Timesheets.Count() > 0)
            {
                if (MessageBox.Show("The selected day contains some timesheets.\nAre you sure to change the day type?\n\nAll the existing timesheets will be deleted!", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                { 
                    _db.Timesheets.RemoveRange(day.Timesheets);
                    Timesheets = null;
                }
                else
                    cancel = true;
            }

            if(!cancel)
            {
                day.Type = (long)type;
                _db.Days.AddOrUpdate(day);
                _db.SaveChanges();
            }

            if (day.Type != (long)EDayType.SickLeave && day.Type != (long)EDayType.VacationDay)
                IsInputEnabled = true;
            else
                IsInputEnabled = false;

            day.NotifyTimesheetsPropertiesChanged();
        }

        public void ResetDay(Day day)
        {
            if (!_db.Days.Any(d => d.Timestamp == day.Timestamp))
                return;

            _db.Days.Remove(day);

            if (_db.SaveChanges() > 0)
            {
                day.Type = (long)EDayType.WorkDay;
                day.NotifyTimesheetsPropertiesChanged();
                Timesheets = null;
            }
        }

        public void CopyDay(Day day)
        {
            if (day == null)
                return;

            ClipboardX.Clear();
            ClipboardX.AddItem("Day", day);
            ClipboardX.AddItem("Timesheets", day.Timesheets.ToList());
        }

        public void CutDay(Day day)
        {
            if (day == null)
                return;
            
            ClipboardX.Clear();
            ClipboardX.AddItem("Day", day.Clone());
            ClipboardX.AddItem("Timesheets", day.Timesheets.ToList());
            
            ResetDay(day);
        }

        public void PasteDay(Day destinationDay)
        {
            if (destinationDay == null || !ClipboardX.Contains("Day") || !ClipboardX.Contains("Timesheets"))
                return;
            
            Day sourceDay = ClipboardX.GetItem<Day>("Day");
            IList<Timesheet> sourceTimesheets = ClipboardX.GetItem<IList<Timesheet>>("Timesheets");

            if (sourceDay == null || sourceTimesheets == null)
                return;

            IList<Timesheet> destinationTimesheets = new List<Timesheet>();

            foreach (Timesheet timesheet in sourceTimesheets)
            {
                Timesheet tmp = timesheet.Clone();
                tmp.Timestamp = destinationDay.Timestamp;
                destinationTimesheets.Add(tmp);
            }

            if (destinationDay.Timesheets != null && destinationDay.Timesheets.Count > 0)
                _db.Timesheets.RemoveRange(destinationDay.Timesheets);

            _db.Timesheets.AddRange(destinationTimesheets);

            destinationDay.Type = sourceDay.Type;
            _db.Days.AddOrUpdate(destinationDay);

            if(_db.SaveChanges() > 0)
            {
                sourceDay.NotifyTimesheetsPropertiesChanged();
                destinationDay.NotifyTimesheetsPropertiesChanged();
            }
        }

        public void DeleteTimesheet(Timesheet timesheet)
        {
            if (timesheet == null)
                return;

            _db.Timesheets.Remove(timesheet);

            if (_db.SaveChanges() > 0)
            {
                SelectedWorkingDay.NotifyTimesheetsPropertiesChanged();
                Timesheets = SelectedWorkingDay.Timesheets.ToList();
                SelectedTimesheet = null;
            }
        }

        public void SaveTimesheet(Timesheet timesheet)
        {
            if (timesheet == null)
                return;

            if (!timesheet.IsValid)
            {
                MessageBox.Show("Each time period requires a beginning and an end, and these periods can't overlap between them!\nFurthermore the FDL system doesn't permit periods ending after the 04:00 AM of the next day.", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if(timesheet.HasOverlaps(SelectedWorkingDay.Timesheets.Where(t => t.Id != timesheet.Id)))
            {
                MessageBox.Show("This timesheet is overlapping with the existing ones!", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _db.Days.AddOrUpdate(SelectedWorkingDay);
            _db.Timesheets.AddOrUpdate(timesheet);

            if (_db.SaveChanges() > 0)
            {
                SelectedWorkingDay.NotifyTimesheetsPropertiesChanged();
                Timesheets = SelectedWorkingDay.Timesheets.ToList();
                SelectedTimesheet = null;
            }
        }
    }
}