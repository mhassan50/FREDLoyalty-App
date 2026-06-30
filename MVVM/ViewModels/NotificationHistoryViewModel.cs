using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using System.Collections.ObjectModel;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class NotificationHistoryViewModel : ObservableObject
    {
        private readonly ILoyaltyService _loyaltyService;

        [ObservableProperty] private ObservableCollection<NotificationHistoryItemViewModel> notificationItems = new();
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isEmpty;
        [ObservableProperty] private bool isNotEmpty;
        [ObservableProperty] private int totalCount;

        public NotificationHistoryViewModel(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            NotificationItems.Clear();
            try
            {
                var customerCode = App._Customer?.CustomerCode;
                Console.WriteLine($"[NOTIF] CustomerCode: {customerCode}");

                if (customerCode == 0 || customerCode == null)
                {
                    Console.WriteLine("[NOTIF] CustomerCode is null or 0 — aborting.");
                    return;
                }

                var cusstring = customerCode.ToString();
                var criteria = new Mdl_NotificationHistoryCriteria { CustomerCode = cusstring };
                Console.WriteLine($"[NOTIF] Calling API with CustomerCode: {cusstring}");

                var result = await _loyaltyService.GetNotificationHistoryAsync(criteria);
                Console.WriteLine($"[NOTIF] API result — IsSuccess: {result?.IsSuccess}, Message: {result?.Message}, DataCount: {result?.Data?.Count}");

                if (result?.IsSuccess == true && result.Data?.Count > 0)
                {
                    foreach (var item in result.Data)
                    {
                        Console.WriteLine($"[NOTIF] Item — LNo:{item.LNo} | Type:{item.NotifType} | Title:{item.Title} | SentAt:{item.SentAt}");
                        NotificationItems.Add(NotificationHistoryItemViewModel.From(item));
                    }
                }
                else
                {
                    Console.WriteLine("[NOTIF] No data returned or IsSuccess=false.");
                }

                TotalCount = NotificationItems.Count;
                IsEmpty = NotificationItems.Count == 0;
                IsNotEmpty = !IsEmpty;
                Console.WriteLine($"[NOTIF] Final — TotalCount:{TotalCount} | IsEmpty:{IsEmpty} | IsNotEmpty:{IsNotEmpty}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTIF] Exception: {ex.Message}");
                Console.WriteLine($"[NOTIF] StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                Console.WriteLine($"[NOTIF] LoadAsync complete. IsLoading set to false.");
            }
        }
    }
}