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
    
    public partial class SFMeterMaintenanceEvent
    {
        public long EventId { get; set; }
        public Nullable<long> GlobalMeterId { get; set; }
        public int CustomerId { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public System.DateTime EventDateTime { get; set; }
        public int MaintenanceCode { get; set; }
        public int TechnicianId { get; set; }
        public Nullable<int> WorkOrderID { get; set; }
    
        public virtual Meter Meter { get; set; }
    }
}