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
    
    public partial class PAMMeterAccess
    {
        public int Customerid { get; set; }
        public int Clusterid { get; set; }
        public int MeterId { get; set; }
        public Nullable<decimal> LastAccessed { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
