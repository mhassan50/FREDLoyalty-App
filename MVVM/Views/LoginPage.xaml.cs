using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel(new CommonRepo());
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    // Back → go to Welcome page
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }

    // Register → push RegistrationPage onto NavigationPage stack
    private async void OnRegisterClicked(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new RegistrationPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Google → push GoogleAuth page
    private async void OnGoogleLoginClicked(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new GoogleAuthDummyPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}