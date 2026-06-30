using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.Views;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using Application = Microsoft.Maui.Controls.Application;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
#if IOS
using FREDLoyalty_App.Platforms.iOS;
#endif

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ICommRepo _apiService;
        private readonly GoogleAuthService _googleAuth;
#if IOS
        private readonly AppleAuthService _appleAuth;   // ← ADD
#endif



        public LoginViewModel(ICommRepo apiService)
        {
            _apiService = apiService;
            _googleAuth = new GoogleAuthService();
            #if IOS
                _appleAuth = new AppleAuthService();         // ← ADD
            #endif
        }

        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private bool isBusy = false;
        [ObservableProperty] private bool isRememberMe = false;
        [ObservableProperty] private bool isPasswordHidden = true;
        [ObservableProperty] private string eyeIcon = "eye.png";

        partial void OnIsRememberMeChanged(bool value)
            => _ = SecureStorageHelper.SetAsync("IsRememberMe", value ? "true" : "false");

        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                IsBusy = true;
                var result = await _apiService.LoginCustomerAsync(new Mdl_CustomerLogin
                {
                    Email = Email,
                    Pwd = Password
                });

                if (result?.IsSuccess == true)
                    await CompleteAsync(result, Email, Password, string.Empty, isGoogle: false, isApple: false);
                else
                    await Alert("Login Failed", result?.Message);
            }
            catch (Exception ex) { await Alert("Error", ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoogleLoginAsync()
        {
            try
            {
                IsBusy = true;
                var googleUser = await _googleAuth.SignInAsync();
                if (googleUser == null) return;

                var result = await _apiService.GoogleAuthAsync(new Mdl_GoogleAuth
                {
                    Email = googleUser.Email,
                    Name = googleUser.Name,
                    GoogleId = googleUser.GoogleId
                });

                if (result?.IsSuccess == true)
                    await CompleteAsync(result, googleUser.Email, string.Empty,
                        googleUser.GoogleId, isGoogle: true, isApple: false);
                else
                    await Alert("Google Sign-In Failed", result?.Message);
            }
            catch (Exception ex) { await Alert("Google Sign-In Error", ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task AppleLoginAsync()
        {
        #if IOS
            try
            {
                IsBusy = true;
                var appleUser = await _appleAuth.SignInAsync();
                if (appleUser == null) return;   // user cancelled

                var (email, name) = await AppleCacheHelper.ResolveAsync(appleUser);

                if (string.IsNullOrWhiteSpace(email))
                {
                    await Alert("Apple Sign-In",
                        "We couldn't retrieve your Apple email. Please try again, or sign in with another method.");
                    return;
                }

                var result = await _apiService.AppleAuthAsync(new Mdl_AppleAuth
                {
                    Email   = email,
                    Name    = name,
                    AppleId = appleUser.AppleId
                });

                if (result?.IsSuccess == true)
                    await CompleteAsync(result, email, string.Empty, appleUser.AppleId, isGoogle: false, isApple: true);
                else
                    await Alert("Apple Sign-In Failed", result?.Message);
            }
            catch (Exception ex) { await Alert("Apple Sign-In Error", ex.Message); }
            finally { IsBusy = false; }
        #else
                    await Task.CompletedTask;   // Apple button is hidden on Android; never reached
        #endif
        }


        [RelayCommand]
        private void TogglePassword()
        {
            IsPasswordHidden = !IsPasswordHidden;
            EyeIcon = IsPasswordHidden ? "eye.png" : "hidden.png";
        }

        private async Task CompleteAsync(
           Mdl_CustomerResult result,
           string email,
           string password,
           string socialId,
           bool isGoogle,
           bool isApple)
        {
            var customer = result.Customer ?? new Mdl_Customer();

            if (customer.CustomerCode == 0)
                customer.CustomerCode = result.CustomerCode ?? 0;
            if (string.IsNullOrEmpty(customer.Email))
                customer.Email = email;
            if (string.IsNullOrEmpty(customer.GoogleId) && isGoogle)
                customer.GoogleId = socialId;
            if (string.IsNullOrEmpty(customer.AppleId) && isApple)
                customer.AppleId = socialId;
            if (string.IsNullOrEmpty(customer.CustomerName))
                customer.CustomerName = email;

            App._Customer = customer;
            App.Branches = result.Branches ?? new List<Mdl_Branch>();

            await SecureStorageHelper.SetAsync("AuthToken", result.Token ?? string.Empty);
            await SecureStorageHelper.SetAsync("Email", email);
            await SecureStorageHelper.SetAsync("CustomerCode", customer.CustomerCode.ToString());
            await SecureStorageHelper.SetAsync("IsGoogle", isGoogle ? "true" : "false");
            await SecureStorageHelper.SetAsync("IsApple", isApple ? "true" : "false");

            if (isGoogle || isApple)
            {
                if (isGoogle) await SecureStorageHelper.SetAsync("GoogleId", socialId);
                if (isApple) await SecureStorageHelper.SetAsync("AppleId", socialId);
                await SecureStorageHelper.SetAsync("IsRememberMe", "true");
                Preferences.Set("remember_me", true);
                Preferences.Set("saved_email", email);
            }
            else
            {
                await SecureStorageHelper.SetAsync("Password", password);
                if (IsRememberMe)
                {
                    Preferences.Set("remember_me", true);
                    Preferences.Set("saved_email", email);
                }
                else
                {
                    Preferences.Remove("remember_me");
                    Preferences.Remove("saved_email");
                }
            }

            // Carry the social ID into verification so the right column is written
            if (result.NeedsVerification)
            {
                Application.Current.MainPage = new NavigationPage(new VerificationPage());
            }
            else
            {
                await App.SetAppShellAfterLogin();
            }
        }

        public async Task InitializeAsync()
        {
            var isGoogle = await SecureStorageHelper.GetAsync("IsGoogle");
            var isApple  = await SecureStorageHelper.GetAsync("IsApple");

            if (isGoogle == "true" || isApple == "true")
            {
                var savedEmail = await SecureStorageHelper.GetAsync("Email");
                if (!string.IsNullOrWhiteSpace(savedEmail))
                    Email = savedEmail;
            }
            else
            {
                var savedEmail = await SecureStorageHelper.GetAsync("Email");
                var savedPwd   = await SecureStorageHelper.GetAsync("Password");

                if (!string.IsNullOrWhiteSpace(savedEmail)) Email    = savedEmail;
                if (!string.IsNullOrWhiteSpace(savedPwd))   Password = savedPwd;
            }
        }

        private static Task Alert(string title, string msg)
            => App.Current.MainPage.DisplayAlert(title, msg ?? "Unknown error", "OK");
    }
}