//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duncan.PEMS.DataAccess.RBAC
{
    using System;
    using System.Collections.Generic;
    
    public partial class MaintenanceGroupCustomer
    {
        public int MaintenanceGroupCustomerId { get; set; }
        public int MaintenanceGroupId { get; set; }
        public int CustomerId { get; set; }
    
        public virtual CustomerProfile CustomerProfile { get; set; }
        public virtual CustomerProfile CustomerProfile1 { get; set; }
    }
}
