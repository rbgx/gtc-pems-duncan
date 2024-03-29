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
    
    public partial class OLTEventDetail
    {
        public Nullable<long> ParkingSpaceId { get; set; }
        public int CustomerId { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public System.DateTime EventDateTime { get; set; }
        public int EventCode { get; set; }
        public int ReceiptNumber { get; set; }
        public Nullable<int> PaidValue { get; set; }
        public Nullable<int> PaidUntil { get; set; }
        public Nullable<int> BayNumber { get; set; }
        public int TransReference { get; set; }
        public Nullable<int> TransStatus { get; set; }
        public Nullable<int> TransSubcode { get; set; }
        public Nullable<int> TransType { get; set; }
        public string PaymentTarget { get; set; }
        public Nullable<int> CashKeySrID { get; set; }
        public Nullable<int> CashKeyBalanceBefore { get; set; }
    
        public virtual CardType CardType { get; set; }
        public virtual Meter Meter { get; set; }
        public virtual ParkingSpace ParkingSpace { get; set; }
        public virtual PaymentTargetType PaymentTargetType { get; set; }
    }
}
