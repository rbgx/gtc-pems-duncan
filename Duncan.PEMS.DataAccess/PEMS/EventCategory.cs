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
    
    public partial class EventCategory
    {
        public EventCategory()
        {
            this.EventCodes = new HashSet<EventCode>();
        }
    
        public int EventCategoryId { get; set; }
        public string EventCategoryDesc { get; set; }
    
        public virtual ICollection<EventCode> EventCodes { get; set; }
    }
}