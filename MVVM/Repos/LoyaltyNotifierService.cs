using FREDLoyalty_App.MVVM.Models;
using System.Net.Http.Json;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class LoyaltyNotifierService : ILoyaltyNotifierService
    {
        private readonly HttpClient _httpClient;

        public LoyaltyNotifierService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Mdl_LoyaltyCounterBalanceResult> GetLoyaltyBalanceByEmailAsync(string email)
        {
            try
            {
                string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
                var response = await _httpClient.GetAsync(
                    $"{mApiUrl}/loyalty/counterNotifier?email={Uri.EscapeDataString(email)}");

                if (!response.IsSuccessStatusCode)
                {
                    return new Mdl_LoyaltyCounterBalanceResult
                    {
                        IsSuccess = false,
                        Message = $"API error: {response.StatusCode}",
                        Data = null
                    };
                }

                var result = await response.Content
                    .ReadFromJsonAsync<Mdl_LoyaltyCounterBalanceResult>();

                return result ?? new Mdl_LoyaltyCounterBalanceResult
                {
                    IsSuccess = false,
                    Message = "Empty response from server",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoyaltyNotifierService] Error: {ex.Message}");
                return new Mdl_LoyaltyCounterBalanceResult
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
