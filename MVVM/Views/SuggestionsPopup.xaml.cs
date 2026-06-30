using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views
{
    public partial class SuggestionPopup : ContentPage
    {
        public SuggestionPopup(RatingViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnOverlayTapped(object sender, TappedEventArgs e)
            => await ClosePopup();

        private async void OnCloseClicked(object sender, TappedEventArgs e)
            => await ClosePopup();

        private async Task ClosePopup()
        {
            await this.FadeTo(0, 180);
            await Navigation.PopModalAsync(false);
        }
    }
}