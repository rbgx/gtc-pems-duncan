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
    
    public partial class SensorPaymentTransactionCurrent
    {
        public long ParkingSpaceId { get; set; }
        public long SensorPaymentTransactionID { get; set; }
        public System.DateTime ArrivalTime { get; set; }
        public long ArrivalPSOAuditId { get; set; }
        public Nullable<System.DateTime> DepartureTime { get; set; }
        public Nullable<long> DeparturePSOAuditId { get; set; }
        public Nullable<System.DateTime> FirstTxPaymentTime { get; set; }
        public Nullable<System.DateTime> FirstTxStartTime { get; set; }
        public Nullable<System.DateTime> FirstTxExpiryTime { get; set; }
        public Nullable<int> FirstTxAmountInCent { get; set; }
        public Nullable<int> FirstTxTimePaidMinute { get; set; }
        public Nullable<int> FirstTxPaymentMethod { get; set; }
        public Nullable<int> FirstTxID { get; set; }
        public Nullable<System.DateTime> LastTxPaymentTime { get; set; }
        public Nullable<System.DateTime> LastTxExpiryTime { get; set; }
        public Nullable<int> LastTxAmountInCent { get; set; }
        public Nullable<int> LastTxTimePaidMinute { get; set; }
        public Nullable<int> LastTxPaymentMethod { get; set; }
        public Nullable<int> LastTxID { get; set; }
        public Nullable<int> TotalAmountInCent { get; set; }
        public Nullable<int> TotalNumberOfPayment { get; set; }
        public Nullable<int> TotalTimePaidMinute { get; set; }
        public Nullable<int> TotalOccupiedMinute { get; set; }
        public Nullable<int> DiscountSchema { get; set; }
        public Nullable<int> GracePeriodMinute { get; set; }
        public Nullable<int> ViolationMinute { get; set; }
        public Nullable<int> OccupancyStatus { get; set; }
        public Nullable<int> NonCompliantStatus { get; set; }
        public Nullable<int> RemaingPaidTimeMinute { get; set; }
        public Nullable<System.DateTime> ZeroOutTime { get; set; }
        public Nullable<int> OperationalStatus { get; set; }
        public string InfringementLink { get; set; }
        public Nullable<System.DateTime> RecordCreatTime { get; set; }
        public Nullable<int> TimeType1 { get; set; }
        public Nullable<int> TimeType2 { get; set; }
        public Nullable<int> TimeType3 { get; set; }
        public Nullable<int> TimeType4 { get; set; }
        public Nullable<int> TimeType5 { get; set; }
        public Nullable<int> FreeParkingMinute { get; set; }
        public Nullable<System.DateTime> OccupancyDate { get; set; }
        public Nullable<int> ViolationSegmentCount { get; set; }
        public Nullable<int> FreeParkingTime { get; set; }
        public Nullable<bool> GracePeriodUsed { get; set; }
        public Nullable<int> SensorId { get; set; }
        public Nullable<int> GatewayId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public Nullable<bool> PrepayUsed { get; set; }
        public Nullable<bool> FreeParkingUsed { get; set; }
        public Nullable<int> SensorUndeterminedReason { get; set; }
        public Nullable<int> TotalVacantMinute { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Gateway Gateway { get; set; }
        public virtual NonCompliantStatu NonCompliantStatu { get; set; }
        public virtual OccupancyStatu OccupancyStatu { get; set; }
        public virtual OperationalStatu OperationalStatu { get; set; }
        public virtual ParkingSpaceOccupancyAudit ParkingSpaceOccupancyAudit { get; set; }
        public virtual ParkingSpaceOccupancyAudit ParkingSpaceOccupancyAudit1 { get; set; }
        public virtual ParkingSpace ParkingSpace { get; set; }
        public virtual SensorPaymentTransaction SensorPaymentTransaction { get; set; }
        public virtual Sensor Sensor { get; set; }
    }
}