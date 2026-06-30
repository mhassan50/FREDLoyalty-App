using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.Views;

namespace FREDLoyalty_App
{
    public partial class App : Application
    {
        public static Mdl_Customer _Customer { get; set; } = new Mdl_Customer();

        public static List<Mdl_Branch> Branches { get; set; } = new();
        public static string _ApiId { get; set; } = string.Empty;

        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Light;

            // Start with AutoLogin page directly
            MainPage = new AutoLoginPage();
        }

        // Called after successful login or register
        public static async Task SetAppShellAfterLogin()
        {
            Application.Current.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//Home");
            Application.Current.Dispatcher.Dispatch(async () =>
            {
                await RegisterFcmTokenAsync();
            });
        }

        public static async Task RegisterFcmTokenAsync()
        {
            try
            {
                var fcmService = IPlatformApplication.Current?.Services
                    .GetService<IFcmService>();
                if (fcmService == null) return;

                string customerCode = _Customer?.CustomerCode.ToString();
                if (string.IsNullOrWhiteSpace(customerCode)) return;

                var token = await fcmService.GetTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                    await fcmService.SaveTokenToApiAsync(customerCode, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] RegisterFcmToken error: {ex.Message}");
            }
        }
    }
}