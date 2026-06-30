namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_LoyaltyCounterBalance
    {
        public string LoyaltyCode { get; set; }
        public string LoyaltyName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal QtyPurchased { get; set; }
        public decimal QtyKnockOff { get; set; }
        public decimal xQtyPurchased { get; set; }
        public decimal QtyFreeAvailable { get; set; }
        public decimal QtyFreeConsumed { get; set; }
        public decimal xQtyFreeAvailable { get; set; }
    }

    public class Mdl_LoyaltyCounterBalanceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<Mdl_LoyaltyCounterBalance> Data { get; set; }
    }
    public class Mdl_LoyaltyCounterBalanceCriteria
    {
        public string CustomerCode { get; set; }
        public string LoyaltyCode { get; set; }
    }
}
