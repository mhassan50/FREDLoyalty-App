using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class ProfilePage : ContentPage
{
	private readonly ProfileViewModel _viewModel;
    public ProfilePage()
	{
		InitializeComponent();
		_viewModel = new ProfileViewModel(new CommonRepo());
		_viewModel.Navigation = this.Navigation;
		this.BindingContext = _viewModel;
    }
    protected override bool OnBackButtonPressed()
    {
        // Intercept back button → navigate to home
        _ = OnBackButtonClicked(this, EventArgs.Empty);
        return true; // handled
    }

    private async Task OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//More");
    }
}