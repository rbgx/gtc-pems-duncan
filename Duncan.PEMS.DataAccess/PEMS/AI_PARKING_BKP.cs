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
    
    public partial class AI_PARKING_BKP
    {
        public int ParkingId { get; set; }
        public System.DateTime IssueDateTime { get; set; }
        public string UnitSerial { get; set; }
        public string IssueNoPfx { get; set; }
        public long IssueNo { get; set; }
        public string IssueNoSfx { get; set; }
        public string IssueNoChkDgt { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public string Remark3 { get; set; }
        public string VoidStatus { get; set; }
        public Nullable<System.DateTime> VoidStatusDate { get; set; }
        public string VoidReason { get; set; }
        public string VoidedInField { get; set; }
        public string Reissued { get; set; }
        public Nullable<int> OfficerID { get; set; }
        public string Agency { get; set; }
        public string Beat { get; set; }
        public string LocLot { get; set; }
        public string LocBlock { get; set; }
        public string LocStreet { get; set; }
        public string LocDescriptor { get; set; }
        public string LocCrossStreet1 { get; set; }
        public string LocCrossStreet2 { get; set; }
        public Nullable<float> LocLatitude { get; set; }
        public Nullable<float> LocLongitude { get; set; }
        public string LocDirection { get; set; }
        public string LocCity { get; set; }
        public string LocState { get; set; }
        public Nullable<int> LocSuburb { get; set; }
        public string VehLicNo { get; set; }
        public string VehLicState { get; set; }
        public string VehVIN { get; set; }
        public string VehColor1 { get; set; }
        public string VehColor2 { get; set; }
        public string VehMake { get; set; }
        public string VehModel { get; set; }
        public string VehBodyStyle { get; set; }
        public Nullable<System.DateTime> VehLicExpDate { get; set; }
        public string VehVIN4 { get; set; }
        public string PermitNo { get; set; }
        public string MeterNo { get; set; }
        public string MeterBayNo { get; set; }
        public string IsWarning { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public string MeterID { get; set; }
        public long ParkingSpaceId { get; set; }
        public string VEHLICTYPE { get; set; }
        public string VEHLABELNO { get; set; }
        public string VEHCHECKDIGIT { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string LOCSIDEOFSTREET { get; set; }
        public string LOCZIP { get; set; }
        public string ViolationClass { get; set; }
        public string ViolationCode { get; set; }
        public string ViolationDesc { get; set; }
    }
}
