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
    
    public partial class CustomGroup1
    {
        public CustomGroup1()
        {
            this.MeterMapAudits = new HashSet<MeterMapAudit>();
            this.MeterMaps = new HashSet<MeterMap>();
        }
    
        public int CustomGroupId { get; set; }
        public int CustomerId { get; set; }
        public string DisplayName { get; set; }
        public string Comment { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual ICollection<MeterMapAudit> MeterMapAudits { get; set; }
        public virtual ICollection<MeterMap> MeterMaps { get; set; }
    }
}