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
    
    public partial class Tariff
    {
        public Tariff()
        {
            this.ParkingSchedules = new HashSet<ParkingSchedule>();
        }
    
        public Nullable<long> GlobalMeterID { get; set; }
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterId { get; set; }
        public int TariffID { get; set; }
        public int TariffAmount { get; set; }
        public int TimeUnitAmount { get; set; }
        public int TariffTimeUnit { get; set; }
        public int MaxParkingTime { get; set; }
        public int MaxParkingUnit { get; set; }
        public int LinkedTariffID { get; set; }
    
        public virtual Meter Meter { get; set; }
        public virtual ICollection<ParkingSchedule> ParkingSchedules { get; set; }
    }
}
