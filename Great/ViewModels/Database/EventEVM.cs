using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great.ViewModels.Database
{
    public partial class EventEVM : EntityViewModelBase
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
            set
            {
                Set(ref _SharePointId, value);
                RaisePropertyChanged(nameof(SharePointId));
            }
        }

        private long _Type;
        public long Type
        {
            get => _Type;
            set
            {
                Set(ref _Type, value);
                //         RaisePropertyChanged(nameof(Location));
            }
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set
            {
                Set(ref _Title, value);
            }
        }

        private string _Location;
        public string Location
        {
            get => _Location;
            set
            {
                Set(ref _Location, value);
                //         RaisePropertyChanged(nameof(Location));
            }
        }

        private DateTime? _SendDateTime;
        public DateTime? SendDateTime
        {
            get => _SendDateTime;
            set
            {
                Set(ref _SendDateTime, value);
                RaisePropertyChanged(nameof(SendDateTime));
            }
        }

        private long _StartDateTimestamp;
        public long StartDateTimeStamp
        {
            get => _StartDateTimestamp;
            set
            {
                Set(ref _StartDateTimestamp, value);
                //RaisePropertyChanged(nameof(StartDate));
            }
        }

        public DateTime StartDate
        {
            get => DateTime.Now.FromUnixTimestamp(StartDateTimeStamp);
            set
            {
                StartDateTimeStamp = value.ToUnixTimestamp();
                RaisePropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => DateTime.Now.FromUnixTimestamp(EndDateTimeStamp);
            set
            {
                EndDateTimeStamp = value.ToUnixTimestamp();
                RaisePropertyChanged();
            }
        }

        private long _EndDateTimestamp;
        public long EndDateTimeStamp
        {
            get => _EndDateTimestamp;
            set
            {
                Set(ref _EndDateTimestamp, value);
                //RaisePropertyChanged(nameof(EndDate));
            }
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set
            {
                Set(ref _Description, value);
                //     RaisePropertyChanged(nameof(Description));
            }
        }

        private bool _IsAllDay;
        public bool IsAllDay
        {
            get => _IsAllDay;
            set
            {
                Set(ref _IsAllDay, value);
                RaisePropertyChanged();
            }
        }

        private bool _IsSent;
        public bool IsSent
        {
            get => _IsSent;
            set
            {
                Set(ref _IsSent, value);
                RaisePropertyChanged(nameof(IsSent));
            }
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

        public EEventType EType
        {
            get => (EEventType)Type;
            set
            {
                Type = (int)value;
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
                RaisePropertyChanged(nameof(Status1));
            }
        }

        private EventTypeDTO _Type1;
        public EventTypeDTO Type1
        {
            get => _Type1;
            set
            {
                Set(ref _Type1, value);
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
            set
            {
                Set(ref _Approver, value);
                RaisePropertyChanged(nameof(Approver));
            }
        }

        private DateTime? _ApprovationDate;
        public DateTime? ApprovationDate
        {
            get => _ApprovationDate;
            set
            {
                Set(ref _ApprovationDate, value);
                RaisePropertyChanged(nameof(_ApprovationDate));
            }
        }

        #endregion

        public EventEVM(Event ev = null)
        {
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
            var ev = db.Events.Where(x => x.Id == this.Id).FirstOrDefault();
            db.Events.Remove(ev);
            db.SaveChanges();
            return true;
        }

        public override bool Refresh(DBArchive db)
        {
            Event ev = db.Events.SingleOrDefault(e => e.Id == Id);

            if (ev != null)
            {
                Global.Mapper.Map(ev, this);

                //foreach (TimesheetEVM timesheet in Timesheets)
                //    timesheet.Refresh(db);

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
            List<DayEVM> currentDayEVM;
            List<DayEVM> newDays = new List<DayEVM>();
            using (DBArchive db = new DBArchive())
            {
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

        private void UpdateInfo()
        {
            RaisePropertyChanged(nameof(Id));
            RaisePropertyChanged(nameof(SharePointId));
            RaisePropertyChanged(nameof(Type));
            RaisePropertyChanged(nameof(Location));
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(StartDate));

            RaisePropertyChanged(nameof(EndDate));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(IsAllDay));
            RaisePropertyChanged(nameof(Status));

            RaisePropertyChanged(nameof(Status1));
            RaisePropertyChanged(nameof(EStatus));
        }
    }
}
