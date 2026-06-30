using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace FREDLoyalty_App.MVVM.Views;

public partial class RegistrationPage : ContentPage
{
    private readonly RegistrationViewModel _viewModel;

    private string _tempDay = string.Empty;
    private string _tempMonth = string.Empty;
    private string _tempYear = string.Empty;

    private static readonly string[] MonthNames =
    {
        "January","February","March","April","May","June",
        "July","August","September","October","November","December"
    };

    public RegistrationPage()
    {
        InitializeComponent();
        _viewModel = new RegistrationViewModel(new CommonRepo());
        BindingContext = _viewModel;
        DobPicker.DateConfirmed += (day, month, year) =>
        {
            _viewModel.SelectedDay = day;
            _viewModel.SelectedMonth = month;
            _viewModel.SelectedYear = year;
            _viewModel.UpdateDobDisplay();
        };
    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    // Build only once, lazily after page is visible
    //    if (DayList.Children.Count == 0)
    //    {
    //        Task.Run(() =>
    //        {
    //            MainThread.BeginInvokeOnMainThread(BuildDobLists);
    //        });
    //    }
    //}


    //// ─────────────────────────────────────────────────────
    //// HELPERS
    //// ─────────────────────────────────────────────────────

    ///// <summary>
    ///// Returns how many days are in the currently selected month/year.
    ///// Falls back to 31 if month or year aren't chosen yet.
    ///// </summary>
    //private int GetDaysInSelectedMonth()
    //{
    //    int monthIndex = Array.IndexOf(MonthNames, _tempMonth) + 1; // 1-12, 0 if not found
    //    if (monthIndex <= 0) return 31;

    //    int year = int.TryParse(_tempYear, out int y) ? y : DateTime.Now.Year;
    //    return DateTime.DaysInMonth(year, monthIndex);
    //}

    //// ─────────────────────────────────────────────────────
    //// BUILD LISTS
    //// ─────────────────────────────────────────────────────

    //private void BuildDobLists()
    //{
    //    RefreshDayList();   // builds days based on current month/year

    //    BuildList(MonthList,
    //        MonthNames,
    //        val =>
    //        {
    //            _tempMonth = val;
    //            HighlightSelected(MonthList, val);
    //            RefreshDayList();   // month changed → recalc days
    //        });

    //    BuildList(YearList,
    //        Enumerable.Range(1901, DateTime.Now.Year - 1900)
    //                  .Reverse()
    //                  .Select(y => y.ToString()),
    //        val =>
    //        {
    //            _tempYear = val;
    //            HighlightSelected(YearList, val);
    //            RefreshDayList();   // year changed → recalc days (leap year)
    //        });
    //}

    ///// <summary>
    ///// Rebuilds the Day list based on current _tempMonth and _tempYear.
    ///// If the previously selected day exceeds the new max, it is cleared.
    ///// </summary>
    //private void RefreshDayList()
    //{
    //    int maxDays = GetDaysInSelectedMonth();

    //    // If current selection is now invalid, clear it
    //    if (int.TryParse(_tempDay, out int currentDay) && currentDay > maxDays)
    //        _tempDay = string.Empty;

    //    BuildList(DayList,
    //        Enumerable.Range(1, maxDays).Select(d => d.ToString()),
    //        val =>
    //        {
    //            _tempDay = val;
    //            HighlightSelected(DayList, val);
    //        });

    //    // Re-apply highlight if selection is still valid
    //    if (!string.IsNullOrEmpty(_tempDay))
    //        HighlightSelected(DayList, _tempDay);
    //}

    //// ─────────────────────────────────────────────────────
    //// BUILD / HIGHLIGHT
    //// ─────────────────────────────────────────────────────

    //private void BuildList(VerticalStackLayout container,
    //                       IEnumerable<string> items,
    //                       Action<string> onSelect)
    //{
    //    container.Children.Clear();
    //    foreach (var item in items)
    //    {
    //        var label = new Label
    //        {
    //            Text = item,
    //            FontSize = 14,
    //            HorizontalOptions = LayoutOptions.Center,
    //            VerticalOptions = LayoutOptions.Center,
    //            Padding = new Thickness(0),
    //            TextColor = Color.FromArgb("#555555")
    //        };

    //        var border = new Border
    //        {
    //            Content = label,
    //            BackgroundColor = Colors.Transparent,
    //            StrokeThickness = 0,
    //            Padding = new Thickness(10, 7),
    //            HorizontalOptions = LayoutOptions.Center,
    //            MinimumWidthRequest = 60,
    //            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) }
    //        };

    //        var tap = new TapGestureRecognizer();
    //        tap.Tapped += (s, e) => onSelect(item);
    //        border.GestureRecognizers.Add(tap);

    //        container.Children.Add(border);
    //    }
    //}

    //private void HighlightSelected(VerticalStackLayout container, string selected)
    //{
    //    foreach (var child in container.Children)
    //    {
    //        if (child is Border border && border.Content is Label lbl)
    //        {
    //            bool isSelected = lbl.Text == selected;

    //            border.BackgroundColor = isSelected
    //                ? Color.FromArgb("#40616b46")
    //                : Colors.Transparent;

    //            lbl.TextColor = isSelected
    //                ? (Color)Application.Current.Resources["Primary"]
    //                : Color.FromArgb("#555555");

    //            lbl.FontAttributes = isSelected
    //                ? FontAttributes.Bold
    //                : FontAttributes.None;

    //            lbl.FontSize = isSelected ? 15 : 14;
    //        }
    //    }
    //}

    // ─────────────────────────────────────────────────────
    // OVERLAY EVENTS
    // ─────────────────────────────────────────────────────

    //private void OnDobTapped(object sender, TappedEventArgs e)
    //{
    //    _tempDay = _viewModel.SelectedDay;
    //    _tempMonth = _viewModel.SelectedMonth;
    //    _tempYear = _viewModel.SelectedYear;

    //    // Rebuild day list in case month/year were previously set
    //    RefreshDayList();

    //    if (!string.IsNullOrEmpty(_tempDay))
    //        HighlightSelected(DayList, _tempDay);
    //    if (!string.IsNullOrEmpty(_tempMonth))
    //        HighlightSelected(MonthList, _tempMonth);
    //    if (!string.IsNullOrEmpty(_tempYear))
    //        HighlightSelected(YearList, _tempYear);

    //    DobOverlay.IsVisible = true;
    //}

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

    // ─────────────────────────────────────────────────────
    // NAVIGATION
    // ─────────────────────────────────────────────────────

    private void OnBackButtonClicked(object sender, EventArgs e)
        => _ = Navigation.PopAsync();

    private async void OnLoginClicked(object sender, TappedEventArgs e)
    {
        try { await Navigation.PopAsync(); }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }
}