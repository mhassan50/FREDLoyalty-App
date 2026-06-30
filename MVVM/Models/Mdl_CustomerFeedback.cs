namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_CustomerFeedback
    {
        public int LNo { get; set; }
        public int BranchCode { get; set; }
        public int CustomerCode { get; set; }
        public int Rating { get; set; }
        public string Feedback { get; set; }
        public DateTime EntryTime { get; set; }
    }

    public class Mdl_CustomerFeedbackResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
