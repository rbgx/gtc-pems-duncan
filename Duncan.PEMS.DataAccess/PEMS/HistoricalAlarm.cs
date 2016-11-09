//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duncan.PEMS.DataAccess.PEMS
{
    using System;
    using System.Collections.Generic;
    
    public partial class HistoricalAlarm
    {
        public Nullable<long> GlobalMeterId { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterId { get; set; }
        public int EventCode { get; set; }
        public int EventSource { get; set; }
        public System.DateTime TimeOfOccurrance { get; set; }
        public int EventState { get; set; }
        public System.DateTime TimeOfNotification { get; set; }
        public Nullable<System.DateTime> TimeOfClearance { get; set; }
        public Nullable<int> ClearingEventUID { get; set; }
        public int EventUID { get; set; }
        public Nullable<int> TimeType1 { get; set; }
        public Nullable<int> TimeType2 { get; set; }
        public Nullable<int> TimeType3 { get; set; }
        public Nullable<int> TimeType4 { get; set; }
        public Nullable<int> TimeType5 { get; set; }
        public Nullable<System.DateTime> SLADue { get; set; }
        public Nullable<int> WorkOrderId { get; set; }
        public Nullable<int> AlarmUID { get; set; }
        public Nullable<int> TargetServiceDesignation { get; set; }
        public Nullable<int> ClearedByUserId { get; set; }
        public string ClosureNote { get; set; }
        public string Notes { get; set; }
        public int DownTimeMinute { get; set; }
        public Nullable<System.DateTime> ProcessedTime { get; set; }
    
        public virtual Meter Meter { get; set; }
        public virtual TargetServiceDesignation TargetServiceDesignation1 { get; set; }
    }
}
