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
    
    public partial class CreditCardAttempt
    {
        public long TransactionsCreditCardID { get; set; }
        public string ResultMsg { get; set; }
        public Nullable<System.DateTime> ResultDateTime { get; set; }
        public string ResultTrackingCode { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> BankServiceProviderID { get; set; }
    
        public virtual TransactionStatu TransactionStatu { get; set; }
    }
}
