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
    
    public partial class PaymentType
    {
        public PaymentType()
        {
            this.CollDataSumms = new HashSet<CollDataSumm>();
        }
    
        public int PaymentType1 { get; set; }
        public string PaymentTypeDesc { get; set; }
    
        public virtual ICollection<CollDataSumm> CollDataSumms { get; set; }
    }
}
