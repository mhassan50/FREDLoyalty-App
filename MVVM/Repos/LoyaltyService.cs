using FREDLoyalty_App.MVVM.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class LoyaltyService : ILoyaltyService
    {
        public async Task<Mdl_LoyaltyCounterBalanceResult> GetLoyaltyCounterBalances(Mdl_LoyaltyCounterBalanceCriteria criteria)
        {
            var returnResponse = new Mdl_LoyaltyCounterBalanceResult();
            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url = $"{mApiUrl}/api/Loyalty/counter-balance";
            try
            {
                Console.WriteLine($"API URL: {url}");
                Console.WriteLine($"Payload: {JsonSerializer.Serialize(criteria)}");

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                {
                    return await client.PostAsJsonAsync(url, criteria);
                });

                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rawContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw Response Content: {rawContent}");

                    returnResponse = await response.Content.ReadFromJsonAsync<Mdl_LoyaltyCounterBalanceResult>();

                    if (returnResponse == null)
                        throw new Exception("Deserialization failed. The response content was null.");

                    return returnResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    throw new Exception($"Error fetching Loyalty Counter Balance: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                await Shell.Current.DisplayAlert("Error", $"HTTP Request Error: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Exception: {ex.Message}", "OK");
            }
            return returnResponse;
        }

        public async Task<Mdl_LoyaltyCounterResult> GetLoyaltyCounters(Mdl_LoyaltyCounterCriteria criteria)
        {
            var returnResponse = new Mdl_LoyaltyCounterResult();
            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url = $"{mApiUrl}/api/Loyalty/counter";
            try
            {
                Console.WriteLine($"API URL: {url}");
                Console.WriteLine($"Payload: {JsonSerializer.Serialize(criteria)}");

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                {
                    return await client.PostAsJsonAsync(url, criteria);
                });

                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rawContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw Response Content: {rawContent}");

                    returnResponse = await response.Content.ReadFromJsonAsync<Mdl_LoyaltyCounterResult>();

                    if (returnResponse == null)
                        throw new Exception("Deserialization failed. The response content was null.");

                    return returnResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    throw new Exception($"Error fetching Loyalty Counter: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                await Shell.Current.DisplayAlert("Error", $"HTTP Request Error: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Exception: {ex.Message}", "OK");
            }
            return returnResponse;
        }

        public async Task<Mdl_LoyaltyResult> GetLoyalties(Mdl_LoyaltyCriteria criteria)
        {
            var returnResponse = new Mdl_LoyaltyResult();
            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url = $"{mApiUrl}/api/Loyalty/list";
            try
            {
                Console.WriteLine($"API URL: {url}");
                Console.WriteLine($"Payload: {JsonSerializer.Serialize(criteria)}");

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                {
                    return await client.PostAsJsonAsync(url, criteria);
                });

                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rawContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw Response Content: {rawContent}");

                    returnResponse = await response.Content.ReadFromJsonAsync<Mdl_LoyaltyResult>();

                    if (returnResponse == null)
                        throw new Exception("Deserialization failed. The response content was null.");

                    return returnResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    throw new Exception($"Error fetching Loyalties: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                await Shell.Current.DisplayAlert("Error", $"HTTP Request Error: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Exception: {ex.Message}", "OK");
            }
            return returnResponse;
        }

        public async Task DownloadAndExtractImagesAsync()
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Loyalty/DownloadMenus";

                using var client = new HttpClient();

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async c =>
                {
                    return await c.GetAsync(url);
                });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception(error);
                }

                var zipBytes = await response.Content.ReadAsByteArrayAsync();

                // 🔥 Save ZIP locally
                var zipPath = Path.Combine(FileSystem.AppDataDirectory, "Menus.zip");

                await File.WriteAllBytesAsync(zipPath, zipBytes);

                // 🔥 Extract folder
                var extractPath = Path.Combine(FileSystem.AppDataDirectory, "Menus");

                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(zipPath, extractPath);

                Console.WriteLine($"Images extracted to: {extractPath}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }


        public async Task<Mdl_NotificationHistoryResult> GetNotificationHistoryAsync(Mdl_NotificationHistoryCriteria criteria)
        {
            var returnResponse = new Mdl_NotificationHistoryResult();
            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url = $"{mApiUrl}/api/Loyalty/notification-history";
            try
            {
                Console.WriteLine($"API URL: {url}");
                Console.WriteLine($"Payload: {JsonSerializer.Serialize(criteria)}");
                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                {
                    return await client.PostAsJsonAsync(url, criteria);
                });
                Console.WriteLine($"Response Status Code: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var rawContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw Response Content: {rawContent}");
                    returnResponse = await response.Content.ReadFromJsonAsync<Mdl_NotificationHistoryResult>();
                    if (returnResponse == null)
                        throw new Exception("Deserialization failed. The response content was null.");
                    return returnResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    throw new Exception($"Error fetching Notification History: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                await Shell.Current.DisplayAlert("Error", $"HTTP Request Error: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Exception: {ex.Message}", "OK");
            }
            return returnResponse;
        }
    }
}
