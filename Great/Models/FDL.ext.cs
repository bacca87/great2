using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public partial class FDL
    {
        #region Display Properties
        public string FDL_Display { get { return Year + "/" + Id.ToString().PadLeft(5, '0'); } }
        #endregion

        public FDL Clone()
        {
            return new FDL()
            {
                Id = Id,
                Year = Year,
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
