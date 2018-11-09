using Great.Utils.Extensions;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    public partial class CarRentalHistory : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        [NotMapped]
        public DateTime RentStartDate
        {
            get => DateTime.Now.FromUnixTimestamp(StartDate);
            set => StartDate = value.ToUnixTimestamp();
        }

        [NotMapped]
        public DateTime? RentEndDate
        {
            get
            {
                if (EndDate != null)
                {
                    return DateTime.Now.FromUnixTimestamp((long)EndDate);

                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    EndDate = ((DateTime)value).ToUnixTimestamp();
                    OnPropertyChanged(nameof(RentEndDate));
                }
                else
                {
                    EndDate = null;
                }
            }

        }

        #endregion

        #region Validation

        [NotMapped]
        public bool IsValid
        {
            get
            {
                if (StartKm > EndKm)
                {
                    return false;
                }

                if (RentStartDate > RentEndDate)
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

        public void NotifyCarRentalHistoryPropertiesChanged()
        {
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Car));
            OnPropertyChanged(nameof(StartKm));
            OnPropertyChanged(nameof(EndKm));
            OnPropertyChanged(nameof(StartLocation));
            OnPropertyChanged(nameof(EndLocation));
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(StartFuelLevel));
            OnPropertyChanged(nameof(EndFuelLevel));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(Car1));
        }

        public CarRentalHistory Clone()
        {
            return new CarRentalHistory()
            {
                Id = Id,
                Car = Car,
                StartKm = StartKm,
                EndKm = EndKm,
                StartLocation = StartLocation,
                EndLocation = EndLocation,
                StartDate = StartDate,
                EndDate = EndDate,
                StartFuelLevel = StartFuelLevel,
                EndFuelLevel = EndFuelLevel,
                Notes = Notes,
                Car1 = Car1
            };
        }
    }
}
