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
    
    public partial class CoinDenominationCustomer
    {
        public int CoinDenominationCustomerId { get; set; }
        public int CoinDenominationId { get; set; }
        public int CustomerId { get; set; }
        public string CoinName { get; set; }
        public bool IsDisplay { get; set; }
        public Nullable<int> CoinTypeOrdinal { get; set; }
    
        public virtual CoinDenomination CoinDenomination { get; set; }
        public virtual Customer Customer { get; set; }
    }
}