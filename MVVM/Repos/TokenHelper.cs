using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Views;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FREDLoyalty_App.MVVM.Repos
{
    public static class TokenHelper
    {
        public static async Task<string?> RefreshTokenAsync()
        {
            try
            {
                var email       = await SecureStorageHelper.GetAsync("Email");
                var isGoogleStr = await SecureStorageHelper.GetAsync("IsGoogle");
                var isAppleStr  = await SecureStorageHelper.GetAsync("IsApple");   // ← ADD


                if (string.IsNullOrWhiteSpace(email))
                {
                    await AppLogger.LogAsync("❌ Refresh failed: no email stored.");
                    return null;
                }

                bool isGoogle = isGoogleStr == "true";
                bool isApple  = isAppleStr == "true";  // ← ADD
                await AppLogger.LogAsync($"🔄 Refreshing token | Google: {isGoogle} | Apple: {isApple}");

                // ── APPLE branch ─────────────────────────────────────
                if (isApple)
                {
                    var appleId = await SecureStorageHelper.GetAsync("AppleId");

                    if (string.IsNullOrWhiteSpace(appleId))
                    {
                        await AppLogger.LogAsync("❌ Apple refresh failed: no AppleId stored.");
                        return null;
                    }

                    string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                    string url     = $"{baseUrl}/api/Common/AppleAuth";

                    using var client = new HttpClient();
                    var response = await client.PostAsJsonAsync(url, new Mdl_AppleAuth
                    {
                        Email   = email,
                        AppleId = appleId,
                        Name    = string.Empty
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        await AppLogger.LogAsync($"❌ Apple refresh failed: {response.StatusCode}");
                        return null;
                    }

                    var result = await response.Content.ReadFromJsonAsync<Mdl_CustomerResult>();

                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        await SecureStorageHelper.SetAsync("AuthToken", result.Token);
                        await AppLogger.LogAsync("✅ Apple token refreshed.");
                        return result.Token;
                    }

                    return null;
                }
                if (isGoogle)
                {
                    var googleId = await SecureStorageHelper.GetAsync("GoogleId");

                    if (string.IsNullOrWhiteSpace(googleId))
                    {
                        await AppLogger.LogAsync("❌ Google refresh failed: no GoogleId stored.");
                        return null;
                    }

                    string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                    string url     = $"{baseUrl}/api/Common/GoogleAuth";

                    using var client = new HttpClient();
                    var response = await client.PostAsJsonAsync(url, new Mdl_GoogleAuth
                    {
                        Email    = email,
                        GoogleId = googleId,
                        Name     = string.Empty
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        await AppLogger.LogAsync($"❌ Google refresh failed: {response.StatusCode}");
                        return null;
                    }

                    var result = await response.Content
                        .ReadFromJsonAsync<Mdl_CustomerResult>();

                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        await SecureStorageHelper.SetAsync("AuthToken", result.Token);
                        await AppLogger.LogAsync("✅ Google token refreshed.");
                        return result.Token;
                    }

                    return null;
                }
                else
                {
                    var password = await SecureStorageHelper.GetAsync("Password");

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        await AppLogger.LogAsync("❌ Normal refresh failed: no password stored.");
                        return null;
                    }

                    string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                    string url     = $"{baseUrl}/api/Common/Login";

                    using var client = new HttpClient();
                    var response = await client.PostAsJsonAsync(url, new Mdl_CustomerLogin
                    {
                        Email = email,
                        Pwd   = password
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        await AppLogger.LogAsync($"❌ Normal refresh failed: {response.StatusCode}");
                        return null;
                    }

                    var result = await response.Content
                        .ReadFromJsonAsync<Mdl_CustomerResult>();

                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        await SecureStorageHelper.SetAsync("AuthToken", result.Token);
                        await AppLogger.LogAsync("✅ Normal token refreshed.");
                        return result.Token;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                await AppLogger.LogAsync($"🔥 Refresh exception: {ex.Message}");
                return null;
            }
        }

        public static async Task<T?> SendAuthorizedAsync<T>(string url, HttpContent content)
        {
            var response = await EnsureTokenAndRetryAsync(
                url, async client => await client.PostAsync(url, content));

            if (!response.IsSuccessStatusCode)
            {
                await AppLogger.LogAsync($"❌ API Error: {response.StatusCode}");
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<HttpResponseMessage> EnsureTokenAndRetryAsync(
            string url,
            Func<HttpClient, Task<HttpResponseMessage>> apiRequest)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var token = await SecureStorageHelper.GetAsync("AuthToken");

                using var client = new HttpClient();

                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                await AppLogger.LogAsync($"➡️ Request: {url}");

                var response = await apiRequest(client);
                stopwatch.Stop();

                await AppLogger.LogAsync(
                    $"⬅️ {url} → {(int)response.StatusCode} ({response.StatusCode}) | {stopwatch.ElapsedMilliseconds}ms");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await AppLogger.LogAsync("⚠️ Token expired. Refreshing...");

                    var newToken = await RefreshTokenAsync();

                    if (string.IsNullOrEmpty(newToken))
                    {
                        await AppLogger.LogAsync("❌ Refresh failed — redirecting to login.");

                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            SecureStorageHelper.Remove("AuthToken");
                            SecureStorageHelper.Remove("Password");
                            SecureStorageHelper.Remove("GoogleId");
                            SecureStorageHelper.Remove("AppleId");
                            await SecureStorageHelper.SetAsync("IsRememberMe", "false");

                            Preferences.Remove("remember_me");
                            Preferences.Remove("saved_email");

                            App._Customer = null;
                            Application.Current.MainPage = new NavigationPage(new LoginPage());
                        });

                        return response;
                    }

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", newToken);

                    stopwatch.Restart();
                    response = await apiRequest(client);
                    stopwatch.Stop();

                    await AppLogger.LogAsync(
                        $"🔁 Retry → {(int)response.StatusCode} | {stopwatch.ElapsedMilliseconds}ms");
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await AppLogger.LogAsync($"🔥 API Exception: {ex.Message}");
                throw;
            }
        }

        public static class AppLogger
        {
            private static readonly string FilePath =
                Path.Combine(FileSystem.AppDataDirectory, "app_logs.txt");

            public static async Task LogAsync(string message)
            {
                try
                {
                    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}\n";
                    await File.AppendAllTextAsync(FilePath, line);
                    Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LOG ERROR: {ex.Message}");
                }
            }
        }
    }
}