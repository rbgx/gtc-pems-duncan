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
    
    public partial class NonComplianceRate_view
    {
        public string NonCompliance { get; set; }
        public string Suburb { get; set; }
        public int Areaid { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public string Street { get; set; }
        public string TimeClass { get; set; }
        public long SpaceId { get; set; }
        public string SpaceType { get; set; }
        public Nullable<System.DateTime> TimeOfEvent { get; set; }
        public Nullable<System.DateTime> EndTimeOfEvent { get; set; }
        public Nullable<int> TotalNumberOfSpaces { get; set; }
    }
}
