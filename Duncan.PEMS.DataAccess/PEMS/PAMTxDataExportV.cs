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
    
    public partial class PAMTxDataExportV
    {
        public Nullable<long> id { get; set; }
        public int CustomerId { get; set; }
        public int MeterId { get; set; }
        public int Areaid { get; set; }
        public int bay { get; set; }
        public Nullable<System.DateTime> TRANSDATETIME { get; set; }
        public Nullable<System.DateTime> Expiry { get; set; }
        public Nullable<int> SequenceId { get; set; }
        public Nullable<int> TransactionType { get; set; }
        public Nullable<int> AmountCent { get; set; }
        public Nullable<int> Duration { get; set; }
        public string TransactionTypeDesc { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public Nullable<System.DateTime> Received { get; set; }
    }
}
