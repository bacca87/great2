using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    public partial class Car : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Validation
        [NotMapped]
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(LicensePlate) || string.IsNullOrWhiteSpace(LicensePlate))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(Brand) || string.IsNullOrWhiteSpace(Brand))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(Model) || string.IsNullOrWhiteSpace(Model))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void NotifyCarPropertiesChanged()
        {
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(LicensePlate));
            OnPropertyChanged(nameof(Brand));
            OnPropertyChanged(nameof(Model));
            OnPropertyChanged(nameof(CarRentalCompany));
        }

        public Car Clone()
        {
            return new Car()
            {
                Id = Id,
                LicensePlate = LicensePlate,
                Brand = Brand,
                Model = Model,
                CarRentalCompany = CarRentalCompany
            };
        }
    }
}
