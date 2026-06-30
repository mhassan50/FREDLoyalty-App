namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_Loyalty
    {
        public string LoyaltyCode { get; set; }
        public string LoyaltyName { get; set; }
        public DateTime? LoyaltyDtStart { get; set; }
        public DateTime? LoyaltyDtEnd { get; set; }
        public decimal PurchasedQty { get; set; }
        public decimal FreeQty { get; set; }
        public string BranchCode { get; set; }
        public bool IsActive { get; set; }
        public string UserName { get; set; }
        public DateTime? EntryTime { get; set; }
        public string UserNameLM { get; set; }
        public DateTime? EntryTimeLM { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }
        public bool IsAvailable { get; set; }
        public List<Mdl_LoyaltyOffer> Offers { get; set; } = new();
    }

    public class Mdl_LoyaltyOffer
    {
        public string LoyaltyCode { get; set; }
        public int LNo { get; set; }
        public string Offer { get; set; }
    }
    public class Mdl_LoyaltyResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<Mdl_Loyalty> Data { get; set; }
    }
    public class Mdl_LoyaltyCriteria
    {
        public int BranchCode { get; set; }
        public bool? IsActive { get; set; }
    }
}
