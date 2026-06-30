using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using FREDLoyalty_App.Platforms.Android;
using Plugin.Firebase.CloudMessaging;

namespace FREDLoyalty_App
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent);

            FirebaseCloudMessagingImplementation.ChannelId = "loyalty_reminder_channel_v2";

            // ── Create notification channels ──
            CreateNotificationChannels();

            // Notification permission (Android 13+)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                RequestPermissions(
                    new[] { global::Android.Manifest.Permission.PostNotifications },
                    requestCode: 1001);
            }
        }

        private void CreateNotificationChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

            var manager = (NotificationManager)GetSystemService(NotificationService)!;

            // Reward channel
            var rewardChannel = new NotificationChannel(
                "loyalty_reward_channel",
                "Loyalty Rewards",
                NotificationImportance.High)
            {
                Description = "Notifications for free coffee rewards"
            };
            rewardChannel.EnableLights(true);
            rewardChannel.EnableVibration(true);
            manager.CreateNotificationChannel(rewardChannel);

            // Reminder channel v2 — forces recreation with High importance
            var reminderChannel = new NotificationChannel(
                "loyalty_reminder_channel_v2",
                "Loyalty Reminders",
                NotificationImportance.High)
            {
                Description = "Notifications for loyalty reminders and milestones"
            };
            reminderChannel.EnableLights(true);
            reminderChannel.EnableVibration(true);
            manager.CreateNotificationChannel(reminderChannel);
        }
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }


        private static void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }
    }
}
