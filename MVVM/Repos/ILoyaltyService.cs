using FREDLoyalty_App.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.Repos
{
    public interface ILoyaltyService
    {
        Task<Mdl_LoyaltyCounterBalanceResult> GetLoyaltyCounterBalances(Mdl_LoyaltyCounterBalanceCriteria criteria);
        Task<Mdl_LoyaltyCounterResult> GetLoyaltyCounters(Mdl_LoyaltyCounterCriteria criteria);
        Task<Mdl_LoyaltyResult> GetLoyalties(Mdl_LoyaltyCriteria criteria);
        Task DownloadAndExtractImagesAsync();
        Task<Mdl_NotificationHistoryResult> GetNotificationHistoryAsync(Mdl_NotificationHistoryCriteria criteria);

    }
}
