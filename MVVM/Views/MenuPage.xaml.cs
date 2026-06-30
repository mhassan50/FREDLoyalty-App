using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views
{
    public partial class MenuPage : ContentPage
    {
        private readonly MenuViewModel _vm;

        public MenuPage(MenuViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            _vm.Navigation = Navigation;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_vm.Categories.Count == 0)
                await _vm.LoadMenuAsync();
        }
        // // ── Item tapped — show bottom sheet ──────────────────
        // private async void OnItemTapped(object sender, TappedEventArgs e)
        // {
        //     if (e.Parameter is not Mdl_MenuItem item) return;

        //     // Populate sheet
        //     SheetName.Text        = item.DispItemName;
        //     SheetRate.Text        = item.HasRate
        //                                 ? $"Rs. {item.Rate}"
        //                                 : string.Empty;
        //     SheetRate.IsVisible   = item.HasRate;
        //     SheetDescription.Text = item.HasDescription
        //                                 ? item.DispItemDescription
        //                                 : "No description available.";

        //     if (item.HasImage)
        //     {
        //         SheetImage.Source    = item.ItemImage;
        //         SheetImage.IsVisible = true;
        //         SheetEmoji.IsVisible = false;
        //     }
        //     else
        //     {
        //         SheetImage.IsVisible = false;
        //         SheetEmoji.IsVisible = true;
        //     }

        //     // Animate in
        //     BottomSheetOverlay.IsVisible = true;
        //     BottomSheet.TranslationY     = 600;
        //     await BottomSheet.TranslateTo(0, 0, 280, Easing.CubicOut);
        // }

        // // ── Tap dim overlay to close ──────────────────────────
        // private async void OnOverlayTapped(object sender, TappedEventArgs e)
        //     => await CloseSheet();

        // private async Task CloseSheet()
        // {
        //     await BottomSheet.TranslateTo(0, 600, 220, Easing.CubicIn);
        //     BottomSheetOverlay.IsVisible = false;
        // }

        // protected override bool OnBackButtonPressed()
        // {
        //     if (BottomSheetOverlay.IsVisible)
        //     {
        //         _ = CloseSheet();
        //         return true;
        //     }
        //     return base.OnBackButtonPressed();
        // }


    }
}