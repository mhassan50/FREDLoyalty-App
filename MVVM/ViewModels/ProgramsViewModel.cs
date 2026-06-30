using CommunityToolkit.Mvvm.ComponentModel;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class ProgramsViewModel : ObservableObject
    {
        private readonly ILoyaltyService _loyaltyService;

        [ObservableProperty] private ObservableCollection<ProgramItem> programs = new();
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private bool isEmpty = false;
        [ObservableProperty] private bool hasPrograms = false;

        public ProgramsViewModel(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        public async Task LoadProgramsAsync()
        {
            try
            {
                IsLoading = true;
                IsEmpty = false;
                HasPrograms = false;

                var result = await _loyaltyService.GetLoyalties(new Mdl_LoyaltyCriteria
                {
                    BranchCode = 7,
                    IsActive = true
                });

                if (!result.IsSuccess || result.Data == null || !result.Data.Any())
                {
                    IsEmpty = true;
                    HasPrograms = false;
                    return;
                }

                var items = new ObservableCollection<ProgramItem>();

                foreach (var loyalty in result.Data)
                {
                    // ── Deal label ──
                    string dealLabel = $"Buy {loyalty.PurchasedQty:0} Get {loyalty.FreeQty:0} Free";

                    // ── Date strings ──
                    string startStr = loyalty.LoyaltyDtStart.HasValue
                        ? loyalty.LoyaltyDtStart.Value.ToString("dd MMM yyyy")
                        : "—";

                    string endStr = loyalty.LoyaltyDtEnd.HasValue
                        ? loyalty.LoyaltyDtEnd.Value.ToString("dd MMM yyyy")
                        : "Ongoing";

                    // ── End date color ──
                    Color endColor = Color.FromArgb("#EAEAEA");
                    if (loyalty.LoyaltyDtEnd.HasValue)
                    {
                        int daysLeft = (loyalty.LoyaltyDtEnd.Value.Date - DateTime.Today).Days;

                        if (daysLeft < 0)
                            endColor = Color.FromArgb("#FF5252");      // red — already expired
                        else if (daysLeft <= 7)
                            endColor = Color.FromArgb("#FF5252");      // red — ending very soon
                        else if (daysLeft <= 30)
                            endColor = Color.FromArgb("#F4C542");      // gold — ending this month
                    }

                    // ── Status badge ──
                    string statusLabel;
                    Color statusBg;
                    Color statusText;

                    bool isExpired = loyalty.LoyaltyDtEnd.HasValue &&
                                     loyalty.LoyaltyDtEnd.Value.Date < DateTime.Today;

                    int daysRemaining = loyalty.LoyaltyDtEnd.HasValue
                        ? (loyalty.LoyaltyDtEnd.Value.Date - DateTime.Today).Days
                        : int.MaxValue;

                    if (isExpired || !loyalty.IsAvailable)
                    {
                        statusLabel = "UNAVAILABLE";
                        statusBg = Color.FromArgb("#616161");
                        statusText = Color.FromArgb("#FFFFFF");
                    }
                    else if (daysRemaining <= 7)
                    {
                        statusLabel = "ENDING SOON";
                        statusBg = Color.FromArgb("#FF5252");
                        statusText = Color.FromArgb("#FFFFFF");
                    }
                    else
                    {
                        statusLabel = "ACTIVE";
                        statusBg = Color.FromArgb("#2E7D32");
                        statusText = Color.FromArgb("#FFFFFF");
                    }        

                    items.Add(new ProgramItem
                    {
                        LoyaltyName = loyalty.LoyaltyName,
                        ItemName = loyalty.ItemName ?? "All eligible items",
                        DealLabel = dealLabel,
                        StartDateStr = startStr,
                        EndDateStr = endStr,
                        EndDateColor = endColor,
                        StatusLabel = statusLabel,
                        StatusBgColor = statusBg,
                        StatusTextColor = statusText
                    });
                }

                Programs = items;
                IsEmpty = false;
                HasPrograms = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProgramsViewModel] Error: {ex.Message}");
                IsEmpty = true;
                HasPrograms = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}