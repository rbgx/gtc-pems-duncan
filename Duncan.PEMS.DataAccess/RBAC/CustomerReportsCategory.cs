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
    
    public partial class CustomerReportsCategory
    {
        public CustomerReportsCategory()
        {
            this.CustomerReports = new HashSet<CustomerReport>();
        }
    
        public int CustomerReportsCategoryId { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<CustomerReport> CustomerReports { get; set; }
    }
}
