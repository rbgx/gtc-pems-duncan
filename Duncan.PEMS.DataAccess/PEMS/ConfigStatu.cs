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
    
    public partial class ConfigStatu
    {
        public ConfigStatu()
        {
            this.ConfigProfileSpaces = new HashSet<ConfigProfileSpace>();
        }
    
        public int ConfigStatusId { get; set; }
        public string ConfigStatusDest { get; set; }
    
        public virtual ICollection<ConfigProfileSpace> ConfigProfileSpaces { get; set; }
    }
}
