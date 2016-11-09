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
    
    public partial class LatestMeterDataAudit
    {
        public long AuditID { get; set; }
        public int latestmeterdataID { get; set; }
        public Nullable<int> CID { get; set; }
        public Nullable<int> AID { get; set; }
        public Nullable<int> MID { get; set; }
        public Nullable<decimal> Voltage { get; set; }
        public Nullable<System.DateTime> VoltTS { get; set; }
        public Nullable<int> IPPort { get; set; }
        public string IpAddress { get; set; }
        public Nullable<System.DateTime> IPSyncTS { get; set; }
        public string DK2MTR_MPBBin_Name { get; set; }
        public string DK2MTR_MPBCfg_Name { get; set; }
        public string DK2MTR_MBPgm_Name { get; set; }
        public string DK2MTR_Ccf_Name { get; set; }
        public string DK2MTR_RPG_Name { get; set; }
        public string DK2MTR_SNSR_Name { get; set; }
        public Nullable<System.DateTime> DK2MTR_MPBBin_Ts { get; set; }
        public Nullable<System.DateTime> DK2MTR_MPBCfg_Ts { get; set; }
        public Nullable<System.DateTime> DK2MTR_MBPgm_Ts { get; set; }
        public Nullable<System.DateTime> DK2MTR_Ccf_Ts { get; set; }
        public Nullable<System.DateTime> DK2MTR_RPG_Ts { get; set; }
        public Nullable<System.DateTime> DK2MTR_SNSR_Ts { get; set; }
        public string MTR2DK_MPBBin_Name { get; set; }
        public string MTR2DK_MPBCfg_Name { get; set; }
        public string MTR2DK_MBPgm_Name { get; set; }
        public string MTR2DK_Ccf_Name { get; set; }
        public string MTR2DK_RPG_Name { get; set; }
        public string MTR2DK_SNSR_Name { get; set; }
        public Nullable<System.DateTime> MTR2DK_MPBBin_Ts { get; set; }
        public Nullable<System.DateTime> MTR2DK_MPBCfg_Ts { get; set; }
        public Nullable<System.DateTime> MTR2DK_MBPgm_Ts { get; set; }
        public Nullable<System.DateTime> MTR2DK_Ccf_Ts { get; set; }
        public Nullable<System.DateTime> MTR2DK_RPG_Ts { get; set; }
        public Nullable<System.DateTime> MTR2DK_SNSR_Ts { get; set; }
        public Nullable<int> FD_MPBBin_FileId { get; set; }
        public Nullable<int> FD_MPBCfg_FileId { get; set; }
        public Nullable<int> FD_MBPgm_FileId { get; set; }
        public Nullable<int> FD_MBCcf_FileId { get; set; }
        public Nullable<int> FD_MBRpg_FileId { get; set; }
        public Nullable<int> FD_SNSR_FileId { get; set; }
        public Nullable<System.DateTime> FD_MPBBin_TS { get; set; }
        public Nullable<System.DateTime> FD_MPBCfg_TS { get; set; }
        public Nullable<System.DateTime> FD_MBPgm_TS { get; set; }
        public Nullable<System.DateTime> FD_MBCcf_TS { get; set; }
        public Nullable<System.DateTime> FD_MBRpg_TS { get; set; }
        public Nullable<System.DateTime> FD_SNSR_TS { get; set; }
        public Nullable<System.DateTime> dk2meter_xfer_ts { get; set; }
        public Nullable<System.DateTime> meter2dk_xfer_ts { get; set; }
        public string FD_MPBBin_FileStatus { get; set; }
        public string FD_MPBCfg_FileStatus { get; set; }
        public string FD_MBPgm_FileStatus { get; set; }
        public string FD_MBCcf_FileStatus { get; set; }
        public string FD_MBRpg_FileStatus { get; set; }
        public string FD_SNSR_FileStatus { get; set; }
        public Nullable<System.DateTime> Auditdate { get; set; }
        public Nullable<int> SpecialAction { get; set; }
        public Nullable<short> LobAutoSync { get; set; }
        public Nullable<System.DateTime> SpecialActionUpdateDateTime { get; set; }
        public Nullable<System.DateTime> LobAutoSyncUpdateDateTime { get; set; }
    
        public virtual LatestMeterData LatestMeterData { get; set; }
    }
}
