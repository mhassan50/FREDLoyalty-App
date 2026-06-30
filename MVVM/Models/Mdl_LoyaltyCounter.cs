namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_LoyaltyCounter
    {
        public int LNo { get; set; }
        public string LoyaltyCode { get; set; }
        public string LoyaltyName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal QtyPurchased { get; set; }
        public decimal QtyKnockOff { get; set; }
        public decimal QtyFreeAvailable { get; set; }
        public decimal QtyFreeConsumed { get; set; }
        public DateTime? DateFreeConsumed { get; set; }
        public string VoucherNo { get; set; }
        public int BranchCode { get; set; }
        public string BranchName { get; set; }
        public string UserName { get; set; }
        public DateTime? EntryTime { get; set; }
    }

    public class Mdl_LoyaltyCounterResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<Mdl_LoyaltyCounter> Data { get; set; }
    }
    public class Mdl_LoyaltyCounterCriteria
    {
        public string CustomerCode { get; set; }
        public string LoyaltyCode { get; set; }
    }
    // UI display model for the CollectionView
    public class HistoryItem
    {
        public string ItemName { get; set; }
        public string LoyaltyName { get; set; }  // ← new
        public string BranchName { get; set; }  // ← new
        public string VoucherLabel { get; set; }
        public string QtyLabel { get; set; }
        public string TypeTag { get; set; }
        public string DayStr { get; set; }
        public string MonthStr { get; set; }
        public string YearStr { get; set; }
        public string TimeStr { get; set; }

        public Color DotColor { get; set; }
        public Color BadgeBgColor { get; set; }
        public Color BadgeTextColor { get; set; }
        public Color TagBgColor { get; set; }
        public Color TagTextColor { get; set; }
    }

}
