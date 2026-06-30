using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Views
{
    public static class AppStartupHelper
    {
        public static async Task<string> GetStartRouteAsync()
        {
            var isFirstTime = await SecureStorage.GetAsync("IsFirstTimeRegistration");
            var isRememberMe = await SecureStorage.GetAsync("IsRememberMe");
            var token = await SecureStorage.GetAsync("AuthToken");
            var email = await SecureStorage.GetAsync("Email");
            var password = await SecureStorage.GetAsync("Password");
            var isGoogle = await SecureStorage.GetAsync("IsGoogle");

            // ✅ FIRST TIME → Welcome Page
            if (string.IsNullOrEmpty(isFirstTime))
            {
                return "Welcome";
            }

            // ✅ REMEMBER ME → Try Auto Login
            if (isRememberMe == "true" &&
                !string.IsNullOrEmpty(email))
            {
                return "AutoLogin";
            }

            // ✅ Default → Login Page
            return "Login";
        }
    }
}
