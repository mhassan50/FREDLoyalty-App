using FREDLoyalty_App.MVVM.Models;

namespace FREDLoyalty_App.MVVM.Repos
{
    public interface ILoyaltyNotifierService
    {
        /// <summary>
        /// Fetches all loyalty balances for a customer by email in one call.
        /// Used by both the HomeViewModel and the background notification service.
        /// </summary>
        Task<Mdl_LoyaltyCounterBalanceResult> GetLoyaltyBalanceByEmailAsync(string email);
    }
}
