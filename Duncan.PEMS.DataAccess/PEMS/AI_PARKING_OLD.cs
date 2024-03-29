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
    
    public partial class AI_PARKING_OLD
    {
        public AI_PARKING_OLD()
        {
            this.AI_EXPORT_OLD = new HashSet<AI_EXPORT_OLD>();
            this.AI_PARK_NOTE_OLD = new HashSet<AI_PARK_NOTE_OLD>();
            this.AI_PARKING_VIOS_OLD = new HashSet<AI_PARKING_VIOS_OLD>();
        }
    
        public int ParkingId { get; set; }
        public System.DateTime IssueDateTime { get; set; }
        public string UnitSerial { get; set; }
        public string IssueNoPfx { get; set; }
        public long IssueNo { get; set; }
        public string IssueNoSfx { get; set; }
        public string IssueNoChkDgt { get; set; }
        public System.DateTime DueDate { get; set; }
        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public string Remark3 { get; set; }
        public string VoidStatus { get; set; }
        public System.DateTime VoidStatusDate { get; set; }
        public string VoidReason { get; set; }
        public string VoidedInField { get; set; }
        public string Reissued { get; set; }
        public int OfficerID { get; set; }
        public string Agency { get; set; }
        public string Beat { get; set; }
        public string LocLot { get; set; }
        public string LocBlock { get; set; }
        public string LocStreet { get; set; }
        public string LocDescriptor { get; set; }
        public string LocCrossStreet1 { get; set; }
        public string LocCrossStreet2 { get; set; }
        public float LocLatitude { get; set; }
        public float LocLongitude { get; set; }
        public string LocDirection { get; set; }
        public string LocCity { get; set; }
        public string LocState { get; set; }
        public int LocSuburb { get; set; }
        public string VehLicNo { get; set; }
        public string VehLicState { get; set; }
        public string VehVIN { get; set; }
        public string VehColor1 { get; set; }
        public string VehColor2 { get; set; }
        public string VehMake { get; set; }
        public string VehModel { get; set; }
        public string VehBodyStyle { get; set; }
        public System.DateTime VehLicExpDate { get; set; }
        public string VehVIN4 { get; set; }
        public string PermitNo { get; set; }
        public string MeterNo { get; set; }
        public string MeterBayNo { get; set; }
        public string IsWarning { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterID { get; set; }
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
    
        public virtual ICollection<AI_EXPORT_OLD> AI_EXPORT_OLD { get; set; }
        public virtual ICollection<AI_PARK_NOTE_OLD> AI_PARK_NOTE_OLD { get; set; }
        public virtual Meter Meter { get; set; }
        public virtual ParkingSpace ParkingSpace { get; set; }
        public virtual ICollection<AI_PARKING_VIOS_OLD> AI_PARKING_VIOS_OLD { get; set; }
    }
}
