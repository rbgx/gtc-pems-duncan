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
    
    public partial class RegulatedStatu
    {
        public RegulatedStatu()
        {
            this.Meters = new HashSet<Meter>();
        }
    
        public int RegulatedStatusID { get; set; }
        public string RegulatedStatusDesc { get; set; }
    
        public virtual ICollection<Meter> Meters { get; set; }
    }
}
