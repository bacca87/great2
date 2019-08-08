using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Great.Models.Database;

namespace Great.ViewModels.Database
{
    class DayEventEVM : EntityViewModelBase
    {
        private long _TimeStamp;
        public long TimeStamp
        {
            get => _TimeStamp;
            set
            {
                Set(ref _TimeStamp, value);
                RaisePropertyChanged(nameof(TimeStamp));
            }
        }

        private long _EventId;
        public long EventId
        {
            get => _EventId;
            set
            {
                Set(ref _EventId, value);
                RaisePropertyChanged(nameof(EventId));
            }
        }


        private DayEVM _day1;
        public DayEVM Day1
        {
            get => _day1;
            set => Set(ref _day1, value);
        }


        private EventEVM _event1;
        public EventEVM Event1
        {
            get => _event1;
            set => Set(ref _event1, value);
        }

        public DayEventEVM(DayEvent de = null)
        {
            if (de != null)
                Global.Mapper.Map(de, this);
        }

        public override bool Refresh(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Save(DBArchive db)
        {
            DayEvent di = new DayEvent();

            Global.Mapper.Map(this, di);
            db.DayEvents.AddOrUpdate(di);
            db.SaveChanges();


            return true;
        }

        public override bool Delete(DBArchive db)
        {
            DayEvent di = new DayEvent();

            Global.Mapper.Map(this, di);
            //db.DayEvents.Remove(di);
            db.SaveChanges();

            return true;
        }
    }
}
