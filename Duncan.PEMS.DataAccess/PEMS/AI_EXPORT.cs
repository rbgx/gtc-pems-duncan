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
    
    public partial class AI_EXPORT
    {
        public AI_EXPORT()
        {
            this.AI_PARKING_TRANSLINK = new HashSet<AI_PARKING_TRANSLINK>();
        }
    
        public int ExportId { get; set; }
        public int CustomerID { get; set; }
        public Nullable<System.DateTime> ExportDateTime { get; set; }
        public Nullable<int> ExportType { get; set; }
        public Nullable<int> ExportUserId { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<AI_PARKING_TRANSLINK> AI_PARKING_TRANSLINK { get; set; }
    }
}