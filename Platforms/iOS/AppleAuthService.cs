#if IOS
using AuthenticationServices;
using Foundation;
using FREDLoyalty_App.MVVM.Models;
using UIKit;

namespace FREDLoyalty_App.MVVM.Repos
{
    public class AppleAuthService : NSObject,
        IASAuthorizationControllerDelegate,
        IASAuthorizationControllerPresentationContextProviding
    {
        private TaskCompletionSource<AppleUserInfo?>? _tcs;

        public Task<AppleUserInfo?> SignInAsync()
        {
            _tcs = new TaskCompletionSource<AppleUserInfo?>();

            var provider = new ASAuthorizationAppleIdProvider();
            var request = provider.CreateRequest();
            request.RequestedScopes = new[]
            {
                ASAuthorizationScope.FullName,
                ASAuthorizationScope.Email
            };

            var controller = new ASAuthorizationController(new[] { (ASAuthorizationRequest)request })
            {
                Delegate = this,
                PresentationContextProvider = this
            };

            controller.PerformRequests();
            return _tcs.Task;
        }

        // ── Success ──────────────────────────────────────────────
        [Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(ASAuthorizationController controller,
            ASAuthorization authorization)
        {
            if (authorization.GetCredential<ASAuthorizationAppleIdCredential>()
                is ASAuthorizationAppleIdCredential credential)
            {
                // user ID is stable and always returned
                string appleId = credential.User;

                // email + name only returned on the FIRST authorization
                string? email = credential.Email;
                string? fullName = null;
                if (credential.FullName != null)
                {
                    var given = credential.FullName.GivenName ?? string.Empty;
                    var family = credential.FullName.FamilyName ?? string.Empty;
                    fullName = $"{given} {family}".Trim();
                }

                string? identityToken = credential.IdentityToken != null
                    ? new NSString(credential.IdentityToken, NSStringEncoding.UTF8).ToString()
                    : null;

                _tcs?.TrySetResult(new AppleUserInfo
                {
                    AppleId = appleId,
                    Email = email ?? string.Empty,
                    Name = fullName ?? string.Empty,
                    IdentityToken = identityToken ?? string.Empty
                });
            }
            else
            {
                _tcs?.TrySetResult(null);
            }
        }

        // ── Error / cancel ───────────────────────────────────────
        [Export("authorizationController:didCompleteWithError:")]
        public void DidComplete(ASAuthorizationController controller, NSError error)
        {
            // 1001 = user cancelled
            if (error.Code == 1001)
                _tcs?.TrySetResult(null);
            else
                _tcs?.TrySetException(new Exception($"Apple Sign-In failed: {error.LocalizedDescription}"));
        }

        // ── Presentation anchor ──────────────────────────────────
        public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
        {
            return UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .SelectMany(s => s.Windows)
                .FirstOrDefault(w => w.IsKeyWindow)
                ?? UIApplication.SharedApplication.Windows[0];
        }
    }
}
#endif