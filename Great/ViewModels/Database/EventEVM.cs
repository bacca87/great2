using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great.ViewModels.Database
{
    public partial class EventEVM : EntityViewModelBase, IDataErrorInfo
    {
        #region Properties

        private long _Id;
        public long Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private long _SharePointId;
        public long SharePointId
        {
            get => _SharePointId;
            set => Set(ref _SharePointId, value);
        }

        private long _Type;
        public long Type
        {
            get => _Type;
            set => SetAndCheckChanged(ref _Type, value);

        }

        public EEventType EType
        {
            get => (EEventType)Type;
            set
            {
                Type = (int)value;
                RaisePropertyChanged(nameof(Type));
            }
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetAndCheckChanged(ref _Title, value);

        }

        private string _Location;
        public string Location
        {
            get => _Location;
            set => SetAndCheckChanged(ref _Location, value);

        }

        private DateTime? _SendDateTime;
        public DateTime? SendDateTime
        {
            get => _SendDateTime;
            set => Set(ref _SendDateTime, value);
        }

        private long _StartDateTimestamp;
        public long StartDateTimeStamp
        {
            get => _StartDateTimestamp;
            set => SetAndCheckChanged(ref _StartDateTimestamp, value);
        }


        private long _EndDateTimestamp;
        public long EndDateTimeStamp
        {
            get => _EndDateTimestamp;
            set => SetAndCheckChanged(ref _EndDateTimestamp, value);
        }

        public DateTime StartDate
        {
            get => DateTime.Now.FromUnixTimestamp(StartDateTimeStamp);
            set
            {
                StartDateTimeStamp = value.ToUnixTimestamp();
                RaisePropertyChanged(nameof(BeginHour));
                RaisePropertyChanged(nameof(BeginMinutes));
                RaisePropertyChanged(nameof(EndHour));
                RaisePropertyChanged(nameof(EndMinutes));
                RaisePropertyChanged(nameof(StartDate));
                RaisePropertyChanged(nameof(EndDate));
            }
        }
        public DateTime EndDate
        {
            get => DateTime.Now.FromUnixTimestamp(EndDateTimeStamp);
            set
            {
                EndDateTimeStamp = value.ToUnixTimestamp();
                RaisePropertyChanged(nameof(BeginHour));
                RaisePropertyChanged(nameof(BeginMinutes));
                RaisePropertyChanged(nameof(EndHour));
                RaisePropertyChanged(nameof(EndMinutes));
                RaisePropertyChanged(nameof(StartDate));
                RaisePropertyChanged(nameof(EndDate));
            }
        }

        public int BeginHour
        {
            get => StartDate.Hour;
            set
            {
                StartDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, value, StartDate.Minute, 0);
                RaisePropertyChanged(nameof(BeginHour));
                RaisePropertyChanged(nameof(BeginMinutes));
                RaisePropertyChanged(nameof(EndHour));
                RaisePropertyChanged(nameof(EndMinutes));

            }
        }

        public int EndHour
        {
            get => EndDate.Hour;
            set
            {
                EndDate = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, value, EndDate.Minute, 0);
                RaisePropertyChanged(nameof(BeginHour));
                RaisePropertyChanged(nameof(BeginMinutes));
                RaisePropertyChanged(nameof(EndHour));
                RaisePropertyChanged(nameof(EndMinutes));
            }
        }

        public int BeginMinutes
        {
            get => StartDate.Round(new TimeSpan(0, 5, 0)).Minute;
            set
            {
                StartDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartDate.Hour, value, 0);
                RaisePropertyChanged(nameof(BeginHour));
                RaisePropertyChanged(nameof(BeginMinutes));
                RaisePropertyChanged(nameof(EndHour));
                RaisePropertyChanged(nameof(EndMinutes));
            }
        }

        public int EndMinutes
        {
            get => EndDate.Round(new TimeSpan(0, 5, 0)).Minute;
            set
            {
                EndDate = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndDate.Hour, value, 0);
                RaisePropertyChanged(nameof(BeginHour));
                RaisePropertyChanged(nameof(BeginMinutes));
                RaisePropertyChanged(nameof(EndHour));
                RaisePropertyChanged(nameof(EndMinutes));
            }
        }


        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetAndCheckChanged(ref _Description, value);

        }

        private string _Notes;
        public string Notes
        {
            get => _Notes;
            set => SetAndCheckChanged(ref _Notes, value);

        }

        private bool _IsAllDay;
        public bool IsAllDay
        {
            get => _IsAllDay;
            set => SetAndCheckChanged(ref _IsAllDay, value);

        }


        private bool _IsSent;
        public bool IsSent
        {
            get => _IsSent;
            set => Set(ref _IsSent, value);
        }


        private bool _IsCancelRequested;
        public bool IsCancelRequested
        {
            get => _IsCancelRequested;
            set => Set(ref _IsCancelRequested, value);
        }


        private long _Status;
        public long Status
        {
            get => _Status;
            set
            {
                Set(ref _Status, value);
                RaisePropertyChanged(nameof(EStatus));
            }
        }


        private bool _IsNew;
        public bool IsNew
        {
            get => _IsNew;
            set => Set(ref _IsNew, value);
        }

        public EEventStatus EStatus
        {
            get => (EEventStatus)Status;
            set
            {
                Status = (int)value;
                RaisePropertyChanged();
            }
        }


        private EventStatusDTO _Status1;
        public EventStatusDTO Status1
        {
            get => _Status1;
            set
            {
                Set(ref _Status1, value);
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(EStatus));
            }
        }


        private EventTypeDTO _Type1;
        public EventTypeDTO Type1
        {
            get => _Type1;
            set
            {
                Set(ref _Type1, value);
                RaisePropertyChanged(nameof(Type));
                RaisePropertyChanged(nameof(EType));
            }
        }


        private bool _IsReadOnly;
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set => Set(ref _IsReadOnly, value);
        }


        private string _Approver;
        public string Approver
        {
            get => _Approver;
            set => Set(ref _Approver, value);
        }


        private DateTime? _ApprovationDate;
        public DateTime? ApprovationDate
        {
            get => _ApprovationDate;
            set => Set(ref _ApprovationDate, value);
        }

        #endregion

        #region Error Validation
        public string Error => throw new NotImplementedException();
        public bool IsValid =>
            this["Type"] == null
            && this["Title"] == null
            && this["StartDate"] == null
            && this["EndDate"] == null
            && this["EndHour"] == null
            && this["EndMinutes"] == null
            && this["BeginHour"] == null
            && this["BeginMinutes"] == null;
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Type":
                        if (Type == 0)
                            return "Type of event must be set";

                        break;
                    case "Title":
                        //if (string.IsNullOrEmpty(Title) || string.IsNullOrWhiteSpace(Title))
                        //    return "Title of event must be set";
                        break;

                    case "Location":

                        break;

                    case "StartDate":
                    case "BeginHour":
                    case "BeginMinutes":
                    case "EndDate":
                    case "EndHour":
                    case "EndMinutes":

                        if (EndDate < StartDate)
                            return "Dates not valid: End Date < Start Date";
                        break;

                    default:
                        break;
                }

                return null;
            }
        }
        #endregion

        public EventEVM(Event ev = null)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            if (ev != null)
                Global.Mapper.Map(ev, this);
            IsChanged = false;
        }
        public override bool Save(DBArchive db)
        {
            Event ev = new Event();

            Global.Mapper.Map(this, ev);
            db.Events.AddOrUpdate(ev);
            db.SaveChanges();
            IsChanged = false;
            Id = ev.Id;

            return true;
        }


        public override bool Delete(DBArchive db)
        {
            var ev = db.Events.SingleOrDefault(x => x.Id == this.Id);
            if (ev != null)
            {
                db.Events.Remove(ev);
                db.SaveChanges();
                return true;
            }
            return false;

        }
        public override bool Refresh(DBArchive db)
        {
            Event ev = db.Events.SingleOrDefault(e => e.Id == Id);

            if (ev != null)
            {
                Global.Mapper.Map(ev, this);
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            //Override needed only for dicttionaries 
            //https://www.codeproject.com/Tips/1255596/Overriding-Equals-GetHashCode-Laconically-in-CShar

            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is EventEVM)
            {
                EventEVM o = (EventEVM)obj;

                return Title == o.Title
                    && Location == o.Location
                    && IsAllDay == o.IsAllDay
                    && StartDate == o.StartDate
                    && EndDate == o.EndDate
                    && Status == o.Status;

            }
            return false;
        }

        public void AddOrUpdateEventRelations(DBArchive db, out List<DayEVM> DaysToBeCleared, out List<DayEVM> NewDaysInEvent)
        {
            NewDaysInEvent = new List<DayEVM>();
            List<DayEventEVM> dayEventToAdd = new List<DayEventEVM>();
            List<DayEVM> daysToClear = new List<DayEVM>();

            List<DayEVM> actualDaysInEvent = (from d in db.Days join e in db.DayEvents on d.Timestamp equals e.Timestamp where e.EventId == Id select new DayEVM(d)).ToList();

            foreach (DateTime d in AllDatesInRange(StartDate, EndDate))
            {
                long timestamp = d.ToUnixTimestamp();
                Day currentDay = db.Days.SingleOrDefault(x => x.Timestamp == timestamp);

                if (currentDay == null)
                    NewDaysInEvent.Add(new DayEVM { Date = d });

                else NewDaysInEvent.Add(new DayEVM(currentDay));

                dayEventToAdd.Add(new DayEventEVM { TimeStamp = timestamp, EventId = Id });
            }

            db.DayEvents.RemoveRange(db.DayEvents.Where(x => x.EventId == Id));
            NewDaysInEvent.ToList().ForEach(d => d.Save(db));
            dayEventToAdd.ToList().ForEach(r => r.Save(db));

            if (EType == EEventType.Vacations)
            {
                if (EStatus == EEventStatus.Accepted) NewDaysInEvent.ToList().ForEach(d => { if (d.TotalTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
                if (EStatus == EEventStatus.Rejected) NewDaysInEvent.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.WorkDay; d.Save(db); });
                if (EStatus == EEventStatus.Pending) NewDaysInEvent.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
            }

            DaysToBeCleared = NewDaysInEvent.Except(actualDaysInEvent).ToList();

            DaysToBeCleared.ForEach(x => { x.EType = EDayType.WorkDay; x.Save(db); });
        }
        public void AddOrUpdateEventRelations(out List<DayEVM> DaysToBeCleared, out List<DayEVM> NewDaysInEvent)
        {
            NewDaysInEvent = new List<DayEVM>();
            List<DayEventEVM> dayEventToAdd = new List<DayEventEVM>();
            List<DayEVM> daysToClear = new List<DayEVM>();

            using (DBArchive db = new DBArchive())
            {
                List<Day> act = (from d in db.Days join e in db.DayEvents on d.Timestamp equals e.Timestamp where e.EventId == Id select d).ToList();
                List<DayEVM> actualDaysInEvent = act.Select(x => new DayEVM(x)).ToList();

                foreach (DateTime d in AllDatesInRange(StartDate, EndDate))
                {
                    long timestamp = d.ToUnixTimestamp();
                    Day currentDay = db.Days.SingleOrDefault(x => x.Timestamp == timestamp);

                    if (currentDay == null)
                        NewDaysInEvent.Add(new DayEVM { Date = d });
                    else
                        NewDaysInEvent.Add(new DayEVM(currentDay));

                    dayEventToAdd.Add(new DayEventEVM { TimeStamp = timestamp, EventId = Id });
                }

                db.DayEvents.RemoveRange(db.DayEvents.Where(x => x.EventId == Id));
                NewDaysInEvent.ToList().ForEach(d => d.Save(db));
                dayEventToAdd.ToList().ForEach(r => r.Save(db));

                if (EType == EEventType.Vacations)
                {
                    if (EStatus == EEventStatus.Accepted) NewDaysInEvent.ToList().ForEach(d => { if (d.TotalTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
                    if (EStatus == EEventStatus.Rejected) NewDaysInEvent.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.WorkDay; d.Save(db); });
                    if (EStatus == EEventStatus.Pending) NewDaysInEvent.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
                }

                DaysToBeCleared = actualDaysInEvent.Except(NewDaysInEvent).ToList();

                DaysToBeCleared.ForEach(x => { x.EType = EDayType.WorkDay; x.Save(db); });
            }

        }

        public static IEnumerable<DateTime> AllDatesInRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime pointer = startDate.Midnight();

            do
            {
                dates.Add(pointer);
                pointer = pointer.AddDays(1);
            }
            while (pointer <= endDate);

            return dates;
        }

    }
}
