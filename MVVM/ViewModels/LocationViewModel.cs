using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.GoogleMaps;
using System.Collections.ObjectModel;
using FREDLoyalty_App.MVVM.Models;

namespace FREDLoyalty_App.MVVM.ViewModels
{


    public partial class LocationViewModel : ObservableObject
    {
        // ── Maps API key ─────────────────────────────────────
        private const string MapsApiKey = "AIzaSyBkJsD_KEjzkTE_r5_WvOmMSsLuyM-TGec";

        // ── Branch items — shared with LocationPage ───────────
        private List<BranchLocationItem> _branchItems = new();

        [ObservableProperty] private bool isLocating = false;
        [ObservableProperty] private bool isNotLocating = true;
        [ObservableProperty] private bool hasBranches = false;
        [ObservableProperty] private bool noBranches = true;

        public INavigation Navigation { get; set; }

        public LocationViewModel() { }

        // ════════════════════════════════════════════════════
        // Called by LocationPage with items it already built
        // VM updates the SAME objects — binding updates UI
        // ════════════════════════════════════════════════════
        public void SetBranchItems(List<BranchLocationItem> items)
        {
            _branchItems = items;
            HasBranches = items.Any();
            NoBranches = !HasBranches;
        }

        // ════════════════════════════════════════════════════
        // Fetch distances + update items in place
        // No need to rebuild cards — binding handles UI update
        // ════════════════════════════════════════════════════
        public async Task LoadDistancesAsync()
        {
            if (!_branchItems.Any()) return;

            try
            {
                IsLocating = true;
                IsNotLocating = false;

                foreach (var b in _branchItems)
                {
                    b.Distance = "Locating...";
                    b.Eta = "";
                }

                // ── Permission ────────────────────────────────
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    foreach (var b in _branchItems)
                        b.Distance = "Permission denied";
                    return;
                }

                // ── GPS ───────────────────────────────────────
                var location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium,
                        TimeSpan.FromSeconds(10)));

                if (location == null)
                {
                    foreach (var b in _branchItems)
                        b.Distance = "Unable to get location";
                    return;
                }

                var userPos = new Position(location.Latitude, location.Longitude);

                // ── Distance Matrix API ───────────────────────
                await FetchDistancesAsync(userPos);

                // ── Sort cards by nearest — rebuild order ─────
                var sorted = _branchItems.OrderBy(b => b.RawKm).ToList();
                _branchItems = sorted;

                // Notify page to reorder cards
                OnBranchesReady?.Invoke(sorted);
            }
            catch (FeatureNotEnabledException)
            {
                foreach (var b in _branchItems) b.Distance = "GPS is off";
            }
            catch (FeatureNotSupportedException)
            {
                foreach (var b in _branchItems) b.Distance = "GPS not supported";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocationVM] {ex.Message}");
                foreach (var b in _branchItems) b.Distance = "Unavailable";
            }
            finally
            {
                IsLocating = false;
                IsNotLocating = true;
            }
        }

        // ── Event to tell page to reorder cards ───────────────
        public event Action<List<BranchLocationItem>> OnBranchesReady;

        // ════════════════════════════════════════════════════
        // Distance Matrix API
        // ════════════════════════════════════════════════════
        private async Task FetchDistancesAsync(Position userPos)
        {
            try
            {
                var destinations = string.Join("|",
                    _branchItems.Select(b => $"{b.Latitude},{b.Longitude}"));

                var url = "https://maps.googleapis.com/maps/api/distancematrix/json" +
                          $"?origins={userPos.Latitude},{userPos.Longitude}" +
                          $"&destinations={destinations}" +
                          $"&mode=driving" +
                          $"&units=metric" +
                          $"&departure_time=now" +
                          $"&traffic_model=best_guess" +
                          $"&key={MapsApiKey}";

                using var client = new HttpClient();
                var json = await client.GetStringAsync(url);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var elements = doc.RootElement
                                        .GetProperty("rows")[0]
                                        .GetProperty("elements");

                for (int i = 0; i < _branchItems.Count; i++)
                {
                    var branch = _branchItems[i];
                    var element = elements[i];

                    if (element.GetProperty("status").GetString() == "OK")
                    {
                        branch.Distance = element.GetProperty("distance")
                                                 .GetProperty("text")
                                                 .GetString() ?? "N/A";

                        branch.Eta = element.TryGetProperty(
                                        "duration_in_traffic", out var t)
                            ? t.GetProperty("text").GetString() ?? "N/A"
                            : element.GetProperty("duration")
                                     .GetProperty("text")
                                     .GetString() ?? "N/A";

                        branch.RawKm = element.GetProperty("distance")
                                              .GetProperty("value")
                                              .GetDouble() / 1000.0;
                    }
                    else
                    {
                        double km = HaversineKm(userPos, branch.Position);
                        branch.Distance = FormatDistance(km);
                        branch.Eta = EstimateEta(km);
                        branch.RawKm = km;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DistanceMatrix] {ex.Message}");
                foreach (var branch in _branchItems)
                {
                    double km = HaversineKm(userPos, branch.Position);
                    branch.Distance = FormatDistance(km);
                    branch.Eta = EstimateEta(km);
                    branch.RawKm = km;
                }
            }
        }

        // ════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════
        private static double HaversineKm(Position a, Position b)
        {
            const double R = 6371;
            var dLat = ToRad(b.Latitude - a.Latitude);
            var dLon = ToRad(b.Longitude - a.Longitude);
            var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRad(a.Latitude)) * Math.Cos(ToRad(b.Latitude))
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;

        private static string FormatDistance(double km)
            => km < 1 ? $"{(int)(km * 1000)} m away" : $"{km:F1} km away";

        private static string EstimateEta(double km)
        {
            int min = (int)Math.Ceiling(km / 30.0 * 60);
            if (min < 1) return "~1 min drive";
            if (min < 60) return $"~{min} min drive";
            int h = min / 60, m = min % 60;
            return m == 0 ? $"~{h}h drive" : $"~{h}h {m}min drive";
        }

        // ════════════════════════════════════════════════════
        // Commands
        // ════════════════════════════════════════════════════
        [RelayCommand]
        private async Task RefreshLocation()
            => await LoadDistancesAsync();

        // ════════════════════════════════════════════════════
        // Commands
        // ════════════════════════════════════════════════════

        [RelayCommand]
        private async Task OpenBranch(BranchLocationItem branch)
        {
            if (branch == null || string.IsNullOrWhiteSpace(branch.GoogleMapLink))
                return;
            try
            {
                await Browser.OpenAsync(branch.GoogleMapLink, BrowserLaunchMode.External);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocationVM] OpenBranch: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not open Google Maps.", "OK");
            }
        }


        [RelayCommand]
        private async Task GoBack()
        {
            if (Navigation != null)
                await Navigation.PopAsync();
            else
                await Shell.Current.GoToAsync("..");
        }
    }
}