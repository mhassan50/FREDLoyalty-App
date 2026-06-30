using FREDLoyalty_App.MVVM.Views;

namespace FREDLoyalty_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();

        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("Login", typeof(LoginPage));
        //    Routing.RegisterRoute("Home", typeof(HomePage));
            Routing.RegisterRoute("Welcome", typeof(MainPage));
            Routing.RegisterRoute("Registration", typeof(RegistrationPage));
            Routing.RegisterRoute("AutoLogin", typeof(AutoLoginPage)); // 👈 ADD THIS
            Routing.RegisterRoute("History", typeof(HistoryPage));
            Routing.RegisterRoute("Menu", typeof(BranchSelectionMenuPage));         // ← add
            Routing.RegisterRoute("Location", typeof(LocationPage));     // ← add if missing
            Routing.RegisterRoute("Rating", typeof(RatingPage));
            Routing.RegisterRoute("BranchSelection", typeof(BranchSelectionPage));
            Routing.RegisterRoute("NotificationHistory", typeof(NotificationHistoryPage));
        }
    }
}
