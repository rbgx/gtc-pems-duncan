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
    
    public partial class qTransaction
    {
        public int ReconFlags { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<int> MeterId { get; set; }
        public Nullable<System.DateTime> TransDateTime { get; set; }
        public int PaymentType { get; set; }
        public string PaymentTypeDesc { get; set; }
        public Nullable<int> BayNumber { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<int> TimePaid { get; set; }
        public Nullable<int> Status { get; set; }
        public string StatusDesc { get; set; }
        public int ReceiptNo { get; set; }
        public string TransactionID { get; set; }
        public Nullable<int> CreditCardType { get; set; }
        public string CreditCardTypeDesc { get; set; }
        public long TransactionsCardId { get; set; }
        public string Source { get; set; }
        public string AcquirerTransReference { get; set; }
        public string PaymentTarget { get; set; }
    }
}
