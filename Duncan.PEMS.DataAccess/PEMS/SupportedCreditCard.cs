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
    
    public partial class SupportedCreditCard
    {
        public int CustomerID { get; set; }
        public int CreditCardType { get; set; }
        public int BankID { get; set; }
        public int MerchantID { get; set; }
    
        public virtual Bank Bank { get; set; }
        public virtual CreditCardType CreditCardType1 { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
