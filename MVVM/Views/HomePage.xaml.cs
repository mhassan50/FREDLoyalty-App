using CommunityToolkit.Maui.Extensions;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;
    private CancellationTokenSource _animCts;

    public HomePage()
    {
        InitializeComponent();
        _vm = new HomeViewModel(new LoyaltyService());
        _vm.Navigation = Navigation;
        BindingContext = _vm;

        //_vm.PropertyChanged += async (s, e) =>
        //{
        //    if (e.PropertyName == nameof(HomeViewModel.HasUnreadNotifications))
        //        await HandleNotifAnimationAsync();
        //};
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
        //await HandleNotifAnimationAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _animCts?.Cancel();
    }

    //private async Task HandleNotifAnimationAsync()
    //{
    //    _animCts?.Cancel();
    //    _animCts = new CancellationTokenSource();
    //    var token = _animCts.Token;

    //    if (!_vm.HasUnreadNotifications)
    //    {
    //        // Reset to default
    //        NotifBorder.BackgroundColor = Color.FromArgb("#15FFFFFF");
    //        NotifBorder.Stroke = new SolidColorBrush(Color.FromArgb("#40616b46"));
    //        return;
    //    }

    //    // Pulse between muted green and a warm amber highlight
    //    var defaultBg = Color.FromArgb("#15FFFFFF");
    //    var pulseBg = Color.FromArgb("#40F4C542");   // warm gold pulse
    //    var defaultStroke = Color.FromArgb("#40616b46");
    //    var pulseStroke = Color.FromArgb("#F4C542");

    //    try
    //    {
    //        while (!token.IsCancellationRequested)
    //        {
    //            await NotifBorder.BackgroundColorTo(pulseBg, 600);
    //            NotifBorder.Stroke = new SolidColorBrush(pulseStroke);
    //            if (token.IsCancellationRequested) break;

    //            await Task.Delay(600, token);

    //            _ = await NotifBorder.BackgroundColorTo(defaultBg, 600);
    //            NotifBorder.Stroke = new SolidColorBrush(defaultStroke);
    //            if (token.IsCancellationRequested) break;

    //            await Task.Delay(800, token);  // pause between pulses
    //        }
    //    }
    //    catch (TaskCanceledException) { }
    //}

    protected override bool OnBackButtonPressed()
    {
        _ = ConfirmExitAsync();
        return true;
    }

    private async Task ConfirmExitAsync()
    {
        bool answer = await DisplayAlert(
            "Exit App",
            "Are you sure you want to exit?",
            "Yes", "No");
        if (answer)
            Application.Current.Quit();
    }
}