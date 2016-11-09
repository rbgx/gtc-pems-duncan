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
    
    public partial class CustomerProfile
    {
        public CustomerProfile()
        {
            this.CustomerGrids = new HashSet<CustomerGrid>();
            this.CustomerSettings = new HashSet<CustomerSetting>();
            this.CustomerSettingsGroups = new HashSet<CustomerSettingsGroup>();
            this.MaintenanceGroupCustomers = new HashSet<MaintenanceGroupCustomer>();
            this.MaintenanceGroupCustomers1 = new HashSet<MaintenanceGroupCustomer>();
        }
    
        public int CustomerId { get; set; }
        public int Status { get; set; }
        public System.DateTime StatusChangeDate { get; set; }
        public string DisplayName { get; set; }
        public string DefaultLocale { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<bool> Is24HrFormat { get; set; }
        public Nullable<int> TimeZoneID { get; set; }
        public string PEMSConnectionStringName { get; set; }
        public string MaintenanceConnectionStringName { get; set; }
        public string ReportingConnectionStringName { get; set; }
        public int CustomerTypeId { get; set; }
        public string ConnectionStringName { get; set; }
    
        public virtual ICollection<CustomerGrid> CustomerGrids { get; set; }
        public virtual CustomerType CustomerType { get; set; }
        public virtual ICollection<CustomerSetting> CustomerSettings { get; set; }
        public virtual ICollection<CustomerSettingsGroup> CustomerSettingsGroups { get; set; }
        public virtual ICollection<MaintenanceGroupCustomer> MaintenanceGroupCustomers { get; set; }
        public virtual ICollection<MaintenanceGroupCustomer> MaintenanceGroupCustomers1 { get; set; }
    }
}