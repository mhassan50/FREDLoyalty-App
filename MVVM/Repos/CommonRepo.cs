using FREDLoyalty_App.MVVM.Models;
using System.Net.Http.Json;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class CommonRepo : ICommRepo
    {
        public CommonRepo() { }

        // ─────────────────────────────────────────────
        // REGISTER  (no token needed)
        // ─────────────────────────────────────────────
        public async Task<Mdl_CustomerResult> RegisterCustomerAsync(Mdl_Customer model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/Register";

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(url, model);

                return await response.Content
                    .ReadFromJsonAsync<Mdl_CustomerResult>()
                    ?? Fail("Empty response.");
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // LOGIN  (no token needed — this generates one)
        // ─────────────────────────────────────────────
        public async Task<Mdl_CustomerResult> LoginCustomerAsync(Mdl_CustomerLogin model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/Login";

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(url, model);

                return await response.Content
                    .ReadFromJsonAsync<Mdl_CustomerResult>()
                    ?? Fail("Empty response.");
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // GOOGLE AUTH  (no token needed — may generate one)
        // ─────────────────────────────────────────────
        public async Task<Mdl_CustomerResult> GoogleAuthAsync(Mdl_GoogleAuth model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/GoogleAuth";

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(url, model);

                return await response.Content
                    .ReadFromJsonAsync<Mdl_CustomerResult>()
                    ?? Fail("Empty response.");
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }


        // ─────────────────────────────────────────────
        // APPLE AUTH  (no token needed — may generate one)
        // ─────────────────────────────────────────────
        public async Task<Mdl_CustomerResult> AppleAuthAsync(Mdl_AppleAuth model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/AppleAuth";

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(url, model);

                return await response.Content
                    .ReadFromJsonAsync<Mdl_CustomerResult>()
                    ?? Fail("Empty response.");
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // UPDATE VERIFICATION
        // CustomerCode = 0 → new Google user, no token yet
        // CustomerCode > 0 → existing user, use token
        // ─────────────────────────────────────────────
        public async Task<Mdl_CustomerVerificationResult> UpdateVerificationAsync(
            Mdl_CustomerVerification model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/UpdateVerification";

                HttpResponseMessage response;

                if (model.CustomerCode > 0)
                {
                    response = await TokenHelper.EnsureTokenAndRetryAsync(url,
                        async client => await client.PostAsJsonAsync(url, model));
                }
                else
                {
                    using var client = new HttpClient();
                    response = await client.PostAsJsonAsync(url, model);
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                    return new Mdl_CustomerVerificationResult
                    {
                        IsSuccess = false,
                        Message = "Empty response from server."
                    };

                return System.Text.Json.JsonSerializer.Deserialize<Mdl_CustomerVerificationResult>(
                    json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new Mdl_CustomerVerificationResult
                    {
                        IsSuccess = false,
                        Message = "Could not parse response."
                    };
            }
            catch (Exception ex)
            {
                return new Mdl_CustomerVerificationResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // ─────────────────────────────────────────────
        // UPDATE PROFILE  (token required)
        // ─────────────────────────────────────────────
        public async Task<Mdl_UpdateProfileResult> UpdateProfileAsync(Mdl_UpdateProfile model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/update-profile";

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url,
                    async client => await client.PostAsJsonAsync(url, model));

                return await response.Content
                    .ReadFromJsonAsync<Mdl_UpdateProfileResult>()
                    ?? new Mdl_UpdateProfileResult
                    {
                        IsSuccess = false,
                        Message = "Empty response."
                    };
            }
            catch (Exception ex)
            {
                return new Mdl_UpdateProfileResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // ─────────────────────────────────────────────
        // DELETE ACCOUNT (token required)
        // ─────────────────────────────────────────────
        public async Task<Mdl_UpdateProfileResult> DeleteAccountAsync(int customerCode)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/delete-profile?customerCode={customerCode}";

                var response = await TokenHelper.EnsureTokenAndRetryAsync(
                    url,
                    async client => await client.PostAsync(url, null));

                return await response.Content
                    .ReadFromJsonAsync<Mdl_UpdateProfileResult>()
                    ?? new Mdl_UpdateProfileResult  
                    {
                        IsSuccess = false,
                        Message = "Empty response."
                    };
            }
            catch (Exception ex)
            {
                return new Mdl_UpdateProfileResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // ─────────────────────────────────────────────
        // UPDATE PROFILE  (token required)
        // ─────────────────────────────────────────────
        public async Task<Mdl_UpdateProfileResult> UpdateProfileImageAsync(Mdl_UpdateProfileImage model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/update-profileimage";

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url,
                    async client => await client.PostAsJsonAsync(url, model));

                return await response.Content
                    .ReadFromJsonAsync<Mdl_UpdateProfileResult>()
                    ?? new Mdl_UpdateProfileResult
                    {
                        IsSuccess = false,
                        Message = "Empty response."
                    };
            }
            catch (Exception ex)
            {
                return new Mdl_UpdateProfileResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // ─────────────────────────────────────────────
        // UPDATE PASSWORD  (token required)
        // ─────────────────────────────────────────────
        public async Task<Mdl_UpdateProfileResult> UpdatePasswordAsync(Mdl_UpdatePassword model)
        {
            try
            {
                string baseUrl = await Mdl_SettingAPI.CheckApiPing();
                string url = $"{baseUrl}/api/Common/update-password";

                var response = await TokenHelper.EnsureTokenAndRetryAsync(url,
                    async client => await client.PostAsJsonAsync(url, model));

                return await response.Content
                    .ReadFromJsonAsync<Mdl_UpdateProfileResult>()
                    ?? new Mdl_UpdateProfileResult
                    {
                        IsSuccess = false,
                        Message = "Empty response."
                    };
            }
            catch (Exception ex)
            {
                return new Mdl_UpdateProfileResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // ─────────────────────────────────────────────
        // HELPER
        // ─────────────────────────────────────────────
        private static Mdl_CustomerResult Fail(string message)
            => new() { IsSuccess = false, Message = message };
    }
}