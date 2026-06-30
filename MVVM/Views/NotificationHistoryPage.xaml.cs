using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views
{
    public partial class NotificationHistoryPage : ContentPage
    {
        private readonly NotificationHistoryViewModel _vm;

        public NotificationHistoryPage()
        {
            InitializeComponent();
            _vm = new NotificationHistoryViewModel(new LoyaltyService());
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Stamp seen time before loading so the dot clears when user goes back
            Preferences.Set("last_notif_seen", DateTime.UtcNow.ToString("O"));
            await _vm.LoadAsync();
        }
    }
}