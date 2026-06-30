using FREDLoyalty_App.MVVM.Models;
using Plugin.Firebase.CloudMessaging;
using System.Net.Http.Json;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class FcmService : IFcmService
    {
        private readonly HttpClient _http;

        public FcmService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                Console.WriteLine($"[FCM] Token: {token}");
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] GetToken error: {ex.Message}");
                return null;
            }
        }

        public async Task SaveTokenToApiAsync(string customerCode, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return;

                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Notification/save-token";

                var response = await _http.PostAsJsonAsync(url, new Mdl_FcmToken
                {
                    CustomerCode = customerCode,
                    FcmToken = token,
                    Platform = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS" : "Android",
                    UpdatedAt = DateTime.UtcNow
                });

                var result = await response.Content
                    .ReadFromJsonAsync<Mdl_FcmTokenResult>();

                Console.WriteLine($"[FCM] Token saved: {result?.IsSuccess} — {result?.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] SaveToken error: {ex.Message}");
            }
        }
    }
}

