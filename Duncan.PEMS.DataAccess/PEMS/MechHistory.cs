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
    
    public partial class MechHistory
    {
        public int MechId { get; set; }
        public Nullable<int> HousingId { get; set; }
        public string Status { get; set; }
        public System.DateTime HistoryDate { get; set; }
        public string Notes { get; set; }
    }
}
