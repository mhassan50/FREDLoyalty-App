using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class VerificationPage : ContentPage
{
    private readonly VerificationViewModel _viewModel;
       private string _tempDay = string.Empty;
    private string _tempMonth = string.Empty;
    private string _tempYear = string.Empty;

    private static readonly string[] MonthNames =
    {
        "January","February","March","April","May","June",
        "July","August","September","October","November","December"
    };
    public VerificationPage()
	{
		InitializeComponent();
        _viewModel = new VerificationViewModel(new CommonRepo());
        BindingContext = _viewModel;
        DobPicker.DateConfirmed += (day, month, year) =>
        {
            _viewModel.SelectedDay = day;
            _viewModel.SelectedMonth = month;
            _viewModel.SelectedYear = year;
            _viewModel.UpdateDobDisplay();
        };
    }
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        // disabled — do nothing
    }

       private void OnDobTapped(object sender, TappedEventArgs e)
    {
        DobPicker.Show(
            _viewModel.SelectedDay,
            _viewModel.SelectedMonth,
            _viewModel.SelectedYear);
    }

    private static bool IsValidDate(int day, int month, int year)
    {
        try
        {
            _ = new DateTime(year, month, day);
            return true;
        }
        catch { return false; }
    }


}