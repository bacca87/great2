using GalaSoft.MvvmLight;
using Great.Models;
using Great.Models.DTO;
using Great.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.ViewModels.Database
{
    public class FDLEVM : ViewModelBase//, IFDLFile
    {
        //public string Id { get; set; }
        //public long WeekNr { get; set; }
        //public bool IsExtra { get; set; }
        //public long Order { get; set; }

        //public long? Factory { get; set; }
        //public bool OutwardCar { get; set; }
        //public bool ReturnCar { get; set; }
        //public bool OutwardTaxi { get; set; }
        //public bool ReturnTaxi { get; set; }
        //public bool OutwardAircraft { get; set; }
        //public bool ReturnAircraft { get; set; }
        //public string PerformanceDescription { get; set; }
        //public long Result { get; set; }
        //public string ResultNotes { get; set; }
        //public string Notes { get; set; }
        //public string PerformanceDescriptionDetails { get; set; }
        //public long Status { get; set; }
        //public string LastError { get; set; }
        //public string FileName { get; set; }
        //public bool NotifyAsNew { get; set; }

        //public virtual ICollection<ExpenseAccount> ExpenseAccounts { get; set; }
        //public virtual Factory Factory1 { get; set; }
        //public virtual FDLStatusDTO FDLStatus { get; set; }
        //public virtual FDLResult FDLResult { get; set; }
        //public virtual ICollection<Timesheet> Timesheets { get; set; }


        //public string FilePath { get { return ApplicationSettings.Directories.FDL + FileName; } }
       
        //public int Year { get { return Convert.ToInt32(Id.Substring(0, 4)); } }
        
        //public bool IsNew { get { return EStatus == EFDLStatus.New; } } // used for sorting purpose
       
        //public bool IsValid { get { return Timesheets.All(ts => ts.IsValid); } }

       
        //public EFDLStatus EStatus
        //{
        //    get
        //    {
        //        return (EFDLStatus)Status;
        //    }
        //    set
        //    {
        //        Status = (long)value;
        //    }
        //}

       
        //public EFDLResult EResult
        //{
        //    get
        //    {
        //        return (EFDLResult)Result;
        //    }
        //    set
        //    {
        //        Result = (long)value;
        //    }
        //}

        //#region Display Properties
        
        //public string FDL_Display { get { return $"{Id}{(IsExtra ? " (EXTRA)" : "")}"; } }
        
        //public string FDL_Factory_Display { get { return $"{Id}{(Factory1 != null ? $" [{Factory1.Name}]" : "")}{(IsExtra ? " (EXTRA)" : "")}"; } }
        //#endregion
    }
}
