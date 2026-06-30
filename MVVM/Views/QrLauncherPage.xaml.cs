namespace FREDLoyalty_App.MVVM.Views;

public partial class QrLauncherPage : ContentPage
{
    public QrLauncherPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCustomerData();
    }

    private void LoadCustomerData()
    {
        var customer = App._Customer;
        if (customer == null) return;

        CustomerNameLabel.Text = customer.CustomerName;
        //CustomerCodeLabel.Text = $"ID: {customer.CustomerCode}";
        QrCodeView.Value       = customer.CustomerCode.ToString();
    }

    private async void OnGoHomeTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//Home");

    protected override bool OnBackButtonPressed()
    {
        _ = Shell.Current.GoToAsync("//Home");
        return true;
    }
    
}