using Great.Models.Database;
using Great.Models.DTO;
using System;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great.ViewModels.Database
{
    public class FactoryEVM : EntityViewModelBase, IDataErrorInfo
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
            set
            {
                Set(ref _Name, value);
                IsChanged = true;
            }

        }

        private string _CompanyName;
        public string CompanyName
        {
            get => _CompanyName;
            set
            {
                Set(ref _CompanyName, value);
                IsChanged = true;
            }
        }

        private string _Address;
        public string Address
        {
            get => _Address;
            set
            {
                Set(ref _Address, value);
                IsChanged = true;
            }
        }

        private double? _Latitude;
        public double? Latitude
        {
            get => _Latitude;
            set
            {
                Set(ref _Latitude, value);
                IsChanged = true;
            }
        }

        private double? _Longitude;
        public double? Longitude
        {
            get => _Longitude;
            set
            {
                Set(ref _Longitude, value);
                IsChanged = true;
            }
        }

        private long _TransferType;
        public long TransferType
        {
            get => _TransferType;
            set
            {
                Set(ref _TransferType, value);
                IsChanged = true;
            }
        }

        private bool _IsForfait;
        public bool IsForfait
        {
            get => _IsForfait;
            set
            {
                Set(ref _IsForfait, value);
                IsChanged = true;
            }
        }

        private bool _NotifyAsNew;
        public bool NotifyAsNew
        {
            get => _NotifyAsNew;
            set => Set(ref _NotifyAsNew, value);
        }

        private bool _OverrideAddressOnFDL;
        public bool OverrideAddressOnFDL
        {
            get => _OverrideAddressOnFDL;
            set
            {
                Set(ref _OverrideAddressOnFDL, value);
                IsChanged = true;
            }
        }

        private TransferTypeDTO _TransferType1;
        public TransferTypeDTO TransferType1
        {
            get => _TransferType1;
            set
            {
                Set(ref _TransferType1, value);
                IsChanged = true;
            }
        }

        #endregion

        #region Error Validation

        public string Error => throw new NotImplementedException();
        public bool IsValid =>
           this["Name"] == null
            && this["CompanyName"] == null
            && this["Address"] == null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Name":
                        if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
                            return "Name of the factory must be set";

                        break;
                    case "CompanyName":

                        break;

                    case "Address":
                        if (string.IsNullOrEmpty(Address) || string.IsNullOrWhiteSpace(Address))
                            return "Address must be set";

                        break;
                    default:
                        break;
                }

                return null;
            }
        }

        #endregion

        public FactoryEVM(Factory factory = null)
        {
            if (factory != null)
                Global.Mapper.Map(factory, this);
        }

        public override bool Delete(DBArchive db)
        {
            Factory factoryToDelete = db.Factories.SingleOrDefault(f => f.Id == Id);

            if (factoryToDelete != null)
            {
                db.Factories.Remove(factoryToDelete);
                db.SaveChanges();
            }

            return true;
        }

        public override bool Refresh(DBArchive db)
        {
            var f = db.Factories.SingleOrDefault(x => x.Id == Id);

            if (f != null)
            {
                Global.Mapper.Map(f, this);
                return true;
            }

            return false;
        }

        public override bool Save(DBArchive db)
        {
            Factory factory = new Factory();

            Global.Mapper.Map(this, factory);
            db.Factories.AddOrUpdate(factory);
            db.SaveChanges();
            AcceptChanges();
            Id = factory.Id;
            return true;
        }
    }
}
