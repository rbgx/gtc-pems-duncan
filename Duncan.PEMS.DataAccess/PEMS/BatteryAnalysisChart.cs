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
    
    public partial class BatteryAnalysisChart
    {
        public Nullable<System.DateTime> DerivedTime { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public Nullable<int> MeterID { get; set; }
        public string MeterName { get; set; }
        public Nullable<System.DateTime> VoltageDate { get; set; }
        public Nullable<double> MinVolt { get; set; }
        public Nullable<double> MaxVolt { get; set; }
        public Nullable<int> CommsCnt { get; set; }
        public int RecordID { get; set; }
        public Nullable<int> deviceComms { get; set; }
        public string Area { get; set; }
        public string Zone { get; set; }
        public string Street { get; set; }
        public string Suburb { get; set; }
        public Nullable<int> AreaID { get; set; }
        public Nullable<int> ZoneID { get; set; }
    }
}