namespace FREDLoyalty_App.MVVM.Repos;

public static class SecureStorageHelper
{
    public static async Task SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.SetAsync(key, value);
        }
        catch { }
        
        // Always save to Preferences too — works everywhere
        Preferences.Set(key, value);
    }

    public static async Task<string?> GetAsync(string key)
    {
        try
        {
            var val = await SecureStorage.GetAsync(key);
            if (!string.IsNullOrEmpty(val)) return val;
        }
        catch { }
        
        // Fallback to Preferences
        return Preferences.Get(key, string.Empty);
    }

    public static void Remove(string key)
    {
        try
        {
            SecureStorage.Remove(key);
        }
        catch
        {
            Preferences.Remove(key);
        }
    }
}