using Great.Utils;
using Itenso.TimePeriod;
using System;
using System.Collections.ObjectModel;

namespace Great.Models
{
    public class WorkingDay
    {
        public int WeekNr { get; set; }
        public DateTime Date { get; set; }
        public bool IsHoliday { get; } //TODO
        public long Timestamp { get { return UnixTimestamp.GetTimestamp(Date); } }
        
        public bool HasDetails { get { return Timesheets != null ? Timesheets.Count > 0 : false; } }
        public ObservableCollection<Timesheet> Timesheets { get; set; }

        #region Time Periods
        public TimePeriodCollection TimePeriods
        {
            get
            {
                TimePeriodCollection timePeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if(ts.TimePeriods != null)
                        timePeriods.AddAll(ts.TimePeriods);

                return timePeriods.Count > 0 ? timePeriods : null;
            }
        }
        
        public TimePeriodCollection WorkingPeriods
        {
            get
            {
                TimePeriodCollection workingPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                    return null;

                foreach (Timesheet ts in Timesheets)
                    if(ts.WorkingPeriods != null)
                        workingPeriods.AddAll(ts.WorkingPeriods);

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
                    if(ts.TravelPeriods != null)
                        travelPeriods.AddAll(ts.TravelPeriods);

                return travelPeriods.Count > 0 ? travelPeriods : null;
            }
        }
        #endregion
        
        #region Overtimes
        public TimeSpan? Overtime34
        {
            get
            {
                TimeSpan overtime34 = new TimeSpan();

                if (Date.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (TimePeriods?.TotalDuration.Hours > 4)
                        overtime34 = TimeSpan.FromHours(4);
                    else
                        overtime34 = TimePeriods != null ? TimePeriods.TotalDuration : new TimeSpan();
                }
                else
                {
                    if (TimePeriods?.TotalDuration.Hours > 8)
                    {
                        if (TimePeriods?.TotalDuration.Hours >= 10)
                            overtime34 = TimeSpan.FromHours(2);
                        else
                            overtime34 = TimePeriods.TotalDuration - TimeSpan.FromHours(8);
                    }
                }

                return overtime34.Ticks > 0 ? overtime34 : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime35
        {
            get
            {
                TimeSpan overtime35 = new TimeSpan();
                TimePeriodSubtractor<TimeRange> subtractor = new TimePeriodSubtractor<TimeRange>();

                TimePeriodCollection overtime35period = new TimePeriodCollection() {
                    new TimeRange(Date, Date + new TimeSpan(6, 0, 0)),
                    new TimeRange(Date + new TimeSpan(22, 0, 0), Date.AddDays(1) + new TimeSpan(6, 0, 0))
                };

                if (TimePeriods != null)
                {
                    ITimePeriodCollection difference = subtractor.SubtractPeriods(overtime35period, TimePeriods);
                    overtime35 = subtractor.SubtractPeriods(overtime35period, difference).TotalDuration;
                }

                return overtime35.Ticks > 0 ? overtime35 : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime50
        {
            get
            {
                TimeSpan overtime50 = new TimeSpan();

                if (Date.DayOfWeek == DayOfWeek.Saturday && TimePeriods?.TotalDuration.Hours > 4)
                {
                    overtime50 = TimePeriods.TotalDuration - TimeSpan.FromHours(4);
                }
                else
                {
                    if (TimePeriods?.TotalDuration.Hours > 10)
                    {
                        overtime50 = TimePeriods.TotalDuration - TimeSpan.FromHours(10);
                    }
                }

                return overtime50.Ticks > 0 ? overtime50 : (TimeSpan?)null;
            }
        }

        public TimeSpan? Overtime100
        {
            get
            {
                TimeSpan overtime100 = new TimeSpan();

                if (Date.DayOfWeek == DayOfWeek.Sunday) //TODO: aggiungere festivi
                {
                    overtime100 = TimePeriods != null ? TimePeriods.TotalDuration : new TimeSpan();
                }

                return overtime100.Ticks > 0 ? overtime100 : (TimeSpan?)null;
            }
        }
        #endregion

        #region Display Properties        
        public string WeekNr_Display { get { return Date.DayOfWeek == DayOfWeek.Monday? WeekNr.ToString() : ""; } }
        public string Factories_Display
        {
            get
            {
                string factories = string.Empty;

                foreach (Timesheet timesheet in Timesheets)
                {
                    if(timesheet.FDL.HasValue)
                        factories += timesheet?.FDL1?.Factory1.Name + "; ";
                }

                if (factories.Length > 1)
                    factories = factories.Remove(factories.Length - 2);

                return factories;
            }
        }
        #endregion
    }
}
