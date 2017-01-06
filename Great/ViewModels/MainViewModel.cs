using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
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
    public class MainViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The <see cref="CurrentDate" /> property's name.
        /// </summary>
        
        private DateTime _currentDate = DateTime.Now;

        /// <summary>
        /// Sets and gets the CurrentDate property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public DateTime CurrentDate
        {
            get
            {
                return _currentDate;
            }

            set
            {
                if (_currentDate == value)
                {
                    return;
                }

                var oldValue = _currentDate;
                _currentDate = value;
                RaisePropertyChanged(nameof(CurrentDate), oldValue, value);

                UpdateWorkingDays();
            }
        }

        /// <summary>
        /// The <see cref="WorkingDays" /> property's name.
        /// </summary>

        private IList<WorkingDay> _workingDays;

        /// <summary>
        /// Sets and gets the WorkingDays property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public IList<WorkingDay> WorkingDays
        {
            get
            {
                return _workingDays;
            }

            set
            {
                _workingDays = value;
                RaisePropertyChanged(nameof(WorkingDays));
            }
        }

        private DBEntities _db { get; set; }
        #endregion

        #region Commands
        public RelayCommand NextMonthCommand { get; set; }
        public RelayCommand PreviousMonthCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(DBEntities db)
        {
            _db = db;

            NextMonthCommand = new RelayCommand(SetNextMonth);
            PreviousMonthCommand = new RelayCommand(SetPreviousMonth);

            UpdateWorkingDays();
        }

        private void UpdateWorkingDays()
        {
            IList<WorkingDay> days = new List<WorkingDay>();
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            
            foreach (DateTime day in AllDatesInMonth(CurrentDate.Year, CurrentDate.Month))
            {   
                WorkingDay workingDay = new WorkingDay
                {
                    WeekNr = cal.GetWeekOfYear(day, dfi.CalendarWeekRule, dfi.FirstDayOfWeek),
                    Day = day,
                    Timesheets = _db.Timesheets.SqlQuery("select * from Timesheet where Date = @date", new SQLiteParameter("date", day.ToString("yyyy-MM-dd"))).ToList()
                };

                days.Add(workingDay);
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

        private void SetNextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
        }

        private void SetPreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
        }
    }
}