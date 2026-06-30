using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Models
{
    public static class Mdl_SettingAPI
    {




        public static class ApiEndpoints
        {
            public static readonly List<string> ApiList = new()
            {
                "https://api.your-company-domain.example.com",   // Primary — replace with your own backend
                "https://api-externalnetwork.your-company-domain.example.com"   // Backup
            };
        }


        public static async Task<string> CheckApiPing()
        {
            // If already working, use it first
            if (!string.IsNullOrEmpty(App._ApiId))
            {
                if (await IsApiReachable(App._ApiId))
                    return App._ApiId;
            }

            // Try all APIs with retries
            foreach (var api in ApiEndpoints.ApiList)
            {
                if (await IsApiReachableWithRetry(api))
                {
                    App._ApiId = api;
                    Console.WriteLine($"[API] Switched to: {api}");
                    return api;
                }
            }

            throw new Exception("No API endpoints are reachable. Please check your internet connection.");
        }

        private static async Task<bool> IsApiReachableWithRetry(string baseUrl, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"[API] Ping attempt {attempt}/{maxRetries}: {baseUrl}");

                    bool reachable = await IsApiReachable(baseUrl);

                    if (reachable)
                    {
                        Console.WriteLine($"[API] Reachable on attempt {attempt}");
                        return true;
                    }

                    // Wait before retry — longer each time
                    if (attempt < maxRetries)
                    {
                        int delayMs = attempt * 1500; // 1.5s, 3s
                        Console.WriteLine($"[API] Retrying in {delayMs}ms...");
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API] Attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxRetries)
                        await Task.Delay(attempt * 1500);
                }
            }

            return false;
        }

        private static async Task<bool> IsApiReachable(string baseUrl)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(15) // increased from 10
                };

                var response = await client.GetAsync($"{baseUrl}/api/Common/ping");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}


