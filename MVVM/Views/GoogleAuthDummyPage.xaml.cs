namespace FREDLoyalty_App.MVVM.Views;

public partial class GoogleAuthDummyPage : ContentPage
{
	public GoogleAuthDummyPage()
	{
		InitializeComponent();
	}
    private async void OnNextClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (email == "abc" && password == "1234")
        {
            // Navigate to Phone Verification Page
            await Shell.Current.GoToAsync(nameof(VerificationPage));
        }
        else
        {
            await DisplayAlert("Error", "Invalid credentials", "OK");
        }
    }

    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        _ = NavigateBackAsync();
    }

    private async Task NavigateBackAsync()
    {
        try
        {
            // await Shell.Current.Navigation.PushAsync(new TripTourHomeView());
            await Shell.Current.GoToAsync(nameof(RegistrationPage));

        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Alert", "🔥 An error occurred: " + ex.Message, "Ok");
        }

    }
}