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
    
    public partial class RateTransmissionDetail
    {
        public RateTransmissionDetail()
        {
            this.RateTransmissionJobs = new HashSet<RateTransmissionJob>();
        }
    
        public long ID { get; set; }
        public long TransmissionID { get; set; }
        public int SpaceNum { get; set; }
        public int RateInCent { get; set; }
    
        public virtual RateTransmission RateTransmission { get; set; }
        public virtual ICollection<RateTransmissionJob> RateTransmissionJobs { get; set; }
    }
}
