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
    
    public partial class MembershipEventType
    {
        public MembershipEventType()
        {
            this.AccessLogMembershipEvents = new HashSet<AccessLogMembershipEvent>();
        }
    
        public int EventTypeId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<AccessLogMembershipEvent> AccessLogMembershipEvents { get; set; }
    }
}
