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
    
    public partial class AcquirerBatch
    {
        public int AcquirerBatchId { get; set; }
        public int CustomerId { get; set; }
        public string BatchRef { get; set; }
        public int Status { get; set; }
        public System.DateTime OpenDateTime { get; set; }
        public Nullable<System.DateTime> ClosedDateTime { get; set; }
        public Nullable<System.DateTime> SettleDateTime { get; set; }
        public Nullable<int> SettleAttempt { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
