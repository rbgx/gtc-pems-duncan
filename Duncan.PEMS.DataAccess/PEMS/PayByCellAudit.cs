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
    
    public partial class PayByCellAudit
    {
        public int PayByCellAuditId { get; set; }
        public System.DateTime AuditDateTime { get; set; }
        public System.DateTime AuditStart { get; set; }
        public System.DateTime AuditEnd { get; set; }
        public int VendorTxCount { get; set; }
        public int VendorTotalAmountCent { get; set; }
        public int DuncanTxCount { get; set; }
        public int DuncanTotalAmountCent { get; set; }
        public int CustomerId { get; set; }
        public Nullable<int> VendorId { get; set; }
    }
}
