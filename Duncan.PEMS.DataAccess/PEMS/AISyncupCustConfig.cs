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
    
    public partial class AISyncupCustConfig
    {
        public AISyncupCustConfig()
        {
            this.AISyncupDataConfigs = new HashSet<AISyncupDataConfig>();
            this.AISyncupStatus = new HashSet<AISyncupStatu>();
        }
    
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string DBName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string NotificationEmailIDs { get; set; }
        public Nullable<System.DateTime> LastNotificationTS { get; set; }
        public string DatabaseType { get; set; }
        public string Drivers { get; set; }
        public string Dialect { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual ICollection<AISyncupDataConfig> AISyncupDataConfigs { get; set; }
        public virtual ICollection<AISyncupStatu> AISyncupStatus { get; set; }
    }
}
