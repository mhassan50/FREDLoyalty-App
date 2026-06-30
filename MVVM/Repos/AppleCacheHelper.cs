using FREDLoyalty_App.MVVM.Models;

namespace FREDLoyalty_App.MVVM.Repos
{
    // Apple returns email + name ONLY on first authorization.
    // We cache them keyed by AppleId and replay on subsequent logins.
    public static class AppleCacheHelper
    {
        public static async Task<(string email, string name)> ResolveAsync(AppleUserInfo apple)
        {
            string email = apple.Email;
            string name = apple.Name;

            // First login — Apple gave us the data, persist it
            if (!string.IsNullOrWhiteSpace(email))
                await SecureStorageHelper.SetAsync($"apple_email_{apple.AppleId}", email);
            if (!string.IsNullOrWhiteSpace(name))
                await SecureStorageHelper.SetAsync($"apple_name_{apple.AppleId}", name);

            // Subsequent login — Apple gave only the ID, pull from cache
            if (string.IsNullOrWhiteSpace(email))
                email = await SecureStorageHelper.GetAsync($"apple_email_{apple.AppleId}") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                name = await SecureStorageHelper.GetAsync($"apple_name_{apple.AppleId}") ?? string.Empty;

            return (email, name);
        }
    }
}