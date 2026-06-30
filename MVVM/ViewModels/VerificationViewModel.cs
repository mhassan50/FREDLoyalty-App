using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class VerificationViewModel : ObservableObject
    {
        private readonly ICommRepo _apiService;

        public VerificationViewModel(ICommRepo apiService)
        {
            _apiService = apiService;
        }

        [ObservableProperty] private string   phoneNumber  = string.Empty;
        [ObservableProperty] private string dobDisplay = "Select date of birth (optional)";
        [ObservableProperty] private bool     isBusy       = false;
        [ObservableProperty] private string   errorMessage = string.Empty;
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
        private async Task SaveDetailsAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 10)
            {
                ErrorMessage = "Please enter a valid mobile number.";
                return;
            }

            try
            {
                IsBusy = true;

                var customer = App._Customer;
                if (customer == null)
                {
                    ErrorMessage = "Session expired. Please login again.";
                    return;
                }

                var result = await _apiService.UpdateVerificationAsync(
                    new Mdl_CustomerVerification
                    {
                        CustomerCode = customer.CustomerCode,
                        MobileNo     = PhoneNumber,
                        DateOfBirth  = DateOfBirth,
                        Email        = customer.Email,
                        GoogleId     = customer.GoogleId,
                        AppleId      = customer.AppleId,    // null for Google users  ← ADD
                        CustomerName = customer.CustomerName
                    });

                if (result?.IsSuccess == true)
                {
                    if (result.Customer != null)
                    {
                        App._Customer = result.Customer;
                        if (App._Customer.CustomerCode == 0)
                            App._Customer.CustomerCode = result.CustomerCode ?? 0;

                        await SecureStorageHelper.SetAsync("CustomerCode",
                            App._Customer.CustomerCode.ToString());
                    }

                    if (!string.IsNullOrEmpty(result.Token))
                        await SecureStorageHelper.SetAsync("AuthToken", result.Token);

                    await App.SetAppShellAfterLogin();
                }
                else
                {
                    ErrorMessage = result?.Message ?? "Could not save. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred. Please try again.";
                Console.WriteLine($"[VerificationVM] {ex.Message}");
            }
            finally { IsBusy = false; }
        }
    }
}