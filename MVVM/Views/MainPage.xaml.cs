using FREDLoyalty_App.MVVM.Repos;

namespace FREDLoyalty_App.MVVM.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            _ = SecureStorageHelper.SetAsync("IsFirstTimeRegistration", "done");


        }

        private void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            _ = NavigateToRegistrationPageAsync();
        }

        private async Task NavigateToRegistrationPageAsync()
        {
            try
            {
                await Navigation.PushAsync(new RegistrationPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", "🔥 An error occurred: " + ex.Message, "Ok");
            }
        }

        private async void OnLoginClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new LoginPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", "🔥 An error occurred: " + ex.Message, "Ok");
            }
        }
    }
}
