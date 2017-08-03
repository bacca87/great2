//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Great.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Expense
    {
        public long Id { get; set; }
        public long ExpenseAccount { get; set; }
        public long Type { get; set; }
        public Nullable<double> MondayAmount { get; set; }
        public Nullable<double> TuesdayAmount { get; set; }
        public Nullable<double> WednesdayAmount { get; set; }
        public Nullable<double> ThursdayAmount { get; set; }
        public Nullable<double> FridayAmount { get; set; }
        public Nullable<double> SaturdayAmount { get; set; }
        public Nullable<double> SundayAmount { get; set; }
    
        public virtual ExpenseType ExpenseType { get; set; }
        public virtual ExpenseAccount ExpenseAccount1 { get; set; }
    }
}
