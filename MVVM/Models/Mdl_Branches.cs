using CommunityToolkit.Mvvm.ComponentModel;
using Maui.GoogleMaps;

namespace FREDLoyalty_App.MVVM.Models
{
    public class Mdl_Branch
    {
        public int BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string GoogleMapLink { get; set; }
    }

    public class Mdl_BranchResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<Mdl_Branch> Data { get; set; }
    }

    // ── Branch item shown on the location page ───────────────
    public partial class BranchLocationItem : ObservableObject
    {
        public int BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public string GoogleMapLink { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [ObservableProperty] private string distance = "Locating...";
        [ObservableProperty] private string eta = "";
        [ObservableProperty] private double rawKm = double.MaxValue;

        public Position Position => new(Latitude, Longitude);
    }

    public class Branches
    {
        public int BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
    }
}
