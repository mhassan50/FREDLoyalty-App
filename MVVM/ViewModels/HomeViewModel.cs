using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ILoyaltyService _loyaltyService;

        [ObservableProperty] private string welcomeNote;
        [ObservableProperty] private ObservableCollection<ImageItem> imageItems = new();
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string loyaltyName;
        [ObservableProperty] private decimal qtyFreeAvailable;
        [ObservableProperty] private decimal qtyFreeConsumed;
        [ObservableProperty] private decimal qtyPurchased;
        [ObservableProperty] private string progressHint;
        [ObservableProperty] private ObservableCollection<Mdl_LoyaltyOffer> offerHint;
        [ObservableProperty] private bool hasUnreadNotifications;


        public INavigation Navigation { get; set; }

        public HomeViewModel(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        public async Task InitializeAsync()
        {
            var customer = App._Customer;
            if (customer == null) return;

            WelcomeNote = $"Welcome, {customer.CustomerName} 👋";
            await LoadLoyaltyCardAsync(customer.CustomerCode.ToString());
            await CheckUnreadNotificationsAsync(customer.CustomerCode.ToString());
        }

        private async Task CheckUnreadNotificationsAsync(string customerCode)
        {
            try
            {
                var criteria = new Mdl_NotificationHistoryCriteria { CustomerCode = customerCode };
                var result = await _loyaltyService.GetNotificationHistoryAsync(criteria);

                if (result?.IsSuccess == true && result.Data?.Count > 0)
                {
                    // Get the last seen notification time from preferences
                    string lastSeenStr = Preferences.Get("last_notif_seen", string.Empty);

                    if (string.IsNullOrEmpty(lastSeenStr))
                    {
                        HasUnreadNotifications = true;
                        return;
                    }

                    var lastSeen = DateTime.Parse(lastSeenStr);
                    HasUnreadNotifications = result.Data.Any(n => n.SentAt > lastSeen);
                }
                else
                {
                    HasUnreadNotifications = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking notifications: {ex.Message}");
            }
        }
        private async Task LoadLoyaltyCardAsync(string customerCode)
        {
            try
            {
                IsLoading = true;

                var loyaltyResult = await _loyaltyService.GetLoyalties(new Mdl_LoyaltyCriteria
                {
                    BranchCode = 7,
                    IsActive = true
                });

                if (!loyaltyResult.IsSuccess || loyaltyResult.Data == null || !loyaltyResult.Data.Any())
                    return;

                var loyalty = loyaltyResult.Data.First();
                int totalSlots = (int)loyalty.PurchasedQty;

                var balanceResult = await _loyaltyService.GetLoyaltyCounterBalances(
                    new Mdl_LoyaltyCounterBalanceCriteria
                    {
                        CustomerCode = customerCode,
                        LoyaltyCode = loyalty.LoyaltyCode
                    });
                OfferHint = new ObservableCollection<Mdl_LoyaltyOffer>(loyalty.Offers);
                decimal boughtQty = 0;
                if (balanceResult.IsSuccess && balanceResult.Data?.Any() == true)
                {
                    var balance = balanceResult.Data.First();
                    boughtQty = balance.xQtyPurchased;
                    LoyaltyName = balance.LoyaltyName;
                    QtyPurchased = balance.xQtyPurchased;
                    QtyFreeAvailable = balance.xQtyFreeAvailable;
                    QtyFreeConsumed = balance.QtyFreeConsumed;
                }

                BuildLoyaltyIcons(totalSlots, (int)boughtQty);
                BuildProgressHint(totalSlots, (int)boughtQty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading loyalty card: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private void BuildLoyaltyIcons(int totalSlots, int purchasedCount)
        {
            // 5 rotating icon themes — repeats every 5
            var themes = new[]
            {
        new { Icon = "coffeewithtick1", Bg = "#d6e4f7", Border = "#3e67b0" }, // Blue
        new { Icon = "coffeewithtick2", Bg = "#fce4ea", Border = "#e91e47" }, // Red
        new { Icon = "coffeewithtick3", Bg = "#fae0f2", Border = "#d34798" }, // Pink
        new { Icon = "coffeewithtick4", Bg = "#fff9d6", Border = "#fdde00" }, // Yellow
        new { Icon = "coffeewithtick5", Bg = "#dff2e1", Border = "#70c17c" }, // Green
    };

            var items = new ObservableCollection<ImageItem>();

            for (int i = 0; i < totalSlots; i++)
            {
                bool isPurchased = i < purchasedCount;

                if (isPurchased)
                {
                    // Cycle through themes using modulo — repeats every 5
                    var theme = themes[i % 5];

                    items.Add(new ImageItem
                    {
                        ImageSource = theme.Icon,
                        BgColor = Color.FromArgb(theme.Bg),
                        BorderColor = Color.FromArgb(theme.Border),
                        Opacity = 1.0,
                        IsPurchased = true
                    });
                }
                else
                {
                    // Muted — unchanged
                    items.Add(new ImageItem
                    {
                        ImageSource = "coffeesvgrepo",
                        BgColor = Color.FromArgb("#E0E0E0"),
                        BorderColor = Color.FromArgb("#BDBDBD"),
                        Opacity = 0.55,
                        IsPurchased = false
                    });
                }
            }

            ImageItems = items;
        }


        //private void BuildLoyaltyIcons(int totalSlots, int purchasedCount)
        //{
        //    var items = new ObservableCollection<ImageItem>();

        //    for (int i = 0; i < totalSlots; i++)
        //    {
        //        bool isPurchased = i < purchasedCount;

        //        items.Add(isPurchased
        //            ? new ImageItem
        //            {
        //                // Purchased: dark espresso bg + gold border + green checkmark icon
        //                ImageSource = "coffeewithtick",
        //                BgColor = Color.FromArgb("#c8ceaf"),   // Primary — deep espresso
        //                BorderColor = Color.FromArgb("#4a5134"),  // Secondary — coffee gold
        //                Opacity = 1.0,
        //                IsPurchased = true
        //            }
        //            : new ImageItem
        //            {
        //                // Empty: light gold bg + muted brown border + faded icon
        //                ImageSource = "coffeesvgrepo",
        //                BgColor = Color.FromArgb("#E0E0E0"),   // Secondary — light gold
        //                BorderColor = Color.FromArgb("#BDBDBD"),  // Tertiary — medium coffee
        //                Opacity = 0.55,
        //                IsPurchased = false
        //            });
        //    }

        //    ImageItems = items;
        //}

        private void BuildProgressHint(int totalSlots, int purchasedCount)
        {

            if (QtyFreeAvailable > 0)
            {
                ProgressHint = $"You have {QtyFreeAvailable} free coffee{(QtyFreeAvailable > 1 ? "s" : "")} ready to claim! ☕";
                return;
            }

            int remaining = totalSlots - (purchasedCount % totalSlots);
            if (remaining == totalSlots)
            {
                ProgressHint = "Buy your first coffee to start earning! ☕";
            }
            else if (remaining == 1)
            {
                ProgressHint = "Just 1 more coffee until your FREE one! ☕";
            }
            else
            {
                ProgressHint = $"{remaining} more coffees until your FREE one ☕";
            }
        }

        [RelayCommand]
        private async Task OpenMenu()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                await Shell.Current.GoToAsync("Menu");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open menu page.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenQr()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                var popup = new QrPopup();
                popup.Opacity = 0;
                await Navigation.PushModalAsync(popup, false);
                await popup.FadeTo(1, 200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open QR page.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenOffers()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                var vm = new ProgramsViewModel(new LoyaltyService());
                var popup = new ProgramsPopup(vm);
                popup.Opacity = 0;
                await Navigation.PushModalAsync(popup, false);
                await popup.FadeTo(1, 200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open offers.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenHistory()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                await Shell.Current.GoToAsync("History");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open history page.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task RateUs()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                await Navigation.PushAsync(new BranchSelectionPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open branch selection.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task Suggestions()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                var vm = new RatingViewModel(new FeedbackService())
                {
                    Navigation = Navigation
                };
                var popup = new SuggestionPopup(vm);
                popup.Opacity = 0;
                await Navigation.PushModalAsync(popup, false);
                await popup.FadeTo(1, 200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open suggestions.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenMap()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                await Navigation.PushAsync(new LocationPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open locations page.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenProfile()
        {
            if (IsLoading) return;
            if (Navigation == null) return;
            try
            {
                IsLoading = true;
                await Navigation.PushAsync(new ProfilePage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open profile.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenNotifications()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                await Shell.Current.GoToAsync("NotificationHistory");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to open notifications.", "OK");
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (IsLoading) return;
            try
            {
                await Shell.Current.GoToAsync("//Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            if (IsLoading) return;

            bool answer = await Shell.Current.DisplayAlert(
                "Sign Out",
                "Are you sure you want to sign out?",
                "Yes", "Cancel");

            if (!answer) return;

            try
            {
                IsLoading = true;

                SecureStorageHelper.Remove("AuthToken");
                await SecureStorageHelper.SetAsync("IsRememberMe", "false");
                Preferences.Remove("remember_me");
                Preferences.Remove("saved_email");
                App._Customer = null;

                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Unable to sign out.", "OK");
            }
            finally { IsLoading = false; }
        }
    }
}
