using Great.Utils;
using Itenso.TimePeriod;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Great.Models
{
    public partial class Day : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime Date
        {
            get { return UnixTimestamp.GetDateTime(Timestamp); }
            set { Timestamp = UnixTimestamp.GetTimestamp(value); }
        }

        public int WeekNr { get { return DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(Date, DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek); } }
        public bool IsHoliday { get; }

        public bool IsWorkDay { get { return Type == (long)EDayType.WorkDay; } }
        public bool IsVacationDay { get { return Type == (long)EDayType.VacationDay; } }
        public bool IsSickLeave { get { return Type == (long)EDayType.SickLeave; } }

        #region Totals
        public float? TotalTime
        {
            get
            {
                float? total = 0;

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if (ts.TotalTime.HasValue)
                        total += ts.TotalTime.Value;

                return total > 0 ? total : null;
            }
        }

        public float? WorkTime
        {
            get
            {
                float? total = 0;

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if (ts.WorkTime.HasValue)
                        total += ts.WorkTime.Value;

                return total > 0 ? total : null;
            }
        }

        public float? TravelTime
        {
            get
            {
                float? total = 0;

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if (ts.TravelTime.HasValue)
                        total += ts.TravelTime.Value;

                return total > 0 ? total : null;
            }
        }

        public float? HoursOfLeave
        {
            get
            {
                if (TotalTime == null || TotalTime >= 8)
                    return null;

                return 8 - TotalTime;
            }
        }
        #endregion

        #region Time Periods
        public TimePeriodCollection TimePeriods
        {
            get
            {
                TimePeriodCollection timePeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if (ts.TimePeriods != null)
                        timePeriods.AddAll(ts.TimePeriods);

                return timePeriods.Count > 0 ? timePeriods : null;
            }
        }

        public TimePeriodCollection WorkPeriods
        {
            get
            {
                TimePeriodCollection workingPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if (ts.WorkPeriods != null)
                        workingPeriods.AddAll(ts.WorkPeriods);

                return workingPeriods.Count > 0 ? workingPeriods : null;
            }
        }

        public TimePeriodCollection TravelPeriods
        {
            get
            {
                TimePeriodCollection travelPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if (ts.TravelPeriods != null)
                        travelPeriods.AddAll(ts.TravelPeriods);

                return travelPeriods.Count > 0 ? travelPeriods : null;
            }
        }
        #endregion

        #region Overtimes
        public float? Overtime34
        {
            get
            {
                float? overtime34 = null;

                if (Date.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (TotalTime.HasValue && TotalTime.Value > 4)
                        overtime34 = 4;
                    else
                        overtime34 = TotalTime;
                }
                else
                {
                    if (TotalTime.HasValue && TotalTime.Value > 8)
                    {
                        if (TotalTime.Value >= 10)
                            overtime34 = 2;
                        else
                            overtime34 = TotalTime.Value - 8;
                    }
                }

                return overtime34;
            }
        }

        public float? Overtime35
        {
            get
            {
                float? overtime35 = null;
                TimePeriodSubtractor<TimeRange> subtractor = new TimePeriodSubtractor<TimeRange>();

                TimePeriodCollection overtime35period = new TimePeriodCollection() {
                    new TimeRange(Date, Date + new TimeSpan(6, 0, 0)),
                    new TimeRange(Date + new TimeSpan(22, 0, 0), Date.AddDays(1) + new TimeSpan(6, 0, 0))
                };

                if (TimePeriods != null)
                {
                    ITimePeriodCollection difference = subtractor.SubtractPeriods(overtime35period, TimePeriods);
                    overtime35 = TimePeriodTools.GetRoundedTotalDuration(subtractor.SubtractPeriods(overtime35period, difference));
                }

                return overtime35;
            }
        }

        public float? Overtime50
        {
            get
            {
                float? overtime50 = null;

                if (Date.DayOfWeek == DayOfWeek.Saturday && TotalTime.HasValue && TotalTime.Value > 4)
                {
                    overtime50 = TotalTime - 4;
                }
                else
                {
                    if (TotalTime.HasValue && TotalTime.Value > 10)
                    {
                        overtime50 = TotalTime.Value - 10;
                    }
                }

                return overtime50;
            }
        }

        public float? Overtime100
        {
            get
            {
                float? overtime100 = null;

                if (Date.DayOfWeek == DayOfWeek.Sunday) //TODO: aggiungere festivi
                    overtime100 = TotalTime;

                return overtime100;
            }
        }
        #endregion

        #region Display Properties        
        public string WeekNr_Display { get { return Date.DayOfWeek == DayOfWeek.Monday ? WeekNr.ToString() : ""; } }
        public string Factories_Display
        {
            get
            {
                string factories = string.Empty;

                foreach (Timesheet timesheet in Timesheets)
                {
                    if (timesheet.FDL.HasValue)
                        factories += timesheet?.FDL1?.Factory1.Name + "; ";
                }

                if (factories.Length > 1)
                    factories = factories.Remove(factories.Length - 2);

                return factories;
            }
        }
        #endregion        

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyTimesheetsPropertiesChanged()
        {
            OnPropertyChanged(nameof(Type));

            OnPropertyChanged(nameof(IsWorkDay));
            OnPropertyChanged(nameof(IsVacationDay));
            OnPropertyChanged(nameof(IsSickLeave));

            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(WorkTime));
            OnPropertyChanged(nameof(TravelTime));
            OnPropertyChanged(nameof(HoursOfLeave));

            OnPropertyChanged(nameof(Overtime34));
            OnPropertyChanged(nameof(Overtime35));
            OnPropertyChanged(nameof(Overtime50));
            OnPropertyChanged(nameof(Overtime100));

            OnPropertyChanged(nameof(Factories_Display));
        }

        public Day Clone()
        {
            return new Day()
            {
                Timestamp = this.Timestamp,
                Type = this.Type
            };
        }
    }

    public enum EDayType
    {
        WorkDay = 0,
        VacationDay = 1,
        SickLeave = 2
    }
}
