using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AutoMapper;
using Great.Models.Database;
using Great.Models.DTO;

namespace Great.ViewModels.Database
{
    public class FactoryEVM : EntityViewModelBase
    {
        #region Properties
        private long _Id;
        public long Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private string _Name;
        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }

        private string _CompanyName;
        public string CompanyName
        {
            get => _CompanyName;
            set => Set(ref _CompanyName, value);
        }

        private string _Address;
        public string Address
        {
            get => _Address;
            set => Set(ref _Address, value);
        }

        private double? _Latitude;
        public double? Latitude
        {
            get => _Latitude;
            set => Set(ref _Latitude, value);
        }

        private double? _Longitude;
        public double? Longitude
        {
            get => _Longitude;
            set => Set(ref _Longitude, value);
        }

        private long _TransferType;
        public long TransferType
        {
            get => _TransferType;
            set => Set(ref _TransferType, value);
        }

        private bool _IsForfait;
        public bool IsForfait
        {
            get => _IsForfait;
            set => Set(ref _IsForfait, value);
        }

        private bool _NotifyAsNew;
        public bool NotifyAsNew
        {
            get => _NotifyAsNew;
            set => Set(ref _NotifyAsNew, value);
        }

        private TransferTypeDTO _TransferType1;
        public TransferTypeDTO TransferType1
        {
            get => _TransferType1;
            set => Set(ref _TransferType1, value);
        }
        #endregion

        public FactoryEVM(Factory factory = null)
        {
            if (factory != null)
                Mapper.Map(factory, this);
        }

        public override bool Delete(DBArchive db)
        {
            db.Factories.Remove(db.Factories.SingleOrDefault(f => f.Id == Id));
            db.SaveChanges();
            Id = 0;
            return true;
        }

        public override bool Refresh(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Save(DBArchive db)
        {
            Factory factory = new Factory();

            Mapper.Map(this, factory);
            db.Factories.AddOrUpdate(factory);
            db.SaveChanges();
            Id = factory.Id;

            return true;
        }
    }
}
