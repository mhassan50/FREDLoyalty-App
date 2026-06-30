namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_NotificationHistory
    {
        public long LNo { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string LoyaltyCode { get; set; } = string.Empty;
        public string NotifType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class Mdl_NotificationHistoryResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Mdl_NotificationHistory> Data { get; set; } = new();
    }

    public class Mdl_NotificationHistoryCriteria
    {
        public string? CustomerCode { get; set; }
    }

    public class NotificationHistoryItemViewModel
    {
        public string NotifType { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public string StatusLabel { get; init; } = string.Empty;
        public Color StatusBadgeBgColor { get; init; } = Colors.Gray;
        public Color StatusBadgeTextColor { get; init; } = Colors.White;
        public Color TypeBadgeBgColor { get; init; } = Colors.Transparent;
        public Color TypeBadgeTextColor { get; init; } = Colors.Black;
        public Color DotColor { get; init; } = Colors.Gray;
        public string DayStr { get; init; } = string.Empty;
        public string MonthStr { get; init; } = string.Empty;
        public string YearStr { get; init; } = string.Empty;
        public string TimeStr { get; init; } = string.Empty;

    }
}

