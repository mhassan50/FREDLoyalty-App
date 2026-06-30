using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class LocationPage : ContentPage
{
    private readonly LocationViewModel _vm;
    private bool _hasLoaded = false;

    public LocationPage()
    {
        InitializeComponent();
        _vm = new LocationViewModel();
        _vm.Navigation = Navigation;
        BindingContext = _vm;

        // ── Subscribe — reorder cards when distances arrive ───
        _vm.OnBranchesReady += ReorderCards;

        // ── Build cards immediately — maps start loading NOW ──
        BuildCardsFromAppBranches();
    }

    // ════════════════════════════════════════════════════════
    // Build one BranchMapCell per branch
    // Position is known from App.Branches so maps can start
    // loading immediately without waiting for distances
    // ════════════════════════════════════════════════════════
private void BuildCardsFromAppBranches()
{
    var appBranches = App.Branches;
    if (appBranches == null || !appBranches.Any())
    {
        Console.WriteLine("[LocationPage] App.Branches is empty!");
        return;
    }

    // ── Log all branch coordinates ────────────────────────
    foreach (var b in appBranches)
        Console.WriteLine($"[Branch] {b.BranchName} | " +
                          $"Lat={b.Latitude} Lon={b.Longitude} | " +
                          $"MapLink={b.GoogleMapLink}");

    var items = appBranches.Select(b => new BranchLocationItem
    {
        BranchCode    = b.BranchCode,
        BranchName    = b.BranchName,
        BranchAddress = b.BranchAddress,
        GoogleMapLink = b.GoogleMapLink,
        Latitude      = b.Latitude,
        Longitude     = b.Longitude,
        Distance      = "Locating...",
        Eta           = ""
    }).ToList();

    _vm.SetBranchItems(items);

    foreach (var item in items)
        CardsContainer.Children.Add(new BranchMapCell(item));
}
    // ════════════════════════════════════════════════════════
    // Reorder cards after distances are fetched
    // Nearest branch floats to top
    // ════════════════════════════════════════════════════════
    private void ReorderCards(List<BranchLocationItem> sorted)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Collect existing cells keyed by BranchCode
            var cellMap = CardsContainer.Children
                .OfType<BranchMapCell>()
                .ToDictionary(c => ((BranchLocationItem)c.BindingContext).BranchCode);

            CardsContainer.Children.Clear();

            foreach (var item in sorted)
            {
                if (cellMap.TryGetValue(item.BranchCode, out var cell))
                    CardsContainer.Children.Add(cell);
            }
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_hasLoaded) return;
        _hasLoaded = true;
        _ = _vm.LoadDistancesAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = _vm.GoBackCommand.ExecuteAsync(null);
        return true;
    }
}