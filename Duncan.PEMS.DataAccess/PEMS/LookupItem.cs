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
    
    public partial class LookupItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int PropertyGroupId { get; set; }
        public string ItemDesc { get; set; }
        public int SortOrder { get; set; }
    
        public virtual PropertyGroup PropertyGroup { get; set; }
    }
}