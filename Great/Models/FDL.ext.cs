using System;
using System.ComponentModel;
using System.Linq;

namespace Great.Models
{
    public partial class FDL : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public int Year { get { return Convert.ToInt32(Id.Substring(0, 4)); } }
        public bool IsNew { get { return EStatus == EFDLStatus.New; } } // used for sorting purpose
        public bool IsValid { get { return Timesheets.All(ts => ts.IsValid); } }
        
        public EFDLStatus EStatus
        {
            get
            {
                return (EFDLStatus)Status;
            }
            set
            {
                Status = (long)value;
            }
        }

        public EFDLResult EResult
        {
            get
            {
                return (EFDLResult)Result;
            }
            set
            {
                Result = (long)value;
            }
        }

        #region Display Properties
        public string FDL_Display { get { return $"{Id}{(IsExtra ? " (EXTRA)" : "")}"; } }
        public string FDL_Factory_Display { get { return $"{Id}{(Factory1 != null ? $" [{Factory1.Name}]" : "")}{(IsExtra ? " (EXTRA)" : "")}"; } }
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
            OnPropertyChanged(nameof(FDLStatu));
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
