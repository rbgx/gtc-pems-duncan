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
    
    public partial class MechanismMaster
    {
        public MechanismMaster()
        {
            this.CashBoxes = new HashSet<CashBox>();
            this.FileTypeMaps = new HashSet<FileTypeMap>();
            this.Gateways = new HashSet<Gateway>();
            this.MechanismMasterCustomers = new HashSet<MechanismMasterCustomer>();
            this.Meters = new HashSet<Meter>();
            this.Parts = new HashSet<Part>();
            this.Sensors = new HashSet<Sensor>();
            this.DataKeys = new HashSet<DataKey>();
            this.MechMasters = new HashSet<MechMaster>();
            this.CSParks = new HashSet<CSPark>();
        }
    
        public int MechanismId { get; set; }
        public string MechanismDesc { get; set; }
        public Nullable<int> MeterGroupId { get; set; }
    
        public virtual ICollection<CashBox> CashBoxes { get; set; }
        public virtual ICollection<FileTypeMap> FileTypeMaps { get; set; }
        public virtual ICollection<Gateway> Gateways { get; set; }
        public virtual MeterGroup MeterGroup { get; set; }
        public virtual ICollection<MechanismMasterCustomer> MechanismMasterCustomers { get; set; }
        public virtual ICollection<Meter> Meters { get; set; }
        public virtual ICollection<Part> Parts { get; set; }
        public virtual ICollection<Sensor> Sensors { get; set; }
        public virtual ICollection<DataKey> DataKeys { get; set; }
        public virtual ICollection<MechMaster> MechMasters { get; set; }
        public virtual ICollection<CSPark> CSParks { get; set; }
    }
}
