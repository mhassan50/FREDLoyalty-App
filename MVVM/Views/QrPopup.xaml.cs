namespace FREDLoyalty_App.MVVM.Views;

public partial class QrPopup : ContentPage
{
    public TaskCompletionSource PopupClosed { get; } = new();

    public QrPopup()
    {
        InitializeComponent();
        LoadCustomerData();
    }

    private void LoadCustomerData()
    {
        var customer = App._Customer;
        if (customer == null) return;
        CustomerNameLabel.Text = customer.CustomerName;
        QrCodeView.Value = customer.CustomerCode.ToString();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
        => await ClosePopupAsync();

    private async void OnOverlayTapped(object sender, TappedEventArgs e)
        => await ClosePopupAsync();

    // ── Android back button inside popup ────────────────────
    protected override bool OnBackButtonPressed()
    {
        _ = ClosePopupAsync();
        return true;
    }

    // ── Page disappearing as fallback signal ─────────────────
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PopupClosed.TrySetResult(); // safe — TrySetResult won't throw if already set
    }

    private async Task ClosePopupAsync()
    {
        await this.FadeTo(0, 200);
        await Navigation.PopModalAsync(false);
        PopupClosed.TrySetResult();
    }
}