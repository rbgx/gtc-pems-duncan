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
    
    public partial class GSMConnectionLog
    {
        public Nullable<long> GlobalMeterId { get; set; }
        public System.DateTime StartDateTime { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterId { get; set; }
        public int PortNo { get; set; }
        public System.DateTime EndDateTime { get; set; }
        public int ConnectionStatus { get; set; }
        public string Memo { get; set; }
        public Nullable<long> EventUID { get; set; }
        public Nullable<int> TimeType1 { get; set; }
        public Nullable<int> TimeType2 { get; set; }
        public Nullable<int> TimeType3 { get; set; }
        public Nullable<int> TimeType4 { get; set; }
        public Nullable<int> TimeType5 { get; set; }
    
        public virtual GSMConnectionStatu GSMConnectionStatu { get; set; }
        public virtual Meter Meter { get; set; }
    }
}
