using Firebase.CloudMessaging;
using Foundation;
using UIKit;
using UserNotifications;
using FREDLoyalty_App.MVVM.Repos;

namespace FREDLoyalty_App.Platforms.iOS
{
    public class FREDFirebaseMessagingDelegate : NSObject, IMessagingDelegate
    {
        // ═════════════════════════════════════════════════════════
        // NEW TOKEN — save and register to API
        // ═════════════════════════════════════════════════════════
        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(
            Messaging messaging, string fcmToken)
        {
            Console.WriteLine($"[FCM] New token: {fcmToken}");
            Preferences.Set("fcm_token", fcmToken);

            Task.Run(async () =>
            {
                try
                {
                    var customerCode = await SecureStorageHelper.GetAsync("CustomerCode");
                    if (string.IsNullOrWhiteSpace(customerCode)) return;

                    var fcmService = IPlatformApplication.Current?.Services
                        .GetService<IFcmService>();

                    if (fcmService != null)
                        await fcmService.SaveTokenToApiAsync(customerCode, fcmToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FCM] Token refresh error: {ex.Message}");
                }
            });
        }
    }

    public class FREDUserNotificationDelegate : UNUserNotificationCenterDelegate
    {
        // ═════════════════════════════════════════════════════════
        // FOREGROUND — show notification while app is open
        // ═════════════════════════════════════════════════════════
        public override void WillPresentNotification(
            UNUserNotificationCenter center,
            UNNotification notification,
            Action<UNNotificationPresentationOptions> completionHandler)
        {
            completionHandler(
                UNNotificationPresentationOptions.Banner |
                UNNotificationPresentationOptions.Sound |
                UNNotificationPresentationOptions.Badge);
        }

        // ═════════════════════════════════════════════════════════
        // TAP — handle notification tap
        // ═════════════════════════════════════════════════════════
        public override void DidReceiveNotificationResponse(
            UNUserNotificationCenter center,
            UNNotificationResponse response,
            Action completionHandler)
        {
            var userInfo = response.Notification.Request.Content.UserInfo;

            // Extract type for routing if needed
            string type = string.Empty;
            if (userInfo.ContainsKey(new NSString("type")))
                type = userInfo[new NSString("type")].ToString() ?? string.Empty;

            Console.WriteLine($"[FCM] Notification tapped. Type: {type}");

            completionHandler();
        }
    }
}