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
    
    public partial class VSNodeStatu
    {
        public Nullable<long> GlobalMeterId { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterId { get; set; }
        public int BitNumber { get; set; }
        public int NodeNumber { get; set; }
        public System.DateTime LastUpdateTS { get; set; }
        public int Status { get; set; }
    
        public virtual Meter Meter { get; set; }
    }
}
