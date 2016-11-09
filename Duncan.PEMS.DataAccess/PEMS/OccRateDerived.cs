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
    
    public partial class OccRateDerived
    {
        public int RecordID { get; set; }
        public Nullable<System.DateTime> LastUpdatedTime { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public Nullable<int> MetersCnt { get; set; }
        public Nullable<int> OccupiedCnt { get; set; }
        public Nullable<int> VacantCnt { get; set; }
        public Nullable<int> ViolatedCnt { get; set; }
        public string PercentOccupied { get; set; }
        public string PercentVacant { get; set; }
        public string PercentViolated { get; set; }
        public Nullable<int> AreaID { get; set; }
        public string Area { get; set; }
        public Nullable<int> ZoneID { get; set; }
        public string Zone { get; set; }
        public string Street { get; set; }
        public string Suburb { get; set; }
        public Nullable<int> OccupancyStatus { get; set; }
        public string SpaceStatus { get; set; }
        public string ViolationType { get; set; }
        public Nullable<System.DateTime> DerivedTime { get; set; }
    }
}
