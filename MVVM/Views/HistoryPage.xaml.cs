using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Views;

public partial class HistoryPage : ContentPage
{
	private readonly HistoryViewModel _viewModel;
    public HistoryPage()
	{
		InitializeComponent();
        _viewModel = new HistoryViewModel(new LoyaltyService());
		BindingContext = _viewModel;
	}
    protected override bool OnBackButtonPressed()
    {
        // Intercept back button → navigate to home
        _ = OnBackButtonClicked(this, EventArgs.Empty);
        return true; // handled
    }

    private async Task OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

}