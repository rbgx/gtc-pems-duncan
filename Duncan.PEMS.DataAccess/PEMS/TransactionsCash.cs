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
    
    public partial class TransactionsCash
    {
        public long TransactionsCashID { get; set; }
        public Nullable<long> ParkingSpaceID { get; set; }
        public int CustomerId { get; set; }
        public int AreaId { get; set; }
        public int MeterId { get; set; }
        public System.DateTime TransDateTime { get; set; }
        public int BayNumber { get; set; }
        public int AmountInCents { get; set; }
        public int TimePaid { get; set; }
        public Nullable<int> CoinDollar2 { get; set; }
        public Nullable<int> CoinDollar1 { get; set; }
        public Nullable<int> CoinCent50 { get; set; }
        public Nullable<int> CoinCent20 { get; set; }
        public Nullable<int> CoinCent10 { get; set; }
        public Nullable<int> CoinCent5 { get; set; }
        public Nullable<int> CoinCent25 { get; set; }
        public Nullable<int> CoinCent1 { get; set; }
        public Nullable<int> TimePaidByRate { get; set; }
        public Nullable<System.DateTime> CreateDateTime { get; set; }
        public Nullable<bool> PrepayUsed { get; set; }
        public Nullable<bool> FreeParkingUsed { get; set; }
        public Nullable<bool> TopUp { get; set; }
        public Nullable<int> Coin1Cnt { get; set; }
        public Nullable<int> Coin2Cnt { get; set; }
        public Nullable<int> Coin3Cnt { get; set; }
        public Nullable<int> Coin4Cnt { get; set; }
        public Nullable<int> Coin5Cnt { get; set; }
        public Nullable<int> Coin6Cnt { get; set; }
        public Nullable<int> Coin7Cnt { get; set; }
        public Nullable<int> Coin8Cnt { get; set; }
        public Nullable<int> Coin9Cnt { get; set; }
        public Nullable<int> Coin10Cnt { get; set; }
        public Nullable<int> Coin11Cnt { get; set; }
        public Nullable<int> Coin12Cnt { get; set; }
        public Nullable<int> Coin13Cnt { get; set; }
        public Nullable<int> Coin14Cnt { get; set; }
        public Nullable<int> Coin15Cnt { get; set; }
        public Nullable<int> Coin16Cnt { get; set; }
        public Nullable<int> Coin17Cnt { get; set; }
        public Nullable<int> Coin18Cnt { get; set; }
        public Nullable<int> Coin19Cnt { get; set; }
        public Nullable<int> Coin20Cnt { get; set; }
        public Nullable<int> Coin21Cnt { get; set; }
        public Nullable<int> Coin22Cnt { get; set; }
        public Nullable<int> Coin23Cnt { get; set; }
        public Nullable<int> Coin24Cnt { get; set; }
        public Nullable<int> Coin25Cnt { get; set; }
        public Nullable<int> Coin26Cnt { get; set; }
        public Nullable<int> Coin27Cnt { get; set; }
        public Nullable<int> Coin28Cnt { get; set; }
        public Nullable<int> Coin29Cnt { get; set; }
        public Nullable<int> Coin30Cnt { get; set; }
        public Nullable<int> CoinUnknown { get; set; }
        public Nullable<int> CoinRejected { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> OLTCardBalance { get; set; }
    
        public virtual Meter Meter { get; set; }
        public virtual ParkingSpace ParkingSpace { get; set; }
    }
}