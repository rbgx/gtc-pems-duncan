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
    
    public partial class Meters_CurrentState
    {
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterID { get; set; }
        public Nullable<System.DateTime> LastGSMOK { get; set; }
        public Nullable<System.DateTime> LastGSMFailed { get; set; }
    }
}
