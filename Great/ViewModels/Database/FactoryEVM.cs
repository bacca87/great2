using Great2.Models.Database;
using Great2.Models.DTO;
using System;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great2.ViewModels.Database
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
            set => SetAndCheckChanged(ref _Name, value);


        }

        private string _CompanyName;
        public string CompanyName
        {
            get => _CompanyName;
            set => SetAndCheckChanged(ref _CompanyName, value);

        }

        private string _Address;
        public string Address
        {
            get => _Address;
            set => SetAndCheckChanged(ref _Address, value);

        }

        private double? _Latitude;
        public double? Latitude
        {
            get => _Latitude;
            set => SetAndCheckChanged(ref _Latitude, value);

        }

        private double? _Longitude;
        public double? Longitude
        {
            get => _Longitude;
            set => SetAndCheckChanged(ref _Longitude, value);

        }

        private long _TransferType;
        public long TransferType
        {
            get => _TransferType;
            set => SetAndCheckChanged(ref _TransferType, value);
        }

        private bool _IsForfait;
        public bool IsForfait
        {
            get => _IsForfait;
            set => SetAndCheckChanged(ref _IsForfait, value);

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
            set => SetAndCheckChanged(ref _OverrideAddressOnFDL, value);
        }

        private string _CountryCode;
        public string CountryCode
        {
            get => _CountryCode;
            set => SetAndCheckChanged(ref _CountryCode, value);
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
                Auto.Mapper.Map(factory, this);
            IsChanged = false;
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
                Auto.Mapper.Map(f, this);
                return true;
            }

            return false;
        }

        public override bool Save(DBArchive db)
        {
            Factory factory = new Factory();

            Auto.Mapper.Map(this, factory);
            db.Factories.AddOrUpdate(factory);
            db.SaveChanges();
            Id = factory.Id;
            IsChanged = false;
            return true;
        }

        public long GetTransferType()
        {
            if (string.IsNullOrEmpty(CountryCode)) return 0;

            bool isItaly = CountryCode == "IT";

            bool isEurope = CountryCode == "AT";
            isEurope |= CountryCode == "BE";
            isEurope |= CountryCode == "BG";
            isEurope |= CountryCode == "CY";
            isEurope |= CountryCode == "CZ";
            isEurope |= CountryCode == "DE";
            isEurope |= CountryCode == "DK";
            isEurope |= CountryCode == "EE";
            isEurope |= CountryCode == "ES";
            isEurope |= CountryCode == "FI";
            isEurope |= CountryCode == "FR";
            isEurope |= CountryCode == "GB";
            isEurope |= CountryCode == "GR";
            isEurope |= CountryCode == "HR";
            isEurope |= CountryCode == "HU";
            isEurope |= CountryCode == "IE";
            isEurope |= CountryCode == "SK";
            isEurope |= CountryCode == "LT";
            isEurope |= CountryCode == "LU";
            isEurope |= CountryCode == "LV";
            isEurope |= CountryCode == "MT";
            isEurope |= CountryCode == "NL";
            isEurope |= CountryCode == "PO";
            isEurope |= CountryCode == "PT";
            isEurope |= CountryCode == "RO";
            isEurope |= CountryCode == "SE";
            isEurope |= CountryCode == "SI";

            bool isExtraEurope = !isItaly && !isEurope;

            if (isItaly) return 1;
            if (isEurope) return 2;
            if (isExtraEurope) return 3;

            else return 0;


        }
    }
}
