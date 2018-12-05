using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("FDL")]
    public partial class FDL
    {
        public FDL()
        {
            this.ExpenseAccounts = new HashSet<ExpenseAccount>();
            this.Timesheets = new HashSet<Timesheet>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public long WeekNr { get; set; }
        public long StartDay { get; set; }
        public bool IsExtra { get; set; }
        public long? Factory { get; set; }
        public long Order { get; set; }
        public bool OutwardCar { get; set; }
        public bool ReturnCar { get; set; }
        public bool OutwardTaxi { get; set; }
        public bool ReturnTaxi { get; set; }
        public bool OutwardAircraft { get; set; }
        public bool ReturnAircraft { get; set; }
        public string PerformanceDescription { get; set; }
        public long Result { get; set; }
        public string ResultNotes { get; set; }
        public string Notes { get; set; }
        public string PerformanceDescriptionDetails { get; set; }
        public long Status { get; set; }
        public string LastError { get; set; }
        public string FileName { get; set; }
        public bool NotifyAsNew { get; set; }
    
        public virtual ICollection<ExpenseAccount> ExpenseAccounts { get; set; }
        [ForeignKey("Factory")]
        public virtual Factory Factory1 { get; set; }
        [ForeignKey("Status")]
        public virtual FDLStatus FDLStatus { get; set; }
        [ForeignKey("Result")]
        public virtual FDLResult FDLResult { get; set; }
        public virtual ICollection<Timesheet> Timesheets { get; set; }
    }
}
