using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class MorePage : ContentPage
{
    public MorePage()
    {
        InitializeComponent();
        // Reuse HomeViewModel — it already has all the commands needed
        var vm = IPlatformApplication.Current?.Services
            .GetService<HomeViewModel>();

        if (vm != null)
        {
            vm.Navigation = Navigation;
            BindingContext = vm;
        }
    }
    protected override bool OnBackButtonPressed()
    {
        // Intercept back button → navigate to home
        _ = OnBackButtonClicked(this, EventArgs.Empty);
        return true; // handled
    }

    private async Task OnBackButtonClicked(object sender, EventArgs e)
    {
        _ = Navigation.PopAsync();
    }
}