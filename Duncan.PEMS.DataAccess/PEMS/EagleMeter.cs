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
    
    public partial class EagleMeter
    {
        public Nullable<long> id { get; set; }
        public int CustomerID { get; set; }
        public string AreaID { get; set; }
        public string MeterID { get; set; }
        public string MeterName { get; set; }
        public string Location { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public int BayStart { get; set; }
        public int BayEnd { get; set; }
    }
}
