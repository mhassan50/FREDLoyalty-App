

namespace FREDLoyalty_App.MVVM.Models
{
    // ── Add to API Models ──────────────────────────────────
    public class Mdl_UpdateProfile
    {
        public int CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class Mdl_UpdatePassword
    {
        public int CustomerCode { get; set; }
        public string? CurrentPassword { get; set; } // null for Google users
        public string NewPassword { get; set; }
        public bool IsGoogleUser { get; set; }
    }

    public class Mdl_UpdateProfileImage
    {
        public int CustomerCode { get; set; }
        public string ImageBase64 { get; set; } = string.Empty;
    }


    public class Mdl_UpdateProfileResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
