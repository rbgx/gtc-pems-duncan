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
    
    public partial class MeterGroup
    {
        public MeterGroup()
        {
            this.AssetTypes = new HashSet<AssetType>();
            this.CashBoxes = new HashSet<CashBox>();
            this.EventCodeAssetTypes = new HashSet<EventCodeAssetType>();
            this.FDFileTypeMeterGroups = new HashSet<FDFileTypeMeterGroup>();
            this.MechanismMasters = new HashSet<MechanismMaster>();
            this.Meters = new HashSet<Meter>();
            this.ParkingSpaces = new HashSet<ParkingSpace>();
            this.Parts = new HashSet<Part>();
            this.Sensors = new HashSet<Sensor>();
            this.Transactions = new HashSet<Transaction>();
            this.VersionProfileMeters = new HashSet<VersionProfileMeter>();
        }
    
        public int MeterGroupId { get; set; }
        public string MeterGroupDesc { get; set; }
    
        public virtual ICollection<AssetType> AssetTypes { get; set; }
        public virtual ICollection<CashBox> CashBoxes { get; set; }
        public virtual ICollection<EventCodeAssetType> EventCodeAssetTypes { get; set; }
        public virtual ICollection<FDFileTypeMeterGroup> FDFileTypeMeterGroups { get; set; }
        public virtual ICollection<MechanismMaster> MechanismMasters { get; set; }
        public virtual ICollection<Meter> Meters { get; set; }
        public virtual ICollection<ParkingSpace> ParkingSpaces { get; set; }
        public virtual ICollection<Part> Parts { get; set; }
        public virtual ICollection<Sensor> Sensors { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<VersionProfileMeter> VersionProfileMeters { get; set; }
    }
}