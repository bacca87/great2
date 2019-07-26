using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Great.Models.EventManager;

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
                Set (ref _Id, value);
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
                //       RaisePropertyChanged(nameof(IsAllDay));
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
                IsNew = value == EEventStatus.New;
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

        private DateTime _ApprodationDateTime;
        public DateTime ApprovationDateTime
        {
            get => _ApprodationDateTime;
            set
            {
                Set(ref _ApprodationDateTime, value);
                RaisePropertyChanged(nameof(_ApprodationDateTime));
            }
        }

        public ObservableCollectionEx<DayEVM> Days { get; set; }

        #endregion

        public EventEVM() => Days = new ObservableCollectionEx<DayEVM>();

        public EventEVM(Event ev = null)
        {
            if (ev != null)
                Global.Mapper.Map(ev, this);

            Days.CollectionChanged += (sender, e) => UpdateInfo();
            Days.ItemPropertyChanged += (sender, e) => UpdateInfo();
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


        private void UpdateInfo()
        {
            RaisePropertyChanged(nameof(Id));
            RaisePropertyChanged(nameof(SharePointId));
            RaisePropertyChanged(nameof(Type));
            RaisePropertyChanged(nameof(Location));
            RaisePropertyChanged(nameof(StartDate));

            RaisePropertyChanged(nameof(EndDate));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(IsAllDay));
            RaisePropertyChanged(nameof(Status));

            RaisePropertyChanged(nameof(Status1));
            RaisePropertyChanged(nameof(EStatus));
            RaisePropertyChanged(nameof(Days));
        }
    }
}
