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
    
    public partial class AccessLogMembershipEvent
    {
        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
        public Nullable<int> EventTypeId { get; set; }
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
    
        public virtual MembershipEventType MembershipEventType { get; set; }
    }
}
