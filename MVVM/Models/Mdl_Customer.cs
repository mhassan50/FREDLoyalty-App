namespace FREDLoyalty_App.MVVM.Models
{
    // ═════════════════════════════════════════════════════════════
    // CUSTOMER
    // ═════════════════════════════════════════════════════════════
    public class Mdl_Customer
    {
        public int CustomerCode { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Pwd { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? GoogleId { get; set; }
        public string? AppleId { get; set; }
        public string? Snap { get; set; }  // ← added
    }


    // ═════════════════════════════════════════════════════════════
    // NORMAL LOGIN REQUEST
    // ═════════════════════════════════════════════════════════════
    public class Mdl_CustomerLogin
    {
        public string Email { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;
    }

    // ═════════════════════════════════════════════════════════════
    // GOOGLE AUTH REQUEST
    // ═════════════════════════════════════════════════════════════
    public class Mdl_GoogleAuth
    {
        public string Email { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    // ═════════════════════════════════════════════════════════════
    // APPLE AUTH REQUEST
    // ═════════════════════════════════════════════════════════════
    public class Mdl_AppleAuth
    {
        public string Email { get; set; } = string.Empty;
        public string AppleId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // ═════════════════════════════════════════════════════════════
    // SHARED RESULT
    // Used by Login, Register, GoogleAuth
    // GoogleId returned only when NeedsVerification = true
    // so VerificationPage knows which Google user to complete
    // ═════════════════════════════════════════════════════════════
    public class Mdl_CustomerResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CustomerCode { get; set; }
        public string? Token { get; set; }
        public Mdl_Customer? Customer { get; set; }
        public bool NeedsVerification { get; set; } = false;
        public string? GoogleId { get; set; } // ← only set when NeedsVerification=true
        public List<Mdl_Branch> Branches { get; set; } = new();
    }

    // ═════════════════════════════════════════════════════════════
    // VERIFICATION REQUEST
    // Carries mobile + DOB + Google data from VerificationPage
    // CustomerCode = 0 means brand new Google user (no DB record yet)
    // ═════════════════════════════════════════════════════════════
    public class Mdl_CustomerVerification
    {
        public int CustomerCode { get; set; }           // 0 if new Google user
        public string MobileNo { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }           // needed when CustomerCode = 0
        public string? GoogleId { get; set; }           // needed when CustomerCode = 0
        public string? AppleId { get; set; }
        public string? CustomerName { get; set; }           // needed when CustomerCode = 0
    }

    // ═════════════════════════════════════════════════════════════
    // VERIFICATION RESULT
    // Returns token + customer so MAUI can go straight to Home
    // without needing another API call
    // ═════════════════════════════════════════════════════════════
    public class Mdl_CustomerVerificationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CustomerCode { get; set; }
        public string? Token { get; set; }
        public Mdl_Customer? Customer { get; set; }
    }
}
