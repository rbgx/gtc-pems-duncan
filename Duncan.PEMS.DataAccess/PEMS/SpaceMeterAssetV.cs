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
    
    public partial class SpaceMeterAssetV
    {
        public long ParkingSpaceId { get; set; }
        public Nullable<long> GlobalMeterId { get; set; }
        public int CustomerID { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public Nullable<int> AreaId2 { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public string Area { get; set; }
        public string Zone { get; set; }
        public string Street { get; set; }
        public string Suburb { get; set; }
        public string MeterName { get; set; }
        public string SensorName { get; set; }
        public string CashBoxName { get; set; }
        public string GatewayName { get; set; }
        public int BayNumber { get; set; }
        public string DemandArea { get; set; }
    }
}