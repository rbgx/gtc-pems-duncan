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
    
    public partial class qParkingSchedule
    {
        public int CustomerID { get; set; }
        public int AreaID { get; set; }
        public int MeterID { get; set; }
        public System.DateTime StartTime { get; set; }
        public int OperationMode { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public int DayOfWeek { get; set; }
        public int TariffID { get; set; }
        public Nullable<int> IntervalNum { get; set; }
        public int ScheduleID { get; set; }
    }
}
