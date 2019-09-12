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
            set
            {
                Set(ref _NotifyAsNew, value);
                RaisePropertyChanged(nameof(Factory_New_Display));
            }
        }

        private bool _OverrideAddressOnFDL;
        public bool OverrideAddressOnFDL
        {
            get => _OverrideAddressOnFDL;
            set => Set(ref _OverrideAddressOnFDL, value);
        }

        private TransferTypeDTO _TransferType1;
        public TransferTypeDTO TransferType1
        {
            get => _TransferType1;
            set => Set(ref _TransferType1, value);
        }

        #endregion

        #region Display Properties
        public string Factory_New_Display => $"{(NotifyAsNew ? "*" : "")}{Name}";
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
                        if (string.IsNullOrEmpty(Name)||string.IsNullOrWhiteSpace(Name))
                            return "Name of the factory must be set";

                        break;
                    case "CompanyName":
                        if (string.IsNullOrEmpty(CompanyName) || string.IsNullOrWhiteSpace(CompanyName))
                            return "Company Name factory must be set";

                        break;

                    case "Address":
                        if (string.IsNullOrEmpty(Address) || string.IsNullOrWhiteSpace(Address))
                            return "Company Name factory must be set";

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

            Global.Mapper.Map(this, factory);
            db.Factories.AddOrUpdate(factory);
            db.SaveChanges();
            Id = factory.Id;

            return true;
        }
    }
}
