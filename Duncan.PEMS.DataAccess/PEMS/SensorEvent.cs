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
    
    public partial class SensorEvent
    {
        public SensorEvent()
        {
            this.VehicleMovements = new HashSet<VehicleMovement>();
        }
    
        public int SensorEventId { get; set; }
        public string SensorDesc { get; set; }
    
        public virtual ICollection<VehicleMovement> VehicleMovements { get; set; }
    }
}
