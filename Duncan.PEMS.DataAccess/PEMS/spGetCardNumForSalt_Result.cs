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
    
    public partial class spGetCardNumForSalt_Result
    {
        public int CustomerId { get; set; }
        public System.DateTime TransDateTime { get; set; }
        public string AreaName { get; set; }
        public string MeterName { get; set; }
        public long TransactionsCreditCardId { get; set; }
        public int TransType { get; set; }
        public string Location { get; set; }
        public Nullable<int> BayNumber { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<int> TimePaid { get; set; }
        public string CardNumHash { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreditCardTypeDesc { get; set; }
        public Nullable<int> CreditCardType { get; set; }
        public Nullable<int> ReceiptNo { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> UploadTimeBatch { get; set; }
        public Nullable<System.DateTime> UploadTimeOnline { get; set; }
    }
}
