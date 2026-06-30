using FREDLoyalty_App.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Repos
{
    public interface ICommRepo
    {
        Task<Mdl_CustomerResult?> RegisterCustomerAsync(Mdl_Customer model);
        Task<Mdl_CustomerResult?> LoginCustomerAsync(Mdl_CustomerLogin model);
        Task<Mdl_CustomerResult> GoogleAuthAsync(Mdl_GoogleAuth model);
        Task<Mdl_CustomerVerificationResult> UpdateVerificationAsync(
                    Mdl_CustomerVerification model);
        Task<Mdl_UpdateProfileResult> UpdatePasswordAsync(Mdl_UpdatePassword model);
        Task<Mdl_UpdateProfileResult> UpdateProfileAsync(Mdl_UpdateProfile model);
        Task<Mdl_UpdateProfileResult> UpdateProfileImageAsync(Mdl_UpdateProfileImage model);
        Task<Mdl_CustomerResult?> AppleAuthAsync(Mdl_AppleAuth model);
        Task<Mdl_UpdateProfileResult> DeleteAccountAsync(int customerCode);
    }
}
