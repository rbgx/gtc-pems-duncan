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
    
    public partial class WorkOrderEvent
    {
        public long WorkOrderEventId { get; set; }
        public long WorkOrderId { get; set; }
        public long EventId { get; set; }
        public int EventCode { get; set; }
        public Nullable<System.DateTime> EventDateTime { get; set; }
        public Nullable<System.DateTime> SLADue { get; set; }
        public string EventDesc { get; set; }
        public Nullable<int> AlarmTier { get; set; }
        public string Notes { get; set; }
        public Nullable<bool> Automated { get; set; }
        public Nullable<bool> Vandalism { get; set; }
        public Nullable<int> Status { get; set; }
    
        public virtual WorkOrder1 WorkOrder { get; set; }
    }
}
