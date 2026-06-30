using FREDLoyalty_App.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class FeedbackService : IFeedback
    {
        public async Task<Mdl_CustomerFeedbackResult> InsertFeedbackAsync(Mdl_CustomerFeedback model)
        {
            var returnResponse = new Mdl_CustomerFeedbackResult();

            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url = $"{mApiUrl}/api/Feedback/Insert-rating";

            try
            {
                Console.WriteLine($"API URL: {url}");
                Console.WriteLine($"Payload: {JsonSerializer.Serialize(model)}");

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                {
                    return await client.PostAsJsonAsync(url, model);
                });

                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rawContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw Response Content: {rawContent}");

                    returnResponse = await response.Content.ReadFromJsonAsync<Mdl_CustomerFeedbackResult>();

                    if (returnResponse == null)
                        throw new Exception("Deserialization failed. Response was null.");

                    return returnResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");

                    throw new Exception($"Error inserting feedback: {errorContent}");
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

        public async Task<Mdl_CustomerSuggestionResult> InsertSuggestionAsync(Mdl_CustomerSuggestion model)
        {
            var returnResponse = new Mdl_CustomerSuggestionResult();

            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url = $"{mApiUrl}/api/Feedback/Insert-Suggestions";

            try
            {
                Console.WriteLine($"API URL: {url}");
                Console.WriteLine($"Payload: {JsonSerializer.Serialize(model)}");

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                {
                    return await client.PostAsJsonAsync(url, model);
                });

                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rawContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw Response Content: {rawContent}");

                    returnResponse = await response.Content.ReadFromJsonAsync<Mdl_CustomerSuggestionResult>();

                    if (returnResponse == null)
                        throw new Exception("Deserialization failed. Response was null.");

                    return returnResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");

                    throw new Exception($"Error inserting suggestion: {errorContent}");
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
