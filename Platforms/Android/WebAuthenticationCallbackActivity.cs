using Android.App;
using Android.Content;
using Android.Content.PM;

namespace FREDLoyalty_App.Platforms.Android
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.googleusercontent.apps.00000000000000000000uygsw8ifuebw")]
    public class WebAuthenticationCallbackActivity
        : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}
