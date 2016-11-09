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
    
    public partial class CollDataSumm
    {
        public Nullable<long> GlobalMeterID { get; set; }
        public int CustomerId { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public System.DateTime CollDateTime { get; set; }
        public int PaymentType { get; set; }
        public double Amount { get; set; }
        public string OldCashBoxID { get; set; }
        public string NewCashBoxID { get; set; }
        public Nullable<int> CashboxSequenceNo { get; set; }
        public Nullable<System.DateTime> InsertionDateTime { get; set; }
        public Nullable<bool> Processed { get; set; }
        public Nullable<long> EventUID { get; set; }
        public Nullable<System.DateTime> CreateTimestamp { get; set; }
        public Nullable<long> CollectionRunId { get; set; }
        public Nullable<int> CollectorId { get; set; }
    
        public virtual Meter Meter { get; set; }
        public virtual PaymentType PaymentType1 { get; set; }
    }
}
