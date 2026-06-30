using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.Views;
using SkiaSharp;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly ICommRepo _apiService;

        // ── Profile fields ───────────────────────────────────────
        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string mobileNo = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private DateTime dateOfBirth = DateTime.Today.AddYears(-18);
        [ObservableProperty] private string profilePictureUrl = string.Empty;
        [ObservableProperty] private bool isGoogleUser = false;
        [ObservableProperty] private bool isAppleUser = false;   // ← ADD

        [ObservableProperty] private bool isBusy = false;
        [ObservableProperty] private bool isPasswordHidden = true;
        [ObservableProperty] private string eyeIcon = "eye.png";
        [ObservableProperty] private bool isNewPasswordHidden = true;
        [ObservableProperty] private string newEyeIcon = "eye.png";
        [ObservableProperty] private bool isConfirmPasswordHidden = true;
        [ObservableProperty] private string confirmEyeIcon = "eye.png";

        // ── Password fields ──────────────────────────────────────
        [ObservableProperty] private string currentPassword = string.Empty;
        [ObservableProperty] private string newPassword = string.Empty;
        [ObservableProperty] private string confirmNewPassword = string.Empty;
        [ObservableProperty] private string passwordError = string.Empty;
        // ── Profile fields — add these two ──────────────────────
        [ObservableProperty] private ImageSource profileImageSource;
        [ObservableProperty] private string snapBase64 = string.Empty;

        // ── Computed properties ──────────────────────────────────
     //   public bool HasProfilePicture => !string.IsNullOrWhiteSpace(ProfilePictureUrl);
     //   public bool HasNoProfilePicture => !HasProfilePicture;

        public bool IsSocialUser => IsGoogleUser || IsAppleUser;
        public bool IsNotSocialUser => !IsSocialUser;
        public bool IsNotGoogleUser => !IsGoogleUser;
        public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName)) return "?";
                var parts = FullName.Trim().Split(' ');
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
                return FullName[0].ToString().ToUpper();
            }
        }
        public bool ShowAppleBadge =>
        #if IOS
            IsAppleUser;
        #else
            false;
        #endif
        public INavigation Navigation { get; set; }

        public ProfileViewModel(ICommRepo apiService)
        {
            _apiService = apiService;
            LoadFromCustomer();
        }

        // ── Load current customer data ───────────────────────────
        private async Task LoadFromCustomer()
        {
            var c = App._Customer;
            if (c == null) return;

            FullName = c.CustomerName ?? string.Empty;
            MobileNo = c.MobileNo ?? string.Empty;
            Email = c.Email ?? string.Empty;
            DateOfBirth = c.DateOfBirth ?? DateTime.Today.AddYears(-18);
            IsGoogleUser = !string.IsNullOrWhiteSpace(c.GoogleId);
            IsAppleUser  = !string.IsNullOrWhiteSpace(c.AppleId);   // ← ADD
            CurrentPassword = await SecureStorageHelper.GetAsync("Password") ?? string.Empty;

            // ── Resolve profile image ────────────────────────────
            if (!string.IsNullOrWhiteSpace(c.Snap))
            {
                if (c.Snap.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Google user — Snap is a CDN URL
                    ProfileImageSource = ImageSource.FromUri(new Uri(c.Snap));
                }
                else
                {
                    // Email user — Snap is Base64
                    try
                    {
                        var bytes = Convert.FromBase64String(c.Snap);
                        SnapBase64 = c.Snap;
                        ProfileImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                    }
                    catch
                    {
                        ProfileImageSource = null;
                    }
                }
            }
            else
            {
                ProfileImageSource = null;
            }

            OnPropertyChanged(nameof(HasProfilePicture));
            OnPropertyChanged(nameof(HasNoProfilePicture));
            OnPropertyChanged(nameof(IsNotGoogleUser));
            OnPropertyChanged(nameof(Initials));
            OnPropertyChanged(nameof(IsSocialUser));
            OnPropertyChanged(nameof(IsNotSocialUser));
            OnPropertyChanged(nameof(ShowAppleBadge));
        }

        // ── Computed properties ──────────────────────────────────
        public bool HasProfilePicture => ProfileImageSource != null;
        public bool HasNoProfilePicture => !HasProfilePicture;

        // ── Pick & crop profile image ────────────────────────────
        [RelayCommand]
        private async Task PickProfileImage()
        {
            try
            {
                string action = await App.Current.MainPage.DisplayActionSheet(
                    "Profile Photo", "Cancel", null,
                    "Take Photo", "Choose from Gallery");

                if (action == "Cancel" || action == null) return;

                FileResult? photo = null;

                if (action == "Take Photo")
                {
                    if (!MediaPicker.Default.IsCaptureSupported)
                    {
                        await Alert("Unavailable", "Camera is not supported on this device.");
                        return;
                    }
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }
                else if (action == "Choose from Gallery")
                {
                    photo = await MediaPicker.Default.PickPhotoAsync(
                        new MediaPickerOptions { Title = "Select Profile Photo" });
                }

                if (photo == null) return;

                // Read raw bytes
                using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var rawBytes = ms.ToArray();

                // Open crop page
                var cropPage = new ImageCropPage(rawBytes);

                cropPage.OnImageCropped += async (croppedBytes) =>
                {
                    // ── Resize + compress via SkiaSharp ─────────
                    var processedBytes = ResizeAndCompressImage(croppedBytes, maxDimension: 400, quality: 80);

                    Console.WriteLine($"📸 Cropped  : {croppedBytes.Length / 1024} KB");
                    Console.WriteLine($"📸 Processed: {processedBytes.Length / 1024} KB");

                    // ── Update UI on main thread ─────────────────
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SnapBase64 = Convert.ToBase64String(processedBytes);
                        ProfileImageSource = ImageSource.FromStream(() => new MemoryStream(processedBytes));
                        OnPropertyChanged(nameof(HasProfilePicture));
                        OnPropertyChanged(nameof(HasNoProfilePicture));
                    });

                    // ── Upload ───────────────────────────────────
                    IsBusy = true;
                    try
                    {
                        var result = await _apiService.UpdateProfileImageAsync(new Mdl_UpdateProfileImage
                        {
                            CustomerCode = App._Customer.CustomerCode,
                            ImageBase64 = SnapBase64
                        });

                        if (result?.IsSuccess == true)
                        {
                            App._Customer.Snap = SnapBase64;
                            await Alert("Success", "Profile photo updated.");
                        }
                        else
                        {
                            await Alert("Error", result?.Message ?? "Could not upload photo.");
                        }
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                };

                await Navigation.PushModalAsync(cropPage);
            }
            catch (Exception ex)
            {
                await Alert("Error", ex.Message);
            }
        }

        // ── SkiaSharp resize + compress ──────────────────────────
        private static byte[] ResizeAndCompressImage(byte[] imageBytes, int maxDimension = 400, int quality = 80)
        {
            using var inputStream = new SKMemoryStream(imageBytes);
            using var original = SKBitmap.Decode(inputStream);

            if (original == null) return imageBytes;

            // Re-encode at reduced quality even if already small
            if (original.Width <= maxDimension && original.Height <= maxDimension)
            {
                using var smallImage = SKImage.FromBitmap(original);
                using var smallData = smallImage.Encode(SKEncodedImageFormat.Jpeg, quality);
                return smallData.ToArray();
            }

            // Calculate new dimensions preserving aspect ratio
            int newWidth, newHeight;
            if (original.Width >= original.Height)
            {
                newWidth = maxDimension;
                newHeight = (int)((float)original.Height / original.Width * maxDimension);
            }
            else
            {
                newHeight = maxDimension;
                newWidth = (int)((float)original.Width / original.Height * maxDimension);
            }

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            using var skImage = SKImage.FromBitmap(resized);
            using var encodedData = skImage.Encode(SKEncodedImageFormat.Jpeg, quality);
            return encodedData.ToArray();
        }

        // ── Save profile (name, mobile, DOB) ────────────────────
        [RelayCommand]
        private async Task SaveProfile()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    await Alert("Validation", "Please enter your full name.");
                    return;
                }

                IsBusy = true;

                var result = await _apiService.UpdateProfileAsync(new Mdl_UpdateProfile
                {
                    CustomerCode = App._Customer.CustomerCode,
                    CustomerName = FullName,
                    MobileNo = MobileNo,
                    DateOfBirth = DateOfBirth
                });

                if (result?.IsSuccess == true)
                {
                    App._Customer.CustomerName = FullName;
                    App._Customer.MobileNo = MobileNo;
                    App._Customer.DateOfBirth = DateOfBirth;
                    await Alert("Success", "Profile updated successfully.");
                }
                else
                {
                    await Alert("Error", result?.Message ?? "Could not update profile.");
                }
            }
            catch (Exception ex)
            {
                await Alert("Error", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Update password ──────────────────────────────────────
        [RelayCommand]
        private async Task UpdatePassword()
        {
            PasswordError = string.Empty;
            OnPropertyChanged(nameof(HasPasswordError));

            // Validate
            if (!IsSocialUser && string.IsNullOrWhiteSpace(CurrentPassword))
            {
                PasswordError = "Please enter your current password.";
                OnPropertyChanged(nameof(HasPasswordError));
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                PasswordError = "New password must be at least 6 characters.";
                OnPropertyChanged(nameof(HasPasswordError));
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                PasswordError = "Passwords do not match.";
                OnPropertyChanged(nameof(HasPasswordError));
                return;
            }

            try
            {
                IsBusy = true;

                var result = await _apiService.UpdatePasswordAsync(new Mdl_UpdatePassword
                {
                    CustomerCode = App._Customer.CustomerCode,
                    CurrentPassword = CurrentPassword,
                    NewPassword = NewPassword,
                    IsGoogleUser = IsGoogleUser
                });

                if (result?.IsSuccess == true)
                {
                    // Clear fields
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                    // In UpdatePassword():
                    if (!IsGoogleUser)
                        await SecureStorageHelper.SetAsync("Password", NewPassword);

                    await Alert("Success", "Password updated successfully.");
                }
                else
                {
                    PasswordError = result?.Message ?? "Could not update password.";
                    OnPropertyChanged(nameof(HasPasswordError));
                }
            }
            catch (Exception ex)
            {
                PasswordError = ex.Message;
                OnPropertyChanged(nameof(HasPasswordError));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            try
            {
                bool confirm = await App.Current.MainPage.DisplayAlert(
                    "Delete Account",
                    "Are you sure you want to permanently delete your account? This action cannot be undone.",
                    "Delete",
                    "Cancel");

                if (!confirm)
                    return;

                IsBusy = true;

                var result = await _apiService.DeleteAccountAsync(App._Customer.CustomerCode);

                if (result?.IsSuccess == true)
                {
                    // Clear local data
                    App._Customer = null;

                    SecureStorage.RemoveAll();

                    await App.Current.MainPage.DisplayAlert(
                        "Success",
                        "Your account has been deleted successfully.",
                        "OK");

                    // Navigate to Login page
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
                else
                {
                    await Alert("Error", result?.Message ?? "Could not delete account.");
                }
            }
            catch (Exception ex)
            {
                await Alert("Error", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        // ── Back navigation ──────────────────────────────────────
        [RelayCommand]
        private async Task GoBack()
        {
            if (IsBusy)
                return;

            if (Navigation != null)
                await Navigation.PopAsync();
        }


        [RelayCommand]
        private void ToggleCurrentPassword()
        {
            IsPasswordHidden = !IsPasswordHidden;
            EyeIcon = IsPasswordHidden ? "eye.png" : "hidden.png";
        }

        [RelayCommand]
        private void ToggleNewPassword()
        {
            IsNewPasswordHidden = !IsNewPasswordHidden;
            NewEyeIcon = IsNewPasswordHidden ? "eye.png" : "hidden.png";
        }

        [RelayCommand]
        private void ToggleConfirmPassword()
        {
            IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
            ConfirmEyeIcon = IsConfirmPasswordHidden ? "eye.png" : "hidden.png";
        }

        private static Task Alert(string title, string msg)
            => App.Current.MainPage.DisplayAlert(title, msg, "OK");
    }
}