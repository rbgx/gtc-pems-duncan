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
    
    public partial class DeviceUsageSummaryV
    {
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public int OfficerId { get; set; }
        public string OfficerName { get; set; }
        public string UnitSerial { get; set; }
        public System.DateTime IssueDateTime { get; set; }
        public long IssueNo { get; set; }
        public int CustomerID { get; set; }
    }
}