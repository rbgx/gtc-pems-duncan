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
    
    public partial class Meter
    {
        public Meter()
        {
            this.ActiveAlarms = new HashSet<ActiveAlarm>();
            this.AuditLogs = new HashSet<AuditLog>();
            this.BaySnapshots = new HashSet<BaySnapshot>();
            this.CashBoxDataHistories = new HashSet<CashBoxDataHistory>();
            this.CashBoxDataHistories1 = new HashSet<CashBoxDataHistory>();
            this.CollDataImports = new HashSet<CollDataImport>();
            this.CollDataMeterStatus = new HashSet<CollDataMeterStatu>();
            this.CollDataScheds = new HashSet<CollDataSched>();
            this.CollDataSumms = new HashSet<CollDataSumm>();
            this.CollectionRunMeters = new HashSet<CollectionRunMeter>();
            this.CustomerBaseMeterFiles = new HashSet<CustomerBaseMeterFile>();
            this.CustomerFileArchives = new HashSet<CustomerFileArchive>();
            this.DiscountSchemeMeters = new HashSet<DiscountSchemeMeter>();
            this.EventLogs = new HashSet<EventLog>();
            this.FDJobs = new HashSet<FDJob>();
            this.GSMConnectionLogs = new HashSet<GSMConnectionLog>();
            this.MeterInventories = new HashSet<MeterInventory>();
            this.MeterStatusEvents = new HashSet<MeterStatusEvent>();
            this.OLTEventDetails = new HashSet<OLTEventDetail>();
            this.ParkingSchedules = new HashSet<ParkingSchedule>();
            this.ParkingSpaces = new HashSet<ParkingSpace>();
            this.PPOImports = new HashSet<PPOImport>();
            this.PublicHolidays = new HashSet<PublicHoliday>();
            this.PushStatus = new HashSet<PushStatu>();
            this.ScheduledMeters = new HashSet<ScheduledMeter>();
            this.SFMeterMaintenanceEvents = new HashSet<SFMeterMaintenanceEvent>();
            this.SLA_AssetDownTime = new HashSet<SLA_AssetDownTime>();
            this.SLA_OperationSchedule = new HashSet<SLA_OperationSchedule>();
            this.SLA_RegulatedSchedule = new HashSet<SLA_RegulatedSchedule>();
            this.Tariffs = new HashSet<Tariff>();
            this.TechCreditEvents = new HashSet<TechCreditEvent>();
            this.TransactionPackages = new HashSet<TransactionPackage>();
            this.TransactionsBlackLists = new HashSet<TransactionsBlackList>();
            this.TransactionsCashKeys = new HashSet<TransactionsCashKey>();
            this.TransactionsSmartCards = new HashSet<TransactionsSmartCard>();
            this.VehicleMovements = new HashSet<VehicleMovement>();
            this.VehicleSensingEvents = new HashSet<VehicleSensingEvent>();
            this.VersionProfileMeters = new HashSet<VersionProfileMeter>();
            this.VSNodeStatus = new HashSet<VSNodeStatu>();
            this.HistoricalAlarms = new HashSet<HistoricalAlarm>();
            this.MeterMaps = new HashSet<MeterMap>();
            this.MeterResetSchedules = new HashSet<MeterResetSchedule>();
            this.TransactionsCashes = new HashSet<TransactionsCash>();
            this.TransactionsMParks = new HashSet<TransactionsMPark>();
            this.TransactionsCreditCards = new HashSet<TransactionsCreditCard>();
        }
    
        public Nullable<long> GlobalMeterId { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterId { get; set; }
        public string SMSNumber { get; set; }
        public int MeterStatus { get; set; }
        public int TimeZoneID { get; set; }
        public int MeterRef { get; set; }
        public string EmporiaKey { get; set; }
        public string MeterName { get; set; }
        public string Location { get; set; }
        public Nullable<int> BayStart { get; set; }
        public Nullable<int> BayEnd { get; set; }
        public string Description { get; set; }
        public string GSMNumber { get; set; }
        public int SchedServTime { get; set; }
        public string RSFName { get; set; }
        public Nullable<System.DateTime> RSFDateTime { get; set; }
        public string BarCode { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public string ProgramName { get; set; }
        public Nullable<int> MaxBaysEnabled { get; set; }
        public Nullable<int> MeterType { get; set; }
        public Nullable<int> MeterGroup { get; set; }
        public Nullable<int> MParkID { get; set; }
        public Nullable<int> MeterState { get; set; }
        public Nullable<int> DemandZone { get; set; }
        public Nullable<int> TypeCode { get; set; }
        public Nullable<int> OperationalStatusID { get; set; }
        public Nullable<System.DateTime> InstallDate { get; set; }
        public Nullable<int> FreeParkingMinute { get; set; }
        public Nullable<int> RegulatedStatusID { get; set; }
        public Nullable<System.DateTime> WarrantyExpiration { get; set; }
        public Nullable<System.DateTime> OperationalStatusTime { get; set; }
        public Nullable<System.DateTime> LastPreventativeMaintenance { get; set; }
        public Nullable<System.DateTime> NextPreventativeMaintenance { get; set; }
        public Nullable<System.DateTime> OperationalStatusEndTime { get; set; }
        public string OperationalStatusComment { get; set; }
    
        public virtual ICollection<ActiveAlarm> ActiveAlarms { get; set; }
        public virtual Area Area { get; set; }
        public virtual AssetState AssetState { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<BaySnapshot> BaySnapshots { get; set; }
        public virtual ICollection<CashBoxDataHistory> CashBoxDataHistories { get; set; }
        public virtual ICollection<CashBoxDataHistory> CashBoxDataHistories1 { get; set; }
        public virtual ICollection<CollDataImport> CollDataImports { get; set; }
        public virtual ICollection<CollDataMeterStatu> CollDataMeterStatus { get; set; }
        public virtual ICollection<CollDataSched> CollDataScheds { get; set; }
        public virtual ICollection<CollDataSumm> CollDataSumms { get; set; }
        public virtual ICollection<CollectionRunMeter> CollectionRunMeters { get; set; }
        public virtual ICollection<CustomerBaseMeterFile> CustomerBaseMeterFiles { get; set; }
        public virtual ICollection<CustomerFileArchive> CustomerFileArchives { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual DemandZone DemandZone1 { get; set; }
        public virtual ICollection<DiscountSchemeMeter> DiscountSchemeMeters { get; set; }
        public virtual ICollection<EventLog> EventLogs { get; set; }
        public virtual ICollection<FDJob> FDJobs { get; set; }
        public virtual ICollection<GSMConnectionLog> GSMConnectionLogs { get; set; }
        public virtual MechanismMaster MechanismMaster { get; set; }
        public virtual MeterGroup MeterGroup1 { get; set; }
        public virtual ICollection<MeterInventory> MeterInventories { get; set; }
        public virtual OperationalStatu OperationalStatu { get; set; }
        public virtual RegulatedStatu RegulatedStatu { get; set; }
        public virtual TimeZone TimeZone { get; set; }
        public virtual ICollection<MeterStatusEvent> MeterStatusEvents { get; set; }
        public virtual ICollection<OLTEventDetail> OLTEventDetails { get; set; }
        public virtual ICollection<ParkingSchedule> ParkingSchedules { get; set; }
        public virtual ICollection<ParkingSpace> ParkingSpaces { get; set; }
        public virtual ICollection<PPOImport> PPOImports { get; set; }
        public virtual ICollection<PublicHoliday> PublicHolidays { get; set; }
        public virtual ICollection<PushStatu> PushStatus { get; set; }
        public virtual ICollection<ScheduledMeter> ScheduledMeters { get; set; }
        public virtual ICollection<SFMeterMaintenanceEvent> SFMeterMaintenanceEvents { get; set; }
        public virtual ICollection<SLA_AssetDownTime> SLA_AssetDownTime { get; set; }
        public virtual ICollection<SLA_OperationSchedule> SLA_OperationSchedule { get; set; }
        public virtual ICollection<SLA_RegulatedSchedule> SLA_RegulatedSchedule { get; set; }
        public virtual ICollection<Tariff> Tariffs { get; set; }
        public virtual ICollection<TechCreditEvent> TechCreditEvents { get; set; }
        public virtual ICollection<TransactionPackage> TransactionPackages { get; set; }
        public virtual ICollection<TransactionsBlackList> TransactionsBlackLists { get; set; }
        public virtual ICollection<TransactionsCashKey> TransactionsCashKeys { get; set; }
        public virtual ICollection<TransactionsSmartCard> TransactionsSmartCards { get; set; }
        public virtual TransDataSumm TransDataSumm { get; set; }
        public virtual ICollection<VehicleMovement> VehicleMovements { get; set; }
        public virtual ICollection<VehicleSensingEvent> VehicleSensingEvents { get; set; }
        public virtual ICollection<VersionProfileMeter> VersionProfileMeters { get; set; }
        public virtual ICollection<VSNodeStatu> VSNodeStatus { get; set; }
        public virtual ICollection<HistoricalAlarm> HistoricalAlarms { get; set; }
        public virtual ICollection<MeterMap> MeterMaps { get; set; }
        public virtual ICollection<MeterResetSchedule> MeterResetSchedules { get; set; }
        public virtual ICollection<TransactionsCash> TransactionsCashes { get; set; }
        public virtual ICollection<TransactionsMPark> TransactionsMParks { get; set; }
        public virtual ICollection<TransactionsCreditCard> TransactionsCreditCards { get; set; }
    }
}
