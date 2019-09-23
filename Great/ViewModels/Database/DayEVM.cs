using Great.Models;
using Great.Models.Database;
using Great.Utils;
using Great.Utils.Extensions;
using Itenso.TimePeriod;
using Nager.Date;
using Nager.Date.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Day = Great.Models.Database.Day;

namespace Great.ViewModels.Database
{
    public class DayEVM : EntityViewModelBase
    {
        #region Properties
        public long _Timestamp;
        public long Timestamp
        {
            get => _Timestamp;
            set => Set(ref _Timestamp, value);
        }

        public long _Type;
        public long Type
        {
            get => _Type;
            set
            {
                Set(ref _Type, value);
                RaisePropertyChanged(nameof(EType));
            }
        }

        public DayType _DayType;
        public DayType DayType
        {
            get => _DayType;
            set => Set(ref _DayType, value);
        }

        public ObservableCollectionEx<TimesheetEVM> Timesheets { get; set; }

        public DateTime Date
        {
            get => DateTime.Now.FromUnixTimestamp(Timestamp);
            set
            {
                Timestamp = value.ToUnixTimestamp();
                RaisePropertyChanged();
            }
        }

        public int WeekNr => Date.WeekNr();
        public bool IsHoliday => DateSystem.IsPublicHoliday(Date, UserSettings.Localization.Country);
        public string HolidayLocalName
        {
            get
            {
                IEnumerable<PublicHoliday> holidays = DateSystem.GetPublicHoliday(Date, Date, UserSettings.Localization.Country);
                return holidays?.Count() > 0 ? holidays.FirstOrDefault().LocalName : string.Empty;
            }
        }

        public EDayType EType
        {
            get => (EDayType)Type;
            set
            {
                Type = (long)value;
                RaisePropertyChanged();
            }
        }

        public bool IsWorkDay => EType == EDayType.WorkDay;
        public bool IsVacationDay => EType == EDayType.VacationDay;
        public bool IsSickLeave => EType == EDayType.SickLeave;
        public bool IsHomeWork => EType == EDayType.HomeWorkDay;
        public bool IsSpecialLeave => EType == EDayType.SpecialLeave;

        #region Totals
        public float? TotalTime
        {
            get
            {
                float? total = 0;

                if (Timesheets == null || Timesheets.Count == 0)
                {
                    return null;
                }

                foreach (TimesheetEVM ts in Timesheets)
                {
                    if (ts.TotalTime.HasValue)
                    {
                        total += ts.TotalTime.Value;
                    }
                }

                return total > 0 ? total : null;
            }
        }

        public float? WorkTime
        {
            get
            {
                float? total = 0;

                if (Timesheets == null || Timesheets.Count == 0)
                {
                    return null;
                }

                foreach (TimesheetEVM ts in Timesheets)
                {
                    if (ts.WorkTime.HasValue)
                    {
                        total += ts.WorkTime.Value;
                    }
                }

                return total > 0 ? total : null;
            }
        }

        public float? TravelTime
        {
            get
            {
                float? total = 0;

                if (Timesheets == null || Timesheets.Count == 0)
                {
                    return null;
                }

                foreach (TimesheetEVM ts in Timesheets)
                {
                    if (ts.TravelTime.HasValue)
                    {
                        total += ts.TravelTime.Value;
                    }
                }

                return total > 0 ? total : null;
            }
        }

        public float? HoursOfLeave
        {
            get
            {
                if (TotalTime == null || TotalTime >= 8)
                {
                    return null;
                }

                return 8 - TotalTime;
            }
        }

        public float? HoursOfVacation
        {
            get
            {
                if (EType != EDayType.VacationDay || TotalTime >= 8)
                {
                    return null;
                }

                return 8 - (TotalTime ?? 0);
            }
        }

        public float? HoursOfHomeWorking
        {
            get
            {
                if (EType == EDayType.HomeWorkDay)
                    return TotalTime;
                return null;
            }
        }
        public float? HoursOfSpecialLeave
        {
            get
            {
                if (EType != EDayType.SpecialLeave || TotalTime >= 8)
                {
                    return null;
                }

                return 8 - (TotalTime ?? 0);
            }
        }
        public float? HoursOfSicklLeave
        {
            get
            {
                if (EType != EDayType.SickLeave || TotalTime >= 8)
                {
                    return null;
                }

                return 8 - (TotalTime ?? 0);
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
                {
                    return null;
                }

                foreach (TimesheetEVM ts in Timesheets)
                {
                    if (ts.TimePeriods != null)
                    {
                        timePeriods.AddAll(ts.TimePeriods);
                    }
                }

                return timePeriods.Count > 0 ? timePeriods : null;
            }
        }

        public TimePeriodCollection WorkPeriods
        {
            get
            {
                TimePeriodCollection workingPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                {
                    return null;
                }

                foreach (TimesheetEVM ts in Timesheets)
                {
                    if (ts.WorkPeriods != null)
                    {
                        workingPeriods.AddAll(ts.WorkPeriods);
                    }
                }

                return workingPeriods.Count > 0 ? workingPeriods : null;
            }
        }

        public TimePeriodCollection TravelPeriods
        {
            get
            {
                TimePeriodCollection travelPeriods = new TimePeriodCollection();

                if (Timesheets == null || Timesheets.Count == 0)
                {
                    return null;
                }

                foreach (TimesheetEVM ts in Timesheets)
                {
                    if (ts.TravelPeriods != null)
                    {
                        travelPeriods.AddAll(ts.TravelPeriods);
                    }
                }

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

                if (!IsHoliday)
                {
                    if (Date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        if (TotalTime.HasValue && TotalTime.Value > 4)
                        {
                            overtime34 = 4;
                        }
                        else
                        {
                            overtime34 = TotalTime;
                        }
                    }
                    else
                    {
                        if (TotalTime.HasValue && TotalTime.Value > 8)
                        {
                            if (TotalTime.Value >= 10)
                            {
                                overtime34 = 2;
                            }
                            else
                            {
                                overtime34 = TotalTime.Value - 8;
                            }
                        }
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

                if (!IsHoliday)
                {
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
                }

                return overtime35;
            }
        }

        public float? Overtime50
        {
            get
            {
                float? overtime50 = null;

                if (!IsHoliday)
                {
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
                }

                return overtime50;
            }
        }

        public float? Overtime100
        {
            get
            {
                float? overtime100 = null;

                if (Date.DayOfWeek == DayOfWeek.Sunday || IsHoliday)
                {
                    overtime100 = TotalTime;
                }

                return overtime100;
            }
        }

        #endregion

        #region Display Properties
        public string WeekNr_Display => Date.DayOfWeek == DayOfWeek.Monday ? WeekNr.ToString() : "";
        public string Factories_Display
        {
            get
            {
                string factories = string.Empty;

                foreach (TimesheetEVM timesheet in Timesheets)
                {
                    if (!string.IsNullOrEmpty(timesheet.FDL))
                    {
                        factories += timesheet?.FDL1?.Factory1?.Name + "; ";
                    }
                }

                if (!string.IsNullOrEmpty(factories))
                {
                    factories = factories.Remove(factories.Length - 2);
                }

                return factories;
            }
        }
        public string Notes_Display
        {
            get
            {
                string notes = string.Empty;

                foreach (TimesheetEVM timesheet in Timesheets)
                {
                    if (!string.IsNullOrEmpty(timesheet.Notes))
                    {
                        notes += timesheet?.Notes + ";";
                    }
                }

                if (!string.IsNullOrEmpty(notes))
                {
                    notes = notes.Remove(notes.Length - 1);
                }

                return notes;
            }
        }
        #endregion

        #endregion

        public DayEVM(Day day = null)
        {
            Timesheets = new ObservableCollectionEx<TimesheetEVM>();

            if (day != null)
            {
                Global.Mapper.Map(day, this);
            }

            Timesheets.CollectionChanged += (sender, e) => UpdateInfo();
            Timesheets.ItemPropertyChanged += (sender, e) => UpdateInfo();

            IsChanged = false;
        }

        private void UpdateInfo()
        {
            RaisePropertyChanged(nameof(IsWorkDay));
            RaisePropertyChanged(nameof(IsVacationDay));
            RaisePropertyChanged(nameof(IsSpecialLeave));
            RaisePropertyChanged(nameof(IsSickLeave));
            RaisePropertyChanged(nameof(IsHomeWork));

            RaisePropertyChanged(nameof(TotalTime));
            RaisePropertyChanged(nameof(WorkTime));
            RaisePropertyChanged(nameof(TravelTime));
            RaisePropertyChanged(nameof(HoursOfLeave));

            RaisePropertyChanged(nameof(Overtime34));
            RaisePropertyChanged(nameof(Overtime35));
            RaisePropertyChanged(nameof(Overtime50));
            RaisePropertyChanged(nameof(Overtime100));

            RaisePropertyChanged(nameof(Factories_Display));
        }

        public override bool Save(DBArchive db)
        {
            Day day = new Day();

            Global.Mapper.Map(this, day);
            db.Days.AddOrUpdate(day);
            db.SaveChanges();
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            var day = db.Days.SingleOrDefault(d => d.Timestamp == Timestamp);

            if (day != null)
            {
                db.Days.Remove(day);
                db.SaveChanges();
            }

            Timesheets.Clear();
            return true;
        }

        public override bool Refresh(DBArchive db)
        {
            Day day = db.Days.SingleOrDefault(d => d.Timestamp == Timestamp);

            if (day != null)
            {
                Global.Mapper.Map(day, this);

                foreach (TimesheetEVM timesheet in Timesheets)
                {
                    timesheet.Refresh(db);
                }

                return true;
            }

            return false;
        }

    }

    public enum EDayType
    {
        WorkDay = 0,
        VacationDay = 1,
        SickLeave = 2,
        HomeWorkDay = 3,
        SpecialLeave = 4
    }
}
