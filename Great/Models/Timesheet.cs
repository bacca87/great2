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
    
    public partial class Timesheet
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public Nullable<long> TravelStartTimeAM { get; set; }
        public Nullable<long> TravelEndTimeAM { get; set; }
        public Nullable<long> TravelStartTimePM { get; set; }
        public Nullable<long> TravelEndTimePM { get; set; }
        public Nullable<long> WorkStartTimeAM { get; set; }
        public Nullable<long> WorkEndTimeAM { get; set; }
        public Nullable<long> WorkStartTimePM { get; set; }
        public Nullable<long> WorkEndTimePM { get; set; }
        public Nullable<long> FDL { get; set; }
    
        public virtual FDL FDL1 { get; set; }
    }
}
