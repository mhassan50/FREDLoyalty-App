using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views
{
    public partial class ProgramsPopup : ContentPage
    {
        public ProgramsPopup(ProgramsViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ProgramsViewModel vm)
                await vm.LoadProgramsAsync();
        }

        private async void OnOverlayTapped(object sender, TappedEventArgs e)
            => await ClosePopup();

        private async void OnCloseClicked(object sender, TappedEventArgs e)
            => await ClosePopup();

        private async Task ClosePopup()
        {
            if (BindingContext is ProgramsViewModel vm)
                if (vm.IsLoading)
                    return; // Prevent closing while busy
            await this.FadeTo(0, 180);
            await Navigation.PopModalAsync(false);
        }
    }
}