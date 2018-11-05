
using System.ComponentModel;

namespace Great.Models
{
    public partial class Factory : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Factory Clone()
        {
            return new Factory() {
                Id = Id,
                Name = Name,
                CompanyName = CompanyName,
                Address = Address,
                TransferType = TransferType,
                IsForfait = IsForfait,
                Latitude = Latitude,
                Longitude = Longitude,
            };
        }
    }
}
