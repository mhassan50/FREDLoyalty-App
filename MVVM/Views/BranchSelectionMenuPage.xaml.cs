using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;
using FREDLoyalty_App.MVVM.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace FREDLoyalty_App.MVVM.Views;

public partial class BranchSelectionMenuPage : ContentPage
{
    public BranchSelectionMenuPage()
    {
        InitializeComponent();
        BuildBranchCards();
    }

    private void BuildBranchCards()
    {
        var branches = App.Branches;
        if (branches == null || !branches.Any()) return;

        foreach (var branch in branches)
            BranchList.Children.Add(BuildCard(branch));
    }

    private View BuildCard(Mdl_Branch branch)
    {
        var icon = new Border
        {
            StrokeShape      = new RoundRectangle { CornerRadius = 14 },
            BackgroundColor  = (Color)Application.Current.Resources["Primary"],
            StrokeThickness  = 0,
            WidthRequest     = 52,
            HeightRequest    = 52,
            Content = new Label
            {
                Text              = "☕",
                FontSize          = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions   = LayoutOptions.Center
            }
        };

        var nameLabel = new Label
        {
            Text           = branch.BranchName,
            FontSize       = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor      = (Color)Application.Current.Resources["Primary"]
        };

        var addressLabel = new Label
        {
            Text          = branch.BranchAddress ?? string.Empty,
            FontSize      = 11,
            TextColor     = (Color)Application.Current.Resources["LightText"],
            LineBreakMode = LineBreakMode.WordWrap,
            IsVisible     = !string.IsNullOrWhiteSpace(branch.BranchAddress)
        };

        var chevron = new Label
        {
            Text              = "›",
            FontSize          = 26,
            TextColor         = (Color)Application.Current.Resources["Primary"],
            VerticalOptions   = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing   = 14,
            VerticalOptions = LayoutOptions.Center
        };

        row.Add(icon, 0, 0);
        row.Add(new VerticalStackLayout
        {
            Spacing         = 3,
            VerticalOptions = LayoutOptions.Center,
            Children        = { nameLabel, addressLabel }
        }, 1, 0);
        row.Add(chevron, 2, 0);

        var card = new Border
        {
            StrokeShape     = new RoundRectangle { CornerRadius = 18 },
            BackgroundColor = Colors.White,
            Stroke          = (Color)Application.Current.Resources["Gray200"],
            StrokeThickness = 1,
            Padding         = new Thickness(16, 14),
            Shadow = new Shadow
            {
                Brush   = new SolidColorBrush(Colors.Black),
                Offset  = new Point(0, 2),
                Radius  = 8,
                Opacity = 0.07f
            },
            Content = row
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await card.ScaleTo(0.97, 80);
            await card.ScaleTo(1.0, 80);

            var vm = new MenuViewModel(new MenuService())
            {
                Navigation = Navigation
            };

            vm.SetBranch(branch);

            await Navigation.PushAsync(new MenuPage(vm));
        };

        card.GestureRecognizers.Add(tap);
        return card;
    }


    private async void OnBackTapped(object sender, TappedEventArgs e)
        => await OnBackButtonClicked();

    protected override bool OnBackButtonPressed()
    {
        _ = OnBackButtonClicked();
        return true;
    }

    private async Task OnBackButtonClicked()
    {
        await Shell.Current.GoToAsync("//Home");
    }
}