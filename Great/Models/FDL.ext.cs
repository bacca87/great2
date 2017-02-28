using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public partial class FDL : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public int Year { get { return Convert.ToInt32(Id.Substring(0, 4)); } }

        #region Display Properties
        public string FDL_Display { get { return Id + (IsExtra ? " (EXTRA)" : ""); } }
        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyFDLPropertiesChanged()
        {  
            OnPropertyChanged(nameof(Factory));
            OnPropertyChanged(nameof(Order));
            OnPropertyChanged(nameof(OutwardCar));
            OnPropertyChanged(nameof(OutwardTaxi));
            OnPropertyChanged(nameof(OutwardAircraft));
            OnPropertyChanged(nameof(ReturnCar));
            OnPropertyChanged(nameof(ReturnTaxi));
            OnPropertyChanged(nameof(ReturnAircraft));
            OnPropertyChanged(nameof(PerformanceDescription));
            OnPropertyChanged(nameof(Result));
            OnPropertyChanged(nameof(ResultNotes));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(PerformanceDescriptionDetails));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(LastError));

            OnPropertyChanged(nameof(FDLResult));
            OnPropertyChanged(nameof(Factory1));
        }

        public FDL Clone()
        {
            return new FDL()
            {
                Id = Id,
                IsExtra = IsExtra,
                WeekNr = WeekNr,
                FileName = FileName,
                Factory = Factory,
                Order = Order,
                OutwardCar = OutwardCar,
                ReturnCar = ReturnCar,
                OutwardTaxi = OutwardTaxi,
                ReturnTaxi = ReturnTaxi,
                OutwardAircraft = OutwardAircraft,
                ReturnAircraft = ReturnAircraft,
                PerformanceDescription = PerformanceDescription,
                Result = Result,
                ResultNotes = ResultNotes,
                Notes = Notes,
                PerformanceDescriptionDetails = PerformanceDescriptionDetails,
                Status = Status,
                LastError = LastError
            };
        }
    }
}
