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
            set
            {
                Set(ref _Id, value);
                RaisePropertyChanged(nameof(Id));
            }
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
            set
            {
                Set(ref _Type, value);
                IsChanged = true;
            }
        }

        public EEventType EType
        {
            get => (EEventType)Type;
            set
            {
                Type = (int)value;
                RaisePropertyChanged(nameof(Type));
                IsChanged = true;
            }
        }


        private string _Title;
        public string Title
        {
            get => _Title;
            set
            {
                Set(ref _Title, value);
                IsChanged = true;
            }
        }

        private string _Location;
        public string Location
        {
            get => _Location;
            set
            {
                Set(ref _Location, value);
                IsChanged = true;
            }
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
            set => Set(ref _StartDateTimestamp, value);
        }


        private long _EndDateTimestamp;
        public long EndDateTimeStamp
        {
            get => _EndDateTimestamp;
            set => Set(ref _EndDateTimestamp, value);
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
                IsChanged = true;
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
                IsChanged = true;
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
            set
            {
                Set(ref _Description, value);
                IsChanged = true;
            }
        }


        private bool _IsAllDay;
        public bool IsAllDay
        {
            get => _IsAllDay;
            set
            {
                Set(ref _IsAllDay, value);
                IsChanged = true;
            }
        }


        private bool _IsSent;
        public bool IsSent
        {
            get => _IsSent;
            set => Set(ref _IsSent, value);
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
                IsNew = value == EEventStatus.Pending && SharePointId == 0;
                RaisePropertyChanged(nameof(Status));
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
                IsChanged = true;
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
        }
        public override bool Save(DBArchive db)
        {
            Event ev = new Event();

            Global.Mapper.Map(this, ev);
            db.Events.AddOrUpdate(ev);
            db.SaveChanges();
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
        public void AddOrUpdateEventRelations(DBArchive db)
        {
            List<DayEVM> currentDayEVM;
            List<DayEVM> newDays = new List<DayEVM>();

            List<DayEventEVM> relationsToAdd = new List<DayEventEVM>();
            IEnumerable<Day> currentDays = (from d in db.Days join e in db.DayEvents on d.Timestamp equals e.Timestamp where e.EventId == Id select d);
            currentDayEVM = currentDays.Select(x => new DayEVM(x)).ToList();

            foreach (DateTime d in AllDatesInRange(StartDate, EndDate))
            {
                long timestamp = d.ToUnixTimestamp();
                Day currentDay = db.Days.SingleOrDefault(x => x.Timestamp == timestamp);

                if (currentDay == null)
                    newDays.Add(new DayEVM { Date = d });

                else newDays.Add(new DayEVM(currentDay));

                relationsToAdd.Add(new DayEventEVM { TimeStamp = timestamp, EventId = Id });
            }

            db.DayEvents.RemoveRange(db.DayEvents.Where(x => x.EventId == Id));
            newDays.ToList().ForEach(d => d.Save(db));
            relationsToAdd.ToList().ForEach(r => r.Save(db));


            if (EType == EEventType.Vacations)
            {
                if (EStatus == EEventStatus.Accepted) newDays.ToList().ForEach(d => { if (d.TotalTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
                if (EStatus == EEventStatus.Rejected) newDays.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.WorkDay; d.Save(db); });
                if (EStatus == EEventStatus.Pending) newDays.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
            }

        }
        public void AddOrUpdateEventRelations()
        {
            List<DayEVM> newDays = new List<DayEVM>();
            using (DBArchive db = new DBArchive())
            {
                List<DayEventEVM> relationsToAdd = new List<DayEventEVM>();
                IEnumerable<Day> currentDays = (from d in db.Days join e in db.DayEvents on d.Timestamp equals e.Timestamp where e.EventId == Id select d);

                foreach (DateTime d in AllDatesInRange(StartDate, EndDate))
                {
                    long timestamp = d.ToUnixTimestamp();
                    Day currentDay = db.Days.SingleOrDefault(x => x.Timestamp == timestamp);

                    if (currentDay == null)
                        newDays.Add(new DayEVM { Date = d });

                    else newDays.Add(new DayEVM(currentDay));

                    relationsToAdd.Add(new DayEventEVM { TimeStamp = timestamp, EventId = Id });
                }

                db.DayEvents.RemoveRange(db.DayEvents.Where(x => x.EventId == Id));
                newDays.ToList().ForEach(d => d.Save(db));
                relationsToAdd.ToList().ForEach(r => r.Save(db));

                if (EType == EEventType.Vacations)
                {
                    if (EStatus == EEventStatus.Accepted) newDays.ToList().ForEach(d => { if (d.TotalTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
                    if (EStatus == EEventStatus.Rejected) newDays.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.WorkDay; d.Save(db); });
                    if (EStatus == EEventStatus.Pending) newDays.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday && d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday) d.EType = EDayType.VacationDay; d.Save(db); });
                }

            }

        }
        public static IEnumerable<DateTime> AllDatesInRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();

            DateTime pointer = startDate;

            do
            {
                dates.Add(new DateTime(pointer.Date.Year, pointer.Date.Month, pointer.Date.Day, pointer.Hour, pointer.Minute, pointer.Second));
                pointer = pointer.AddDays(1);
            }
            while (pointer <= endDate);


            return dates;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EventEVM)) return false;

            EventEVM baseEv = (EventEVM)obj;

            return this.Title == baseEv.Title
                && this.Location == baseEv.Location
                && this.IsAllDay == baseEv.IsAllDay
                && this.StartDate == baseEv.StartDate
                &&this.Status == baseEv.Status
                && this.EndDate == baseEv.EndDate;
        }

    }
}
