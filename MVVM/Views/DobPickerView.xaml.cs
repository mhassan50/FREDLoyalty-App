using System.ComponentModel;
using System.Windows.Input;

namespace FREDLoyalty_App.MVVM.Views;

public partial class DobPickerView : ContentView
{
    public event Action<string, string, string>? DateConfirmed;

    private string _tempDay = string.Empty;
    private string _tempMonth = string.Empty;
    private string _tempYear = string.Empty;
    private bool _isBuilt = false;

    private static readonly string[] MonthNames =
    {
        "January","February","March","April",
        "May","June","July","August",
        "September","October","November","December"
    };

    private static readonly List<DobItem> _monthItems =
        MonthNames.Select(m => new DobItem(m)).ToList();

    private static readonly List<DobItem> _yearItems =
        Enumerable.Range(1901, DateTime.Now.Year - 1900)
                  .Reverse()
                  .Select(y => new DobItem(y.ToString()))
                  .ToList();

    // ── Commands for tap gestures in DataTemplate ────────
    public ICommand DayTappedCommand { get; }
    public ICommand MonthTappedCommand { get; }
    public ICommand YearTappedCommand { get; }

    public DobPickerView()
    {
        InitializeComponent();
        BindingContext = this;

        DayTappedCommand = new Command<DobItem>(item =>
        {
            if (item == null) return;
            var list = DayCV.ItemsSource as List<DobItem>;
            list?.ForEach(i => i.SetSelected(i == item));
            _tempDay = item.Value;
        });

        MonthTappedCommand = new Command<DobItem>(item =>
        {
            if (item == null) return;
            _monthItems.ForEach(i => i.SetSelected(i == item));
            _tempMonth = item.Value;
            RefreshDayList();
        });

        YearTappedCommand = new Command<DobItem>(item =>
        {
            if (item == null) return;
            _yearItems.ForEach(i => i.SetSelected(i == item));
            _tempYear = item.Value;
            RefreshDayList();
        });
    }

    // ── Public API ───────────────────────────────────────
    public void Show(string day, string month, string year)
    {
        _tempDay = day;
        _tempMonth = month;
        _tempYear = year;

        if (!_isBuilt)
        {
            MonthCV.ItemsSource = _monthItems;
            YearCV.ItemsSource = _yearItems;
            _isBuilt = true;
        }

        RefreshDayList();
        ApplySelections();
        IsVisible = true;
    }

    public void Hide() => IsVisible = false;

    // ── Day list refresh ─────────────────────────────────
    private void RefreshDayList()
    {
        int maxDays = GetDaysInMonth();

        if (int.TryParse(_tempDay, out int d) && d > maxDays)
            _tempDay = string.Empty;

        DayCV.ItemsSource = Enumerable.Range(1, maxDays)
                                      .Select(n => new DobItem(n.ToString()))
                                      .ToList();
    }

    private int GetDaysInMonth()
    {
        int m = Array.IndexOf(MonthNames, _tempMonth) + 1;
        if (m <= 0) return 31;
        int y = int.TryParse(_tempYear, out int yr) ? yr : DateTime.Now.Year;
        return DateTime.DaysInMonth(y, m);
    }

    // ── Restore highlights when reopening ────────────────
    private void ApplySelections()
    {
        if (!string.IsNullOrEmpty(_tempDay))
        {
            var list = DayCV.ItemsSource as List<DobItem>;
            list?.ForEach(i => i.SetSelected(i.Value == _tempDay));
        }

        if (!string.IsNullOrEmpty(_tempMonth))
            _monthItems.ForEach(i => i.SetSelected(i.Value == _tempMonth));

        if (!string.IsNullOrEmpty(_tempYear))
            _yearItems.ForEach(i => i.SetSelected(i.Value == _tempYear));
    }

    // ── Confirm / Dismiss ────────────────────────────────
    private void OnDismiss(object sender, TappedEventArgs e) => Hide();

    private void OnConfirmed(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_tempDay) ||
            string.IsNullOrEmpty(_tempMonth) ||
            string.IsNullOrEmpty(_tempYear))
        {
            Application.Current.MainPage
                .DisplayAlert("Incomplete", "Please select day, month and year.", "OK");
            return;
        }

        int m = Array.IndexOf(MonthNames, _tempMonth) + 1;
        int d = int.Parse(_tempDay);
        int y = int.Parse(_tempYear);

        try { _ = new DateTime(y, m, d); }
        catch
        {
            Application.Current.MainPage
                .DisplayAlert("Invalid Date",
                    $"{_tempMonth} {y} only has {DateTime.DaysInMonth(y, m)} days.", "OK");
            _tempDay = string.Empty;
            RefreshDayList();
            return;
        }

        DateConfirmed?.Invoke(_tempDay, _tempMonth, _tempYear);
        Hide();
    }
}

// ── DobItem ──────────────────────────────────────────────
public class DobItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Value { get; }

    private bool _isSelected;

    private static readonly Color SelectedBg = Color.FromArgb("#40616b46");
    private static readonly Color UnselectedBg = Colors.Transparent;
    private static readonly Color SelectedText =
        (Color)Application.Current.Resources["Primary"];
    private static readonly Color UnselectedText = Color.FromArgb("#555555");

    public Color BgColor => _isSelected ? SelectedBg : UnselectedBg;
    public Color TextColor => _isSelected ? SelectedText : UnselectedText;
    public double FontSize => _isSelected ? 15 : 14;
    public FontAttributes FontAttr =>
        _isSelected ? FontAttributes.Bold : FontAttributes.None;

    public DobItem(string value) => Value = value;

    public void SetSelected(bool selected)
    {
        if (_isSelected == selected) return;
        _isSelected = selected;
        var changed = PropertyChanged;
        changed?.Invoke(this, new PropertyChangedEventArgs(nameof(BgColor)));
        changed?.Invoke(this, new PropertyChangedEventArgs(nameof(TextColor)));
        changed?.Invoke(this, new PropertyChangedEventArgs(nameof(FontSize)));
        changed?.Invoke(this, new PropertyChangedEventArgs(nameof(FontAttr)));
    }
}