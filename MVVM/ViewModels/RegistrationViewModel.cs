using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.Views;
#if IOS
using FREDLoyalty_App.Platforms.iOS;
#endif

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class RegistrationViewModel : ObservableObject
    {
        private readonly ICommRepo _apiService;
        private readonly GoogleAuthService _googleAuth;
        #if IOS
                private readonly AppleAuthService _appleAuth;   // ← ADD
        #endif


        public RegistrationViewModel(ICommRepo apiService)
        {
            _apiService = apiService;
            _googleAuth = new GoogleAuthService();

            #if IOS
                _appleAuth = new AppleAuthService();
            #endif
            Days = Enumerable.Range(1, 31).Select(d => d.ToString()).ToList();
            Months = new List<string>
            {
                "January","February","March","April","May","June",
                "July","August","September","October","November","December"
            };
            Years = Enumerable.Range(1901, DateTime.Now.Year - 1901 + 1)
                              .Reverse()
                              .Select(y => y.ToString())
                              .ToList();
        }

        [ObservableProperty] private string fullName        = string.Empty;
        [ObservableProperty] private string email           = string.Empty;
        [ObservableProperty] private string mobile          = string.Empty;
        [ObservableProperty] private string password        = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private bool   isBusy         = false;
        [ObservableProperty] private string dobDisplay      = "Select date of birth";

        [ObservableProperty] private List<string> days   = new();
        [ObservableProperty] private List<string> months = new();
        [ObservableProperty] private List<string> years  = new();

        [ObservableProperty] private string selectedDay   = string.Empty;
        [ObservableProperty] private string selectedMonth = string.Empty;
        [ObservableProperty] private string selectedYear  = string.Empty;

        public void UpdateDobDisplay()
        {
            if (!string.IsNullOrEmpty(SelectedDay) &&
                !string.IsNullOrEmpty(SelectedMonth) &&
                !string.IsNullOrEmpty(SelectedYear))
                DobDisplay = $"{SelectedDay} {SelectedMonth} {SelectedYear}";
        }

        public DateTime? DateOfBirth
        {
            get
            {
                if (int.TryParse(SelectedDay, out int d) &&
                    int.TryParse(SelectedYear, out int y) &&
                    !string.IsNullOrEmpty(SelectedMonth))
                {
                    int m = Months.IndexOf(SelectedMonth) + 1;
                    try { return new DateTime(y, m, d); }
                    catch { return null; }
                }
                return null;
            }
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            try
            {
                if (Password != ConfirmPassword)
                {
                    await Alert("Error", "Passwords do not match.");
                    return;
                }

                IsBusy = true;

                var result = await _apiService.RegisterCustomerAsync(new Mdl_Customer
                {
                    CustomerName = FullName,
                    Email        = Email,
                    MobileNo     = Mobile,
                    Pwd          = Password,
                    DateOfBirth  = DateOfBirth
                });

                if (result?.IsSuccess == true)
                {
                    var customer = result.Customer ?? new Mdl_Customer();
                    if (customer.CustomerCode == 0)
                        customer.CustomerCode = result.CustomerCode ?? 0;

                    App._Customer = customer;
                    App.Branches  = result.Branches ?? new List<Mdl_Branch>();

                    await SecureStorageHelper.SetAsync("AuthToken",    result.Token ?? string.Empty);
                    await SecureStorageHelper.SetAsync("Email",        Email);
                    await SecureStorageHelper.SetAsync("Password",     Password);
                    await SecureStorageHelper.SetAsync("IsGoogle",     "false");
                    await SecureStorageHelper.SetAsync("IsApple",      "false");
                    await SecureStorageHelper.SetAsync("IsRememberMe", "true");
                    await SecureStorageHelper.SetAsync("CustomerCode", customer.CustomerCode.ToString());

                    Preferences.Set("remember_me", true);
                    Preferences.Set("saved_email", Email);

                    await App.SetAppShellAfterLogin();
                }
                else
                {
                    await Alert("Registration Failed", result?.Message);
                }
            }
            catch (Exception ex) { await Alert("Error", ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoogleRegisterAsync()
        {
            try
            {
                IsBusy = true;

                var googleUser = await _googleAuth.SignInAsync();
                if (googleUser == null) return;

                var result = await _apiService.GoogleAuthAsync(new Mdl_GoogleAuth
                {
                    Email    = googleUser.Email,
                    Name     = googleUser.Name,
                    GoogleId = googleUser.GoogleId
                });

                if (result?.IsSuccess == true)
                {
                    var customer = result.Customer ?? new Mdl_Customer();

                    if (customer.CustomerCode == 0)
                        customer.CustomerCode = result.CustomerCode ?? 0;
                    if (string.IsNullOrEmpty(customer.Email))
                        customer.Email = googleUser.Email;
                    if (string.IsNullOrEmpty(customer.GoogleId))
                        customer.GoogleId = googleUser.GoogleId;
                    if (string.IsNullOrEmpty(customer.CustomerName))
                        customer.CustomerName = googleUser.Name ?? googleUser.Email;

                    App._Customer = customer;
                    App.Branches  = result.Branches ?? new List<Mdl_Branch>();

                    await SecureStorageHelper.SetAsync("AuthToken",    result.Token ?? string.Empty);
                    await SecureStorageHelper.SetAsync("Email",        googleUser.Email);
                    await SecureStorageHelper.SetAsync("GoogleId",     googleUser.GoogleId);
                    await SecureStorageHelper.SetAsync("Password",     string.Empty);
                    await SecureStorageHelper.SetAsync("IsGoogle",     "true");
                    await SecureStorageHelper.SetAsync("IsApple",      "false");
                    await SecureStorageHelper.SetAsync("IsRememberMe", "true");
                    await SecureStorageHelper.SetAsync("CustomerCode", customer.CustomerCode.ToString());

                    Preferences.Set("remember_me", true);
                    Preferences.Set("saved_email", googleUser.Email);

                    if (result.NeedsVerification)
                        Application.Current.MainPage = new NavigationPage(new VerificationPage());
                    else
                        await App.SetAppShellAfterLogin();
                }
                else
                {
                    await Alert("Google Sign-Up Failed", result?.Message);
                }
            }
            catch (Exception ex) { await Alert("Google Sign-Up Error", ex.Message); }
            finally { IsBusy = false; }
        }


        [RelayCommand]
        private async Task AppleRegisterAsync()
        {
        #if IOS
            try
            {
                IsBusy = true;

                var appleUser = await _appleAuth.SignInAsync();
                if (appleUser == null) return;

                var (aEmail, aName) = await AppleCacheHelper.ResolveAsync(appleUser);

                if (string.IsNullOrWhiteSpace(aEmail))
                {
                    await Alert("Apple Sign-In",
                        "We couldn't retrieve your Apple email. Please try again.");
                    return;
                }

                var result = await _apiService.AppleAuthAsync(new Mdl_AppleAuth
                {
                    Email   = aEmail,
                    Name    = aName,
                    AppleId = appleUser.AppleId
                });

                if (result?.IsSuccess == true)
                {
                    var customer = result.Customer ?? new Mdl_Customer();

                    if (customer.CustomerCode == 0)
                        customer.CustomerCode = result.CustomerCode ?? 0;
                    if (string.IsNullOrEmpty(customer.Email))
                        customer.Email = aEmail;
                    if (string.IsNullOrEmpty(customer.AppleId))
                        customer.AppleId = appleUser.AppleId;
                    if (string.IsNullOrEmpty(customer.CustomerName))
                        customer.CustomerName = string.IsNullOrWhiteSpace(aName) ? aEmail : aName;

                    App._Customer = customer;
                    App.Branches  = result.Branches ?? new List<Mdl_Branch>();

                    await SecureStorageHelper.SetAsync("AuthToken",    result.Token ?? string.Empty);
                    await SecureStorageHelper.SetAsync("Email",        aEmail);
                    await SecureStorageHelper.SetAsync("AppleId",      appleUser.AppleId);
                    await SecureStorageHelper.SetAsync("Password",     string.Empty);
                    await SecureStorageHelper.SetAsync("IsGoogle",     "false");
                    await SecureStorageHelper.SetAsync("IsApple",      "true");
                    await SecureStorageHelper.SetAsync("IsRememberMe", "true");
                    await SecureStorageHelper.SetAsync("CustomerCode", customer.CustomerCode.ToString());

                    Preferences.Set("remember_me", true);
                    Preferences.Set("saved_email", aEmail);

                    if (result.NeedsVerification)
                        Application.Current.MainPage = new NavigationPage(new VerificationPage());
                    else
                        await App.SetAppShellAfterLogin();
                }
                else
                {
                    await Alert("Apple Sign-Up Failed", result?.Message);
                }
            }
            catch (Exception ex) { await Alert("Apple Sign-Up Error", ex.Message); }
            finally { IsBusy = false; }
        #else
                    await Task.CompletedTask;
        #endif
        }

        private static Task Alert(string title, string msg)
            => App.Current.MainPage.DisplayAlert(title, msg ?? "Unknown error", "OK");
    }
}