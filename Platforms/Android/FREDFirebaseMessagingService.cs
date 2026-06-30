using Android.App;
using Android.Content;
using Android.OS;
using Firebase.Messaging;
using FREDLoyalty_App.MVVM.Repos;

namespace FREDLoyalty_App.Platforms.Android
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FREDFirebaseMessagingService : FirebaseMessagingService
    {
        private const string RewardChannelId = "loyalty_reward_channel";
        private const string ReminderChannelId = "loyalty_reminder_channel_v2"; // ← updated

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);

            Console.WriteLine($"[FCM] New token: {token}");
            Preferences.Set("fcm_token", token);

            Task.Run(async () =>
            {
                try
                {
                    var customerCode = await SecureStorageHelper.GetAsync("CustomerCode");

                    if (string.IsNullOrWhiteSpace(customerCode)) return;

                    var fcmService = IPlatformApplication.Current?.Services
                        .GetService<IFcmService>();

                    if (fcmService != null)
                        await fcmService.SaveTokenToApiAsync(customerCode, token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FCM] Token refresh error: {ex.Message}");
                }
            });
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            string title = message.GetNotification()?.Title ?? "Polymath Coffee";
            string body = message.GetNotification()?.Body ?? string.Empty;
            string type = message.Data.ContainsKey("type")
                                ? message.Data["type"] : "reminder";
            string channelId = type == "reward"
                                ? RewardChannelId
                                : ReminderChannelId; // ← now points to v2

            ShowNotification(title, body, channelId);
        }

        private void ShowNotification(string title, string body, string channelId)
        {
            try
            {
                var manager = (NotificationManager)GetSystemService(NotificationService)!;
                var intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

                var pending = PendingIntent.GetActivity(this, 0, intent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                var builder = new Notification.Builder(this, channelId)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetSmallIcon(Resource.Drawable.coffeesvg)
                    .SetContentIntent(pending)
                    .SetAutoCancel(true)
                    .SetColor(unchecked((int)0xFFF4C542))
                    .SetStyle(new Notification.BigTextStyle()
                        .BigText(body)
                        .SetBigContentTitle(title));

                manager.Notify((int)DateTime.UtcNow.Ticks % 10000, builder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] ShowNotification error: {ex.Message}");
            }
        }
    }
}