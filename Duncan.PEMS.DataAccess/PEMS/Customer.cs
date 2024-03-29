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
    
    public partial class Customer
    {
        public Customer()
        {
            this.AcquirerBatches = new HashSet<AcquirerBatch>();
            this.AISyncupCustConfigs = new HashSet<AISyncupCustConfig>();
            this.Areas = new HashSet<Area>();
            this.AssetStateCustomers = new HashSet<AssetStateCustomer>();
            this.AssetTypes = new HashSet<AssetType>();
            this.BlackListActives = new HashSet<BlackListActive>();
            this.BlackListFiles = new HashSet<BlackListFile>();
            this.CashBoxes = new HashSet<CashBox>();
            this.CBImpFiles = new HashSet<CBImpFile>();
            this.CoinDenominationCustomers = new HashSet<CoinDenominationCustomer>();
            this.CollectionRunVendors = new HashSet<CollectionRunVendor>();
            this.CollRoutes = new HashSet<CollRoute>();
            this.CollRouteManualAmounts = new HashSet<CollRouteManualAmount>();
            this.CreditCardTypesCustomers = new HashSet<CreditCardTypesCustomer>();
            this.CustomerDetails = new HashSet<CustomerDetail>();
            this.CustomerPropertyGroups = new HashSet<CustomerPropertyGroup>();
            this.CustomGroup1 = new HashSet<CustomGroup1>();
            this.CustomGroup2 = new HashSet<CustomGroup2>();
            this.CustomGroup3 = new HashSet<CustomGroup3>();
            this.DemandZoneCustomers = new HashSet<DemandZoneCustomer>();
            this.DiscountSchemeCustomers = new HashSet<DiscountSchemeCustomer>();
            this.DiscountSchemes = new HashSet<DiscountScheme>();
            this.DiscountSchemeCustomerInfoes = new HashSet<DiscountSchemeCustomerInfo>();
            this.DiscountSchemeEmailTemplates = new HashSet<DiscountSchemeEmailTemplate>();
            this.EnforceRoutes = new HashSet<EnforceRoute>();
            this.EventCodes = new HashSet<EventCode>();
            this.Gateways = new HashSet<Gateway>();
            this.HousingMasters = new HashSet<HousingMaster>();
            this.MechanismMasterCustomers = new HashSet<MechanismMasterCustomer>();
            this.MeterDiagnosticTypeCustomers = new HashSet<MeterDiagnosticTypeCustomer>();
            this.Meters = new HashSet<Meter>();
            this.PAMBayExpts = new HashSet<PAMBayExpt>();
            this.PAMClusters = new HashSet<PAMCluster>();
            this.PAMCustomerMaps = new HashSet<PAMCustomerMap>();
            this.PAMMeterAccesses = new HashSet<PAMMeterAccess>();
            this.PaymentReceiveds = new HashSet<PaymentReceived>();
            this.Reports = new HashSet<Report>();
            this.ReportMasters = new HashSet<ReportMaster>();
            this.Schedules = new HashSet<Schedule>();
            this.SensorPaymentTransactions = new HashSet<SensorPaymentTransaction>();
            this.Sensors = new HashSet<Sensor>();
            this.SLA_Holiday = new HashSet<SLA_Holiday>();
            this.SLA_MaintenanceSchedule = new HashSet<SLA_MaintenanceSchedule>();
            this.SubAreas = new HashSet<SubArea>();
            this.SupportedCreditCards = new HashSet<SupportedCreditCard>();
            this.TechnicianKeyChangeLogs = new HashSet<TechnicianKeyChangeLog>();
            this.TimeTypeCustomers = new HashSet<TimeTypeCustomer>();
            this.WorkOrderCallers = new HashSet<WorkOrderCaller>();
            this.Zones = new HashSet<Zone>();
            this.ZoneSeqs = new HashSet<ZoneSeq>();
            this.DataKeys = new HashSet<DataKey>();
            this.AuditRegistries = new HashSet<AuditRegistry>();
            this.SensorPaymentTransactionCurrents = new HashSet<SensorPaymentTransactionCurrent>();
            this.PAMTxes = new HashSet<PAMTx>();
            this.AI_ACTIVITYLOG = new HashSet<AI_ACTIVITYLOG>();
            this.AI_EXPORT = new HashSet<AI_EXPORT>();
            this.AI_PARK_NOTE = new HashSet<AI_PARK_NOTE>();
            this.AI_PARKING_TRANSLINK = new HashSet<AI_PARKING_TRANSLINK>();
            this.MechMasters = new HashSet<MechMaster>();
            this.CollectionRunReports = new HashSet<CollectionRunReport>();
            this.AI_OFFICERS = new HashSet<AI_OFFICERS>();
            this.CSParks = new HashSet<CSPark>();
            this.CollectionRuns = new HashSet<CollectionRun>();
        }
    
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string FromEmailAddress { get; set; }
        public string BankHostAddr { get; set; }
        public Nullable<int> BankHostPort { get; set; }
        public Nullable<int> BankMaxThreads { get; set; }
        public Nullable<int> BankServiceProvider { get; set; }
        public string BankStoreID { get; set; }
        public Nullable<int> ResubmitProfileID { get; set; }
        public Nullable<int> UnReconcileCleanupLag { get; set; }
        public Nullable<bool> DoesCreditCashKeys { get; set; }
        public Nullable<int> doesBlacklistViaFD { get; set; }
        public Nullable<bool> BlackListCC { get; set; }
        public Nullable<int> TxTiming { get; set; }
        public string CityCode { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> CreateDateTime { get; set; }
        public string City { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public Nullable<int> SLAMinutes { get; set; }
        public bool IsEMV { get; set; }
        public bool IsPayByPhone { get; set; }
        public int GracePeriodMinute { get; set; }
        public int FreeParkingMinute { get; set; }
        public string DateTimeFormat { get; set; }
        public Nullable<int> TimeZoneID { get; set; }
        public Nullable<bool> ZeroOutMeter { get; set; }
        public Nullable<bool> Streetline { get; set; }
        public Nullable<bool> DiscountScheme { get; set; }
        public string CountryCode { get; set; }
        public string SLAMethod { get; set; }
        public Nullable<int> BankSettleTimeInMinute { get; set; }
    
        public virtual ICollection<AcquirerBatch> AcquirerBatches { get; set; }
        public virtual ICollection<AISyncupCustConfig> AISyncupCustConfigs { get; set; }
        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<AssetStateCustomer> AssetStateCustomers { get; set; }
        public virtual ICollection<AssetType> AssetTypes { get; set; }
        public virtual ICollection<BlackListActive> BlackListActives { get; set; }
        public virtual ICollection<BlackListFile> BlackListFiles { get; set; }
        public virtual ICollection<CashBox> CashBoxes { get; set; }
        public virtual ICollection<CBImpFile> CBImpFiles { get; set; }
        public virtual ICollection<CoinDenominationCustomer> CoinDenominationCustomers { get; set; }
        public virtual ICollection<CollectionRunVendor> CollectionRunVendors { get; set; }
        public virtual ICollection<CollRoute> CollRoutes { get; set; }
        public virtual ICollection<CollRouteManualAmount> CollRouteManualAmounts { get; set; }
        public virtual ICollection<CreditCardTypesCustomer> CreditCardTypesCustomers { get; set; }
        public virtual ICollection<CustomerDetail> CustomerDetails { get; set; }
        public virtual ICollection<CustomerPropertyGroup> CustomerPropertyGroups { get; set; }
        public virtual TimeZone TimeZone { get; set; }
        public virtual ICollection<CustomGroup1> CustomGroup1 { get; set; }
        public virtual ICollection<CustomGroup2> CustomGroup2 { get; set; }
        public virtual ICollection<CustomGroup3> CustomGroup3 { get; set; }
        public virtual ICollection<DemandZoneCustomer> DemandZoneCustomers { get; set; }
        public virtual ICollection<DiscountSchemeCustomer> DiscountSchemeCustomers { get; set; }
        public virtual ICollection<DiscountScheme> DiscountSchemes { get; set; }
        public virtual ICollection<DiscountSchemeCustomerInfo> DiscountSchemeCustomerInfoes { get; set; }
        public virtual ICollection<DiscountSchemeEmailTemplate> DiscountSchemeEmailTemplates { get; set; }
        public virtual ICollection<EnforceRoute> EnforceRoutes { get; set; }
        public virtual ICollection<EventCode> EventCodes { get; set; }
        public virtual ICollection<Gateway> Gateways { get; set; }
        public virtual ICollection<HousingMaster> HousingMasters { get; set; }
        public virtual ICollection<MechanismMasterCustomer> MechanismMasterCustomers { get; set; }
        public virtual ICollection<MeterDiagnosticTypeCustomer> MeterDiagnosticTypeCustomers { get; set; }
        public virtual ICollection<Meter> Meters { get; set; }
        public virtual PAMActiveCustomer PAMActiveCustomer { get; set; }
        public virtual ICollection<PAMBayExpt> PAMBayExpts { get; set; }
        public virtual ICollection<PAMCluster> PAMClusters { get; set; }
        public virtual ICollection<PAMCustomerMap> PAMCustomerMaps { get; set; }
        public virtual ICollection<PAMMeterAccess> PAMMeterAccesses { get; set; }
        public virtual ICollection<PaymentReceived> PaymentReceiveds { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<ReportMaster> ReportMasters { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<SensorPaymentTransaction> SensorPaymentTransactions { get; set; }
        public virtual ICollection<Sensor> Sensors { get; set; }
        public virtual ICollection<SLA_Holiday> SLA_Holiday { get; set; }
        public virtual ICollection<SLA_MaintenanceSchedule> SLA_MaintenanceSchedule { get; set; }
        public virtual ICollection<SubArea> SubAreas { get; set; }
        public virtual ICollection<SupportedCreditCard> SupportedCreditCards { get; set; }
        public virtual ICollection<TechnicianKeyChangeLog> TechnicianKeyChangeLogs { get; set; }
        public virtual ICollection<TimeTypeCustomer> TimeTypeCustomers { get; set; }
        public virtual ICollection<WorkOrderCaller> WorkOrderCallers { get; set; }
        public virtual ICollection<Zone> Zones { get; set; }
        public virtual ICollection<ZoneSeq> ZoneSeqs { get; set; }
        public virtual ICollection<DataKey> DataKeys { get; set; }
        public virtual ICollection<AuditRegistry> AuditRegistries { get; set; }
        public virtual ICollection<SensorPaymentTransactionCurrent> SensorPaymentTransactionCurrents { get; set; }
        public virtual ICollection<PAMTx> PAMTxes { get; set; }
        public virtual ICollection<AI_ACTIVITYLOG> AI_ACTIVITYLOG { get; set; }
        public virtual ICollection<AI_EXPORT> AI_EXPORT { get; set; }
        public virtual ICollection<AI_PARK_NOTE> AI_PARK_NOTE { get; set; }
        public virtual ICollection<AI_PARKING_TRANSLINK> AI_PARKING_TRANSLINK { get; set; }
        public virtual ICollection<MechMaster> MechMasters { get; set; }
        public virtual ICollection<CollectionRunReport> CollectionRunReports { get; set; }
        public virtual ICollection<AI_OFFICERS> AI_OFFICERS { get; set; }
        public virtual ICollection<CSPark> CSParks { get; set; }
        public virtual ICollection<CollectionRun> CollectionRuns { get; set; }
    }
}
