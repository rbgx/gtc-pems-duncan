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
    
    public partial class MechanismMasterCustomer
    {
        public int MechanismMasterCustomerId { get; set; }
        public int MechanismId { get; set; }
        public int CustomerId { get; set; }
        public string MechanismDesc { get; set; }
        public bool IsDisplay { get; set; }
        public Nullable<int> SLAMinutes { get; set; }
        public Nullable<int> PreventativeMaintenanceScheduleDays { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual MechanismMaster MechanismMaster { get; set; }
    }
}