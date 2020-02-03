using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Great2.Models;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.Utils.Messages;
using Great2.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great2.ViewModels
{
    public class TimesheetsViewModel : ViewModelBase
    {
        #region Properties
        private bool _isAddNewEnabled = false;
        public bool IsAddNewEnabled
        {
            get => _isAddNewEnabled;
            set
            {
                Set(ref _isAddNewEnabled, value);
                CreateNewTimesheetCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set
            {
                Set(ref _isInputEnabled, value);
                SaveTimesheetCommand.RaiseCanExecuteChanged();
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

                UpdateInputsEnablement();

                if (_selectedWorkingDay != null)
                {
                    SelectedTimesheet = _selectedWorkingDay?.Timesheets?.FirstOrDefault();
                    CurrentMonth = _selectedWorkingDay.Date.Month;

                    using (DBArchive db = new DBArchive())
                    {
                        string year = CurrentYear.ToString(); // hack for query
                        FDLs = new ObservableCollection<FDLEVM>(db.FDLs.Where(fdl => fdl.Id.Substring(0, 4) == year && fdl.WeekNr == SelectedWorkingDay.WeekNr).ToList().Select(fdl => new FDLEVM(fdl)).Where(fdl => fdl.StartDayDate.Month == SelectedWorkingDay.Date.Month));
                    }

                    RaisePropertyChanged(nameof(FDLs));

                    if (SelectedTimesheet != null)
                        SelectedFDL = FDLs.SingleOrDefault(f => f.Id == SelectedTimesheet.FDL);
                }
            }
        }

        private TimesheetEVM _selectedTimesheet;
        public TimesheetEVM SelectedTimesheet
        {
            get => _selectedTimesheet;
            set
            {
                Set(ref _selectedTimesheet, value);
                UpdateInputsEnablement();
            }
        }

        private ObservableCollection<string> _tags;
        public ObservableCollection<string> Tags
        {
            get => _tags;
            set => Set(ref _tags, value);
        }

        private ObservableCollection<string> _tagIdentifiers;
        public ObservableCollection<string> TagIdentifiers
        {
            get => _tagIdentifiers;
            set => Set(ref _tagIdentifiers, value);
        }

        private FDLEVM _selectedFDL;
        public FDLEVM SelectedFDL
        {
            get => _selectedFDL;
            set => Set(ref _selectedFDL, value);
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
        public RelayCommand<DayEVM> SetSpecialLeaveCommand { get; set; }

        public RelayCommand<DayEVM> ResetDayCommand { get; set; }
        public RelayCommand<DayEVM> CopyDayCommand { get; set; }
        public RelayCommand<DayEVM> CutDayCommand { get; set; }
        public RelayCommand<DayEVM> PasteDayCommand { get; set; }

        public RelayCommand CreateNewTimesheetCommand { get; set; }
        public RelayCommand<TimesheetEVM> SaveTimesheetCommand { get; set; }
        public RelayCommand<TimesheetEVM> DeleteTimesheetCommand { get; set; }
        public RelayCommand<EventEVM> ShowEventPageCommand { get; set; }

        public RelayCommand PageLoadedCommand { get; set; }
        public RelayCommand PageUnloadedCommand { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public TimesheetsViewModel()
        {
            Tags = new ObservableCollection<string>();
            TagIdentifiers = new ObservableCollection<string>();
            NextYearCommand = new RelayCommand(() => CurrentYear++);
            PreviousYearCommand = new RelayCommand(() => CurrentYear--);
            SelectFirstDayInMonthCommand = new RelayCommand<int>(SelectFirstDayInMonth);
            SelectTodayCommand = new RelayCommand(SelectToday);

            SetVacationDayCommand = new RelayCommand<DayEVM>(SetVacationDay);
            SetSickLeaveCommand = new RelayCommand<DayEVM>(SetSickLeave);
            SetWorkDayCommand = new RelayCommand<DayEVM>(SetWorkDay);
            SetHomeWorkDayCommand = new RelayCommand<DayEVM>(SetHomeWorkingDay);
            SetSpecialLeaveCommand = new RelayCommand<DayEVM>(SetSpecialLeave);

            ResetDayCommand = new RelayCommand<DayEVM>(ResetDay);
            CopyDayCommand = new RelayCommand<DayEVM>(CopyDay);
            CutDayCommand = new RelayCommand<DayEVM>(CutDay);
            PasteDayCommand = new RelayCommand<DayEVM>(PasteDay);

            PageLoadedCommand = new RelayCommand(() => { });
            PageUnloadedCommand = new RelayCommand(() => { });

            CreateNewTimesheetCommand = new RelayCommand(CreateNewTimesheet, () => { return IsAddNewEnabled; });
            SaveTimesheetCommand = new RelayCommand<TimesheetEVM>(SaveTimesheet, (TimesheetEVM timesheet) => { return IsInputEnabled; });
            DeleteTimesheetCommand = new RelayCommand<TimesheetEVM>(DeleteTimesheet, (TimesheetEVM timesheet) => { return IsInputEnabled; });


            MessengerInstance.Register<ItemChangedMessage<DayEVM>>(this, DayTypeChanged);
            MessengerInstance.Register<ItemChangedMessage<FDLEVM>>(this, FDLChanged);
            MessengerInstance.Register<ItemChangedMessage<FactoryEVM>>(this, FactoryChanged);

            UpdateWorkingDays();

            if (Tags.Count() == 0) Tags.Add("Test");
            if (TagIdentifiers.Count() == 0) TagIdentifiers.Add("#");
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
                        var a = db.Days.ToList();
                        long timestamp = day.ToUnixTimestamp();
                        Day currentDay = db.Days.SingleOrDefault(d => d.Timestamp == timestamp);

                        if (currentDay != null)
                        {
                            var de = new DayEVM(currentDay);
                            days.Add(de);
                            var notes = de.Timesheets.Select(x => x.Notes).Where(x => x != null).Where(x=> x.Contains("#"));

                            foreach (string s in notes)
                            {
                                var parts = s.Split(' ');

                                foreach (string str in parts.Where(x=> x.StartsWith("#")))
                                {
                                    if (!Tags.Contains(str)) Tags.Add(str);
                                }

                            }


                        }
                        else
                            days.Add(new DayEVM { Date = day });
                        }
                    }
                }

            WorkingDays = days;
        }

        private void UpdateInputsEnablement()
        {
            IsAddNewEnabled = SelectedWorkingDay != null && (SelectedWorkingDay.EType == EDayType.WorkDay || SelectedWorkingDay.EType == EDayType.HomeWorkDay);
            IsInputEnabled = IsAddNewEnabled && SelectedTimesheet != null;
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

        public void CreateNewTimesheet()
        {
            SelectedTimesheet = null; // hack for clear the datagrid selection
            SelectedTimesheet = SelectedWorkingDay != null ? new TimesheetEVM() { Timestamp = SelectedWorkingDay.Timestamp } : null;
        }

        public void SetVacationDay(DayEVM day) => SetDayType(day, EDayType.VacationDay);
        public void SetSickLeave(DayEVM day) => SetDayType(day, EDayType.SickLeave);
        public void SetWorkDay(DayEVM day) => SetDayType(day, EDayType.WorkDay);
        public void SetHomeWorkingDay(DayEVM day) => SetDayType(day, EDayType.HomeWorkDay);
        public void SetSpecialLeave(DayEVM day) => SetDayType(day, EDayType.SpecialLeave);

        private void SetDayType(DayEVM day, EDayType type)
        {
            bool cancel = false;

            if (day == null || day.EType == type)
                cancel = true;

            if (!cancel && (type != EDayType.WorkDay && type != EDayType.HomeWorkDay) && day.Timesheets.Count() > 0)
            {
                if (MetroMessageBox.Show("The selected day contains some timesheets.\nAre you sure to change the day type?\n\nAll the existing timesheets will be deleted!", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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

            if ((day.Date.DayOfWeek == DayOfWeek.Saturday || day.Date.DayOfWeek == DayOfWeek.Sunday || day.IsHoliday)
            && (type == EDayType.VacationDay || type == EDayType.SickLeave || type == EDayType.SpecialLeave))
            {
                MetroMessageBox.Show("Cannot set a weekend/holiday day as vacation or leave day!");
                return;
            }

            if (!cancel)
            {
                day.EType = type;
                day.Save();
                Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, day));
            }

            UpdateInputsEnablement();
        }

        public void CopyDay(DayEVM day)
        {
            if (day == null)
                return;

            DayEVM dayClone = new DayEVM();
            Auto.Mapper.Map(day, dayClone);

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
                        SelectedWorkingDay.RaisePropertyChanged(nameof(SelectedWorkingDay.Notes_Display));
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            foreach (var timesheet in destinationDay.Timesheets)
                Messenger.Default.Send(new ItemChangedMessage<TimesheetEVM>(this, timesheet));
            }
        public void ResetDay(DayEVM day)
        {
            day.Timesheets?.ToList().ForEach(x => DeleteTimesheet(x));

            if (day.Delete())
            {
                day.EType = EDayType.WorkDay;
            }
            day.RaisePropertyChanged(nameof(day.Notes_Display));
        }
        public void DeleteTimesheet(TimesheetEVM timesheet)
        {
            if (timesheet == null)
                return;

            if (timesheet.Delete())
            {
                SelectedWorkingDay.Timesheets.Remove(timesheet);
                SelectedTimesheet = null;
                SelectedWorkingDay.RaisePropertyChanged(nameof(SelectedWorkingDay.Notes_Display));

                Messenger.Default.Send(new DeletedItemMessage<TimesheetEVM>(this, timesheet));
            }
        }
        public void SaveTimesheet(TimesheetEVM timesheet)
        {
            if (timesheet == null)
                return;

            if (!timesheet.IsValid && timesheet.TotalTime.HasValue)
            {
                MetroMessageBox.Show("Invalid time period! Operation cancelled.", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if(!timesheet.TotalTime.HasValue && SelectedFDL == null && (timesheet.Notes == null || timesheet.Notes.Trim() == string.Empty))
            {
                MetroMessageBox.Show("Empty timesheets are allowed only if at least a FDL or a Note are assigned!", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (timesheet.HasOverlaps(SelectedWorkingDay.Timesheets.Where(t => t.Id != timesheet.Id)))
            {
                MetroMessageBox.Show("This timesheet is overlapping with the existing ones!", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (timesheet.HasOverlaps(SelectedWorkingDay.Timesheets.Where(t => t.Id != timesheet.Id)))
            {
                MetroMessageBox.Show("This timesheet is overlapping with the existing ones!", "Invalid Timesheet", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (SelectedFDL != null && SelectedWorkingDay.Timesheets.Any(t => t.Id != timesheet.Id && t.FDL != string.Empty && t.FDL == SelectedFDL.Id))
            {
                MetroMessageBox.Show("The selected FDL is already assigned to another timesheet!", "Invalid FDL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedFDL != null)
            {
                timesheet.FDL1 = SelectedFDL;
                timesheet.FDL = SelectedFDL.Id;
            }
            else
            {
                // if FDL is empty, we need to reset the FDL1 nav prop for prevent validation errors
                timesheet.FDL1 = null;
                timesheet.FDL = null;
            }

            using (DBArchive db = new DBArchive())
            {
                SelectedWorkingDay.Save(db);

                if (timesheet.Save(db))
                {
                    SelectedWorkingDay.Refresh(db);
                    SelectedTimesheet = null;

                    timesheet.FDL1?.Refresh(db);

                    Messenger.Default.Send(new ItemChangedMessage<TimesheetEVM>(this, timesheet));
                }
            }

            SelectedWorkingDay.RaisePropertyChanged(nameof(SelectedWorkingDay.Notes_Display));
        }

        private void DayTypeChanged(ItemChangedMessage<DayEVM> day)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {

                    if (WorkingDays == null)
                        return;

                    DayEVM d = WorkingDays.SingleOrDefault(x => x.Timestamp == day.Content.Timestamp);

                    if (d != null)
                    {
                        d.EType = day.Content.EType;
                        d.Save();
                    }
                }));
        }

        public void FDLChanged(ItemChangedMessage<FDLEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        var days = item.Content.Timesheets.Select(t => t.Timestamp).Distinct();
                        var daysToRefresh = WorkingDays.Where(x => days.Contains(x.Timestamp));
                        var fdl = FDLs.SingleOrDefault(f => f.Id == item.Content.Id);

                        using (DBArchive db = new DBArchive())
                        {
                            // update days and timesheets
                            if (daysToRefresh != null)
                            {
                                foreach (var day in daysToRefresh)
                                    day.Refresh(db);
                                }

                            // update fdls combo
                            if (fdl != null)
                                fdl.Refresh(db);

                            db.SaveChanges();
                        }
                    }
                })
            );
        }

        public void FactoryChanged(ItemChangedMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts   
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        FactoryDTO factory = Auto.Mapper.Map<FactoryDTO>(item.Content);

                        if (factory != null)
                        {
                            var dayToUpdate = WorkingDays.Where(d => d.Timesheets.Any(t => t.FDL1 != null && t.FDL1.Factory.HasValue && t.FDL1.Factory.Value == item.Content.Id));

                            foreach (var day in dayToUpdate)
                            {
                                foreach (var timesheet in day.Timesheets)
                                    if (timesheet.FDL1 != null)
                                        timesheet.FDL1.Factory1 = factory;

                                day.RaisePropertyChanged(nameof(day.Factories_Display)); // hack to force the View to update the factory name
                            }
                        }
                    }
                })
            );
        }
    }
}