﻿using Great.Utils.Extensions;
using Itenso.TimePeriod;
using Nager.Date;
using Nager.Date.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Great.Models
{
    public partial class Day : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime Date
        {
            get { return DateTime.Now.FromUnixTimestamp(Timestamp); }
            set { Timestamp = value.ToUnixTimestamp(); }
        }

        public int WeekNr { get { return Date.WeekNr(); } }
        public bool IsHoliday { get { return DateSystem.IsPublicHoliday(Date, CountryCode.IT); } }
        public string HolidayLocalName
        {
            get
            {
                IEnumerable<PublicHoliday> holidays = DateSystem.GetPublicHoliday(CountryCode.IT, Date, Date);
                return holidays?.Count() > 0 ? holidays.FirstOrDefault().LocalName : string.Empty;
            }
        }

        public EDayType EType
        {
            get
            {
                return (EDayType)Type;
            }

            set
            {
                Type = (long)value;
            }
        }

        public bool IsWorkDay { get { return EType == EDayType.WorkDay; } }
        public bool IsVacationDay { get { return EType == EDayType.VacationDay; } }
        public bool IsSickLeave { get { return EType == EDayType.SickLeave; } }

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
                    overtime35 = subtractor.SubtractPeriods(overtime35period, difference).GetRoundedTotalDuration();
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
                    if (timesheet.FDL != string.Empty)
                        factories += timesheet?.FDL1?.Factory1?.Name + "; ";
                }

                if (factories != string.Empty)
                    factories = factories.Remove(factories.Length - 2);

                return factories;
            }
        }
        #endregion        

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyDayPropertiesChanged()
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
