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
    
    public partial class TransactionsPending
    {
        public long TransPendingID { get; set; }
        public Nullable<long> TransactionID { get; set; }
        public Nullable<int> AttemptCount { get; set; }
        public Nullable<int> Status { get; set; }
        public int CardTypeCode { get; set; }
    
        public virtual CardType CardType { get; set; }
        public virtual TransactionStatu TransactionStatu { get; set; }
    }
}
