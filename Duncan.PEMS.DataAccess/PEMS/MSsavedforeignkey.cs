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
    
    public partial class MSsavedforeignkey
    {
        public string program_name { get; set; }
        public string constraint_name { get; set; }
        public string parent_schema { get; set; }
        public string parent_name { get; set; }
        public string referenced_object_schema { get; set; }
        public string referenced_object_name { get; set; }
        public bool is_disabled { get; set; }
        public bool is_not_for_replication { get; set; }
        public bool is_not_trusted { get; set; }
        public byte delete_referential_action { get; set; }
        public byte update_referential_action { get; set; }
        public System.DateTime timestamp { get; set; }
    }
}
