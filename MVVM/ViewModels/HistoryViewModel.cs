using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using System.Collections.ObjectModel;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly ILoyaltyService _loyaltyService;

        [ObservableProperty] private ObservableCollection<HistoryItem> historyItems = new();
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isEmpty;
        [ObservableProperty] private bool isNotEmpty;
        [ObservableProperty] private decimal totalPurchased;

        public INavigation Navigation { get; set; }

        public HistoryViewModel(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        public async Task InitializeAsync()
        {
            var customer = App._Customer;
            if (customer == null) return;

            await LoadHistoryAsync(customer.CustomerCode.ToString());
        }

        private async Task LoadHistoryAsync(string customerCode)
        {
            try
            {
                IsLoading = true;
                IsEmpty = false;
                IsNotEmpty = false;

                var result = await _loyaltyService.GetLoyaltyCounters(
                    new Mdl_LoyaltyCounterCriteria
                    {
                        CustomerCode = customerCode,
                        LoyaltyCode = "1"
                    });

                if (!result.IsSuccess || result.Data == null || !result.Data.Any())
                {
                    IsEmpty = true;
                    IsNotEmpty = false;
                    return;
                }

                // Sort descending by EntryTime
                var sorted = result.Data
                    .OrderByDescending(x => x.EntryTime)
                    .ToList();

                TotalPurchased = sorted.Sum(x => x.QtyPurchased);

                var items = new ObservableCollection<HistoryItem>();

                foreach (var record in sorted)
                {
                    bool isRedemption = record.QtyFreeConsumed > 0;
                    bool isKnockOff = record.QtyPurchased < 0 || record.QtyKnockOff > 0;

                    string typeTag;
                    Color dotColor, tagBg, tagText, badgeBg, badgeText;
                    string qtyLabel;

                    if (isRedemption)
                    {
                        typeTag = "REDEEMED";
                        dotColor = Color.FromArgb("#F4C542");
                        tagBg = Color.FromArgb("#F4C542");
                        tagText = Color.FromArgb("#3E2723");
                        qtyLabel = $"{record.QtyFreeConsumed} Consumed";
                        badgeBg = Color.FromArgb("#F4C542");
                        badgeText = Color.FromArgb("#3E2723");
                    }
                    else if (isKnockOff)
                    {
                        // Internal adjustment row — skip rendering entirely
                        continue;
                    }
                    else
                    {
                        typeTag = string.Empty;
                        dotColor = Application.Current.Resources["Primary"] is Color p ? p : Colors.Gray;
                        tagBg = Application.Current.Resources["White"] is Color w ? w : Colors.White;
                        tagText = Application.Current.Resources["Gray900"] is Color g9 ? g9 : Colors.Black;
                        qtyLabel = $"Quantity: {record.QtyPurchased}";
                        badgeBg = Application.Current.Resources["White"] is Color wb ? wb : Colors.White;
                        badgeText = Application.Current.Resources["Gray900"] is Color gb ? gb : Colors.Black;
                    }

                    items.Add(new HistoryItem
                    {
                        ItemName = record.ItemName,
                        LoyaltyName = record.LoyaltyName,
                        BranchName = record.BranchName,
                        VoucherLabel = $"Order #{record.VoucherNo}",
                        QtyLabel = qtyLabel,
                        TypeTag = typeTag,
                        DayStr = record.EntryTime?.ToString("dd") ?? "--",
                        MonthStr = record.EntryTime?.ToString("MMM").ToUpper() ?? "---",
                        YearStr = record.EntryTime?.ToString("yyyy").ToUpper() ?? "----",
                        TimeStr = record.EntryTime?.ToString("hh:mm tt") ?? "--:--",
                        DotColor = dotColor,
                        BadgeBgColor = badgeBg,
                        BadgeTextColor = badgeText,
                        TagBgColor = tagBg,
                        TagTextColor = tagText
                    });
                }

                HistoryItems = items;
                IsEmpty = false;
                IsNotEmpty = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HistoryViewModel] Error: {ex.Message}");
                IsEmpty = true;
                IsNotEmpty = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (IsLoading)
                return;
            if (Navigation != null)
		        await Shell.Current.GoToAsync("//More");
            else
                await Shell.Current.GoToAsync("..");
        }
    }
}
