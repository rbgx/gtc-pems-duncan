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
    
    public partial class RouteMetersDetailsV2
    {
        public int CustomerID { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public System.DateTime DateTime { get; set; }
        public long CollectionRunId { get; set; }
        public long CollectionRunReportId { get; set; }
        public Nullable<int> AmountMeter { get; set; }
        public Nullable<int> AmountChip { get; set; }
        public string MeterName { get; set; }
        public string Area { get; set; }
        public string Zone { get; set; }
        public string Suburb { get; set; }
        public string DemandArea { get; set; }
        public string Street { get; set; }
    }
}