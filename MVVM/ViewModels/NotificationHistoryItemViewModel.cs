// FREDLoyalty_App/MVVM/ViewModels/NotificationHistoryItemViewModel.cs
using FREDLoyalty_App.MVVM.Models;

namespace FREDLoyalty_App.MVVM.ViewModels
{
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
        public bool IsFailed => StatusLabel == "FAILED";

        public static NotificationHistoryItemViewModel From(Mdl_NotificationHistory m)
        {
            bool ok = m.IsSuccess;

            var (typeLabel, dotColor, badgeBg, badgeText) = m.NotifType?.ToLower() switch
            {
                "free_coffee" => (
                    "FREE COFFEE",
                    Color.FromArgb("#F4C542"),
                    Color.FromArgb("#FFF8E1"),
                    Color.FromArgb("#E65100")
                ),
                "milestone" => (
                    "MILESTONE",
                    Color.FromArgb("#7B61FF"),
                    Color.FromArgb("#EDE7F6"),
                    Color.FromArgb("#4527A0")
                ),
                "promo" => (
                    "PROMO",
                    Color.FromArgb("#29B6F6"),
                    Color.FromArgb("#E1F5FE"),
                    Color.FromArgb("#01579B")
                ),
                _ => (
                    (m.NotifType ?? "NOTIFICATION").Replace("_", " ").ToUpperInvariant(),
                    ok ? Color.FromArgb("#4CAF50") : Color.FromArgb("#EF5350"),
                    ok ? Color.FromArgb("#E8F5E9") : Color.FromArgb("#FFEBEE"),
                    ok ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828")
                )
            };

            return new NotificationHistoryItemViewModel
            {
                NotifType = typeLabel,
                Title = m.Title,
                Body = m.Body,
                StatusLabel = ok ? "DELIVERED" : "FAILED",
                StatusBadgeBgColor = ok ? Color.FromArgb("#E8F5E9") : Color.FromArgb("#FFEBEE"),
                StatusBadgeTextColor = ok ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828"),
                DotColor = dotColor,
                TypeBadgeBgColor = badgeBg,
                TypeBadgeTextColor = badgeText,
                DayStr = m.SentAt.ToString("dd"),
                MonthStr = m.SentAt.ToString("MMM").ToUpperInvariant(),
                YearStr = m.SentAt.ToString("yyyy"),
                TimeStr = m.SentAt.ToString("hh:mm tt"),
            };
        }
    }
}