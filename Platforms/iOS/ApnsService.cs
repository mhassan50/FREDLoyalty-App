using Firebase.CloudMessaging;
using Foundation;
using UIKit;

namespace FREDLoyalty_App.Platforms.iOS;

/// <summary>
/// Handles APNs device token registration and passes it to FCM.
/// Registered as UIApplicationDelegate via MauiAppBuilder.
/// </summary>
public class ApnsService : NSObject, IUIApplicationDelegate
{
    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(
        UIApplication application, NSData deviceToken)
    {
        Messaging.SharedInstance.ApnsToken = deviceToken;
        Console.WriteLine("[FCM] APNs token registered successfully.");
    }

    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(
        UIApplication application, NSError error)
    {
        Console.WriteLine($"[FCM] APNs registration failed: {error.LocalizedDescription}");
    }
}