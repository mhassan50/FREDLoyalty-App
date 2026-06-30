using Firebase.Core;
using Firebase.CloudMessaging;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;
using UserNotifications;

namespace FREDLoyalty_App.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

 public override bool FinishedLaunching(UIApplication application,
    NSDictionary launchOptions)
{
    // ── Initialize Firebase ──────────────────────────────
    try
    {
        Firebase.Core.App.Configure();
        Messaging.SharedInstance.Delegate = new FREDFirebaseMessagingDelegate();
        Messaging.SharedInstance.AutoInitEnabled = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Firebase] Init failed: {ex.Message}");
    }

    // ── Initialize Google Maps ───────────────────────────
    try
    {
        Google.Maps.MapServices.ProvideApiKey(
            "API Link");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Maps] Init failed: {ex.Message}");
    }

    // ── Request notification permission ──────────────────
    UNUserNotificationCenter.Current.Delegate =
        new FREDUserNotificationDelegate();

    var authOptions =
        UNAuthorizationOptions.Alert |
        UNAuthorizationOptions.Badge |
        UNAuthorizationOptions.Sound;

    UNUserNotificationCenter.Current.RequestAuthorization(
        authOptions, (granted, error) =>
        {
            Console.WriteLine($"[FCM] Permission granted: {granted}");
            MainThread.BeginInvokeOnMainThread(() =>
                UIApplication.SharedApplication
                    .RegisterForRemoteNotifications());
        });

    return base.FinishedLaunching(application, launchOptions);
}

    // ── Handle Google OAuth redirect ─────────────────────────
    public override bool OpenUrl(UIApplication app, NSUrl url,
        NSDictionary options)
    {
        if (Platform.OpenUrl(app, url, options))
            return true;

        return base.OpenUrl(app, url, options);
    }

    public override bool ContinueUserActivity(
        UIApplication application,
        NSUserActivity userActivity,
        UIApplicationRestorationHandler completionHandler)
    {
        if (Platform.ContinueUserActivity(
                application, userActivity, completionHandler))
            return true;

        return base.ContinueUserActivity(
            application, userActivity, completionHandler);
    }
}