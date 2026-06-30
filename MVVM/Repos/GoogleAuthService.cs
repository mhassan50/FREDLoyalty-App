using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class GoogleAuthService
    {
        // ═════════════════════════════════════════════════════════
        // PLATFORM CLIENT IDs
        // ═════════════════════════════════════════════════════════
#if ANDROID
private const string ClientId = "YOUR-ANDROID-CLIENT-ID.apps.googleusercontent.com";


        private const string RedirectUri =
            "com.googleusercontent.apps.YOUR-GOOGLE-CLIENT-ID:/oauth2redirect";

#elif IOS
private const string ClientId = "YOUR-IOS-CLIENT-ID.apps.googleusercontent.com";


        private const string RedirectUri =
            "com.googleusercontent.apps.YOUR-GOOGLE-CLIENT-ID:/oauth2redirect";

#else
        // Windows / MacCatalyst / Tizen — not supported
        private const string ClientId   = "";
        private const string RedirectUri = "";
#endif

        // ═════════════════════════════════════════════════════════
        // SIGN IN
        // ═════════════════════════════════════════════════════════
        public async Task<GoogleUserInfo?> SignInAsync()
        {
#if !ANDROID && !IOS
            throw new PlatformNotSupportedException(
                "Google Sign-In is only supported on Android and iOS.");
#else
            try
            {
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);
                var authUrl = BuildAuthUrl(codeChallenge);

#pragma warning disable CA1416
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new WebAuthenticatorOptions
                    {
                        Url = new Uri(authUrl),
                        CallbackUrl = new Uri(RedirectUri),
                        PrefersEphemeralWebBrowserSession = true
                    });
#pragma warning restore CA1416

                if (!authResult.Properties.TryGetValue("code", out string? authCode)
                    || string.IsNullOrWhiteSpace(authCode))
                    throw new Exception("Authorization code not returned from Google.");

                var tokens = await ExchangeCodeForTokensAsync(authCode, codeVerifier);
                var userInfo = await GetUserInfoAsync(tokens.AccessToken);

                // Persist tokens
                await SecureStorageHelper.SetAsync("google_access_token", tokens.AccessToken);
                await SecureStorageHelper.SetAsync("google_refresh_token", tokens.RefreshToken);
                Preferences.Set("google_token_expiry",
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokens.ExpiresIn);

                return userInfo;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[Google] Sign-in cancelled by user.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Google] SignIn error: {ex.Message}");
                throw;
            }
#endif
        }

        // ═════════════════════════════════════════════════════════
        // SIGN OUT
        // ═════════════════════════════════════════════════════════
        public async Task SignOutAsync()
        {
#if !ANDROID && !IOS
            return;
#else
            try
            {
                string? token = await SecureStorageHelper.GetAsync("google_access_token");
                if (!string.IsNullOrWhiteSpace(token))
                    await RevokeTokenAsync(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Google] SignOut error: {ex.Message}");
            }
            finally
            {
                SecureStorageHelper.Remove("google_access_token");
                SecureStorageHelper.Remove("google_refresh_token");
                Preferences.Remove("google_token_expiry");
            }
#endif
        }

        // ═════════════════════════════════════════════════════════
        // BUILD AUTH URL
        // ═════════════════════════════════════════════════════════
        private static string BuildAuthUrl(string codeChallenge)
        {
            var parameters = new Dictionary<string, string>
            {
                { "scope",                  "openid profile email" },
                { "access_type",            "offline"              },
                { "include_granted_scopes", "true"                 },
                { "response_type",          "code"                 },
                { "redirect_uri",           RedirectUri            },
                { "client_id",              ClientId               },
                { "code_challenge_method",  "S256"                 },
                { "code_challenge",         codeChallenge          },
                { "prompt",                 "select_account"       }
            };

            var queryString = string.Join("&",
                parameters.Select(p =>
                    $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
        }

        // ═════════════════════════════════════════════════════════
        // EXCHANGE CODE FOR TOKENS
        // ═════════════════════════════════════════════════════════
        private static async Task<TokenResponse> ExchangeCodeForTokensAsync(
            string authCode, string codeVerifier)
        {
            using var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type",    "authorization_code"),
                new KeyValuePair<string,string>("code",          authCode),
                new KeyValuePair<string,string>("redirect_uri",  RedirectUri),
                new KeyValuePair<string,string>("client_id",     ClientId),
                new KeyValuePair<string,string>("code_verifier", codeVerifier)
            });

            var response = await client.PostAsync(
                "https://oauth2.googleapis.com/token", content);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Token exchange failed: {body}");

            var json = JsonObject.Parse(body)
                ?? throw new Exception("Empty token response from Google.");

            return new TokenResponse
            {
                AccessToken = json["access_token"]!.ToString(),
                RefreshToken = json["refresh_token"]?.ToString() ?? string.Empty,
                ExpiresIn = int.Parse(json["expires_in"]!.ToString())
            };
        }

        // ═════════════════════════════════════════════════════════
        // GET USER INFO
        // ═════════════════════════════════════════════════════════
        private static async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
        {
            var credential = GoogleCredential.FromAccessToken(accessToken);

            var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "FREDLoyalty"
            });

            var userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

            if (string.IsNullOrWhiteSpace(userInfo.Id))
                throw new Exception("Google did not return a valid user ID.");

            return new GoogleUserInfo
            {
                GoogleId = userInfo.Id,
                Email = userInfo.Email ?? string.Empty,
                Name = userInfo.Name ?? string.Empty,
                Picture = userInfo.Picture ?? string.Empty
            };
        }

        // ═════════════════════════════════════════════════════════
        // REVOKE TOKEN
        // ═════════════════════════════════════════════════════════
        private static async Task RevokeTokenAsync(string accessToken)
        {
            try
            {
                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("token", accessToken)
                });
                await client.PostAsync(
                    "https://oauth2.googleapis.com/revoke", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Google] RevokeToken error: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════
        // PKCE HELPERS
        // ═════════════════════════════════════════════════════════
        private static string GenerateCodeVerifier()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string GenerateCodeChallenge(string verifier)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
            return Convert.ToBase64String(hash)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

    // ═════════════════════════════════════════════════════════
    // MODELS
    // ═════════════════════════════════════════════════════════
    public class GoogleUserInfo
    {
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }

    internal class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}