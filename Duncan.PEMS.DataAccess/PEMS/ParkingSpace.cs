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
    
    public partial class ParkingSpace
    {
        public ParkingSpace()
        {
            this.ConfigProfileSpaces = new HashSet<ConfigProfileSpace>();
            this.OLTEventDetails = new HashSet<OLTEventDetail>();
            this.ParkingSpaceDetails = new HashSet<ParkingSpaceDetail>();
            this.ParkingSpaceExpiryConfirmationEvents = new HashSet<ParkingSpaceExpiryConfirmationEvent>();
            this.ParkingSpaceMeterBayMaps = new HashSet<ParkingSpaceMeterBayMap>();
            this.ParkingSpaceOccupancies = new HashSet<ParkingSpaceOccupancy>();
            this.SensorPaymentTransactions = new HashSet<SensorPaymentTransaction>();
            this.SensorPaymentTransactionAudits = new HashSet<SensorPaymentTransactionAudit>();
            this.RegulatedHours = new HashSet<RegulatedHour>();
            this.Sensors = new HashSet<Sensor>();
            this.TransactionsCashKeys = new HashSet<TransactionsCashKey>();
            this.TransactionsSmartCards = new HashSet<TransactionsSmartCard>();
            this.TransactionsCashes = new HashSet<TransactionsCash>();
            this.TransactionsMParks = new HashSet<TransactionsMPark>();
            this.SensorPaymentTransactionCurrents = new HashSet<SensorPaymentTransactionCurrent>();
            this.TransactionsCreditCards = new HashSet<TransactionsCreditCard>();
        }
    
        public long ParkingSpaceId { get; set; }
        public int ServerID { get; set; }
        public int CustomerID { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public int BayNumber { get; set; }
        public System.DateTime AddedDateTime { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public Nullable<bool> HasSensor { get; set; }
        public Nullable<int> SpaceStatus { get; set; }
        public Nullable<System.DateTime> DateActivated { get; set; }
        public string Comments { get; set; }
        public string DisplaySpaceNum { get; set; }
        public Nullable<int> DemandZoneId { get; set; }
        public Nullable<System.DateTime> InstallDate { get; set; }
        public Nullable<int> ParkingSpaceType { get; set; }
        public Nullable<int> OperationalStatus { get; set; }
        public Nullable<System.DateTime> OperationalStatusTime { get; set; }
        public Nullable<System.DateTime> OperationalStatusEndTime { get; set; }
        public string OperationalStatusComment { get; set; }
    
        public virtual AssetState AssetState { get; set; }
        public virtual ICollection<ConfigProfileSpace> ConfigProfileSpaces { get; set; }
        public virtual DemandZone DemandZone { get; set; }
        public virtual MeterGroup MeterGroup { get; set; }
        public virtual Meter Meter { get; set; }
        public virtual ICollection<OLTEventDetail> OLTEventDetails { get; set; }
        public virtual OperationalStatu OperationalStatu { get; set; }
        public virtual ICollection<ParkingSpaceDetail> ParkingSpaceDetails { get; set; }
        public virtual ICollection<ParkingSpaceExpiryConfirmationEvent> ParkingSpaceExpiryConfirmationEvents { get; set; }
        public virtual ICollection<ParkingSpaceMeterBayMap> ParkingSpaceMeterBayMaps { get; set; }
        public virtual ICollection<ParkingSpaceOccupancy> ParkingSpaceOccupancies { get; set; }
        public virtual ICollection<SensorPaymentTransaction> SensorPaymentTransactions { get; set; }
        public virtual ICollection<SensorPaymentTransactionAudit> SensorPaymentTransactionAudits { get; set; }
        public virtual ICollection<RegulatedHour> RegulatedHours { get; set; }
        public virtual ICollection<Sensor> Sensors { get; set; }
        public virtual ICollection<TransactionsCashKey> TransactionsCashKeys { get; set; }
        public virtual ICollection<TransactionsSmartCard> TransactionsSmartCards { get; set; }
        public virtual ICollection<TransactionsCash> TransactionsCashes { get; set; }
        public virtual ICollection<TransactionsMPark> TransactionsMParks { get; set; }
        public virtual ICollection<SensorPaymentTransactionCurrent> SensorPaymentTransactionCurrents { get; set; }
        public virtual ICollection<TransactionsCreditCard> TransactionsCreditCards { get; set; }
    }
}
