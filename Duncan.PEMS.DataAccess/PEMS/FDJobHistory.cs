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
    
    public partial class FDJobHistory
    {
        public long JobHistID { get; set; }
        public long JobID { get; set; }
        public System.DateTime HistoryTS { get; set; }
        public int FDJobStatus { get; set; }
    
        public virtual FDJob FDJob { get; set; }
        public virtual FDJobStatu FDJobStatu { get; set; }
    }
}
