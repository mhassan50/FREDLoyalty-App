using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;

namespace FREDLoyalty_App.MVVM.Views;

public partial class AutoLoginPage : ContentPage
{
    private readonly ICommRepo _apiService;
    private bool _isNavigated;

    public AutoLoginPage()
    {
        InitializeComponent();
        _apiService = Handler?.MauiContext?.Services.GetService<ICommRepo>()
                      ?? new CommonRepo();
        Loaded += (s, e) => gifAnimation.IsAnimationPlaying = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_isNavigated) return;
        await Task.Delay(100); // let Activity finish onStart
        await RequestAllPermissionsAsync();

        await RunStartupAsync();
    }


    // ── Request every dangerous permission the app needs ─────────
    private async Task RequestAllPermissionsAsync()
    {
        // Camera
        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cameraStatus != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.Camera>();

        // Gallery / media (covers both old READ_EXTERNAL_STORAGE and new READ_MEDIA_IMAGES)
        var storageStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (storageStatus != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.Photos>();

        // Location (request fine — coarse is implied)
        var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (locationStatus != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        // Notifications (Android 13+ / API 33+)
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var notifStatus = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (notifStatus != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.PostNotifications>();
        }
#endif
    }


    private async Task RunStartupAsync()
    {
        progressBar.Progress = 0;

        try
        {
            var isFirstTime = await SecureStorageHelper.GetAsync("IsFirstTimeRegistration");
            var isRememberMe = await SecureStorageHelper.GetAsync("IsRememberMe");
            var email = await SecureStorageHelper.GetAsync("Email");
            var password = await SecureStorageHelper.GetAsync("Password");
            var googleId = await SecureStorageHelper.GetAsync("GoogleId");
            var appleId = await SecureStorageHelper.GetAsync("AppleId");

            if (string.IsNullOrEmpty(isFirstTime))
            {
                await progressBar.ProgressTo(1, 300, Easing.Linear);
                await Task.Delay(300);
                await SecureStorageHelper.SetAsync("IsFirstTimeRegistration", "done");
                await StopGifAsync();                                   // ← stop before navigate
                Application.Current.MainPage = new NavigationPage(new MainPage());
                return;
            }

            if (isRememberMe != "true" || string.IsNullOrEmpty(email))
            {
                await progressBar.ProgressTo(1, 300, Easing.Linear);
                await Task.Delay(300);
                await StopGifAsync();                                   // ← stop before navigate
                Application.Current.MainPage = new NavigationPage(new LoginPage());
                return;
            }

            var cts = new CancellationTokenSource();
            _ = AnimateProgressAsync(cts.Token);

            Mdl_CustomerResult result;

            if (!string.IsNullOrEmpty(appleId))
            {
                result = await _apiService.AppleAuthAsync(new Mdl_AppleAuth
                { Email = email, AppleId = appleId, Name = string.Empty });
            }
            else if (!string.IsNullOrEmpty(googleId))
            {
                result = await _apiService.GoogleAuthAsync(new Mdl_GoogleAuth
                { Email = email, GoogleId = googleId, Name = string.Empty });
            }
            else
            {
                result = await _apiService.LoginCustomerAsync(new Mdl_CustomerLogin
                { Email = email, Pwd = password ?? string.Empty });
            }

            cts.Cancel();
            await progressBar.ProgressTo(1, 300, Easing.Linear);
            await Task.Delay(300);

            await StopGifAsync();                                       // ← stop before navigate

            if (result?.IsSuccess == true)
            {
                App._Customer = result.Customer;
                App.Branches = result.Branches;
                await SecureStorageHelper.SetAsync("AuthToken", result.Token ?? string.Empty);

                Preferences.Set("remember_me", true);
                Preferences.Set("saved_email", email);

                await App.SetAppShellAfterLogin();
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutoLogin] Error: {ex.Message}");
            await StopGifAsync();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        _isNavigated = true;
    }

    // Stop the GIF animation safely before tearing down the page
    private async Task StopGifAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                gifAnimation.IsAnimationPlaying = false);
            await Task.Delay(50);   // let the native decoder thread settle
        }
        catch { /* ignore */ }
    }

    private async Task AnimateProgressAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && progressBar.Progress < 0.85)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                progressBar.ProgressTo(progressBar.Progress + 0.02, 80, Easing.Linear));
            await Task.Delay(80);
        }
    }
}