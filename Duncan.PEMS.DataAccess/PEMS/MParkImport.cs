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
    
    public partial class MParkImport
    {
        public Nullable<int> DRecType { get; set; }
        public string TransID { get; set; }
        public Nullable<int> TransAmtInCents { get; set; }
        public Nullable<int> NumOfMins { get; set; }
        public System.DateTime TransTimeStamp { get; set; }
        public Nullable<int> ReinoCustomerID { get; set; }
        public Nullable<int> ReinoAreaID { get; set; }
        public Nullable<int> ReinoMeterID { get; set; }
        public Nullable<int> BayNum { get; set; }
        public Nullable<int> UserType { get; set; }
        public string CallerId { get; set; }
        public string BatchIdent { get; set; }
        public Nullable<System.DateTime> BatchDate { get; set; }
        public string BatchFileName { get; set; }
    }
}