using System.ComponentModel;

namespace Great.Models.Database
{
    public partial class CarRentalHistory : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
