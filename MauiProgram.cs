using CommunityToolkit.Maui;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;
using Maui.GoogleMaps.Hosting;
using Microsoft.Maui.LifecycleEvents;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
using Plugin.Firebase.CloudMessaging;
#endif

namespace FREDLoyalty_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()
#if ANDROID
                .UseGoogleMaps()
#elif IOS
                .UseGoogleMaps("AppConfig.GoogleMapsApiKey")
#endif
                .RegisterFirebaseServices()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("IBMPlexMono-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("IBMPlexMono-SemiBold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddHttpClient<IFcmService, FcmService>(client =>
                client.Timeout = TimeSpan.FromSeconds(15));
            builder.Services.AddSingleton<ICommRepo, CommonRepo>();
            builder.Services.AddSingleton<ILoyaltyService, LoyaltyService>();
            builder.Services.AddScoped<ILoyaltyNotifierService, LoyaltyNotifierService>();
            builder.Services.AddSingleton<IMenuService, MenuService>();
            builder.Services.AddSingleton<HomeViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
#if ANDROID
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                {
                    CrossFirebase.Initialize(activity, () => activity);
                }));
            });
#endif
            return builder;
        }
    }
}