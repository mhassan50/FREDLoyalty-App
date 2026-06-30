namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_CustomerSuggestion
    {
        public int LNo { get; set; }
        public int CustomerCode { get; set; }
        public string Suggestion { get; set; }
        public DateTime EntryTime { get; set; }
    }

    public class Mdl_CustomerSuggestionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
