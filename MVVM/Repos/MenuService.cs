using FREDLoyalty_App.MVVM.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class MenuService : IMenuService
    {
        public async Task<Mdl_MenuResult> GetMenuItemsAsync(Mdl_MenuCriteria criteria)
        {
            var returnResponse = new Mdl_MenuResult();
            string mApiUrl = await Mdl_SettingAPI.CheckApiPing();
            string url     = $"{mApiUrl}/api/Menu/itemstest";

            try
            {
                Console.WriteLine($"[Menu] API URL: {url}");
                Console.WriteLine($"[Menu] Payload: {JsonSerializer.Serialize(criteria)}");

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url, async client =>
                    await client.PostAsJsonAsync(url, criteria));

                Console.WriteLine($"[Menu] Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    returnResponse = await response.Content
                        .ReadFromJsonAsync<Mdl_MenuResult>();

                    if (returnResponse == null)
                        throw new Exception("Deserialization failed.");

                    return returnResponse;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Menu] Error: {error}");
                    throw new Exception($"Error fetching menu: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Menu] Exception: {ex.Message}");
            }

            return returnResponse;
        }
    }
}