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
    
    public partial class SpaceType
    {
        public SpaceType()
        {
            this.ParkingSpaceDetails = new HashSet<ParkingSpaceDetail>();
        }
    
        public int SpaceTypeId { get; set; }
        public string SpaceTypeDesc { get; set; }
    
        public virtual ICollection<ParkingSpaceDetail> ParkingSpaceDetails { get; set; }
    }
}
