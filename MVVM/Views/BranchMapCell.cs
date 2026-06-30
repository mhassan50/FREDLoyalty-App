using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.ViewModels;
using Maui.GoogleMaps;
using Microsoft.Maui.Controls.Shapes;

namespace FREDLoyalty_App.MVVM.Views
{
    public class BranchMapCell : ContentView
    {
        private readonly Maui.GoogleMaps.Map _map;
        private readonly Grid _mapGrid;
        private BranchLocationItem _branch;
        private bool _cameraSet = false;

        public BranchMapCell(BranchLocationItem branch)
        {
            _branch = branch;

            _map = new Maui.GoogleMaps.Map
            {
                MapType = MapType.Street,
                HeightRequest = 200,
                IsTrafficEnabled = false,
                IsIndoorEnabled = false,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,

                // ── Set camera BEFORE native init — never shows 0,0 ──
                InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(
                    branch.Position, 16d)
            };

            // ── Pin added immediately ─────────────────────────────
            _map.Pins.Add(new Pin
            {
                Type = PinType.Place,
                Label = branch.BranchName,
                Address = branch.BranchAddress ?? string.Empty,
                Position = branch.Position,
                Icon = BitmapDescriptorFactory.DefaultMarker(
                               Color.FromArgb("#F4C542"))
            });

            // ── Remove MapReady — no longer needed ────────────────
            // _map.MapReady += OnMapReady;  ← DELETE

            _mapGrid = new Grid
            {
                HeightRequest = 200,
                IsClippedToBounds = true,
                Children = { _map }
            };

            BindingContext = branch;
            Content = BuildLayout();
        }

        // ── Native map surface is ready — move camera now ────
        //private void OnMapReady(object sender, EventArgs e)
        //{
        //    if (_cameraSet) return;
        //    _cameraSet = true;

        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        _map.MoveToRegion(MapSpan.FromCenterAndRadius(
        //            _branch.Position, Distance.FromMeters(200)));
        //    });
        //}

        // ════════════════════════════════════════════════════
        // Build card layout
        // ════════════════════════════════════════════════════
        private View BuildLayout()
        {
            // ── Distance badge on map ─────────────────────────
            var distLabel = new Label
            {
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            distLabel.SetBinding(Label.TextProperty,
                nameof(BranchLocationItem.Distance));

            var etaLabel = new Label
            {
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            etaLabel.SetBinding(Label.TextProperty,
                nameof(BranchLocationItem.Eta));

            var distanceBadge = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Color.FromArgb("#CC1a1d13"),
                StrokeThickness = 0,
                Padding = new Thickness(10, 6),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(10, 0, 0, 10),
                Content = new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label { Text = "📍", FontSize = 11,
                                    VerticalOptions = LayoutOptions.Center },
                        distLabel,
                        new Label { Text = "·", FontSize = 11,
                                    TextColor = Color.FromArgb("#AAFFFFFF"),
                                    VerticalOptions = LayoutOptions.Center },
                        new Label { Text = "🚗", FontSize = 11,
                                    VerticalOptions = LayoutOptions.Center },
                        etaLabel
                    }
                }
            };

            var tapBadge = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Color.FromArgb("#AA000000"),
                StrokeThickness = 0,
                Padding = new Thickness(10, 5),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 10, 10),
                Content = new Label
                {
                    Text = "TAP TO OPEN ↗",
                    FontSize = 9,
                    CharacterSpacing = 1,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                }
            };

            var tapOverlay = new Grid { BackgroundColor = Colors.Transparent };
            var mapTap = new TapGestureRecognizer();
            mapTap.Tapped += OnOpenMapTapped;
            tapOverlay.GestureRecognizers.Add(mapTap);

            _mapGrid.Children.Add(distanceBadge);
            _mapGrid.Children.Add(tapBadge);
            _mapGrid.Children.Add(tapOverlay);

            // ── Branch name ───────────────────────────────────
            var nameLabel = new Label
            {
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#616b46")
            };
            nameLabel.SetBinding(Label.TextProperty,
                nameof(BranchLocationItem.BranchName));

            var addressLabel = new Label
            {
                FontSize = 12,
                TextColor = Color.FromArgb("#757575"),
                LineBreakMode = LineBreakMode.WordWrap
            };
            addressLabel.SetBinding(Label.TextProperty,
                nameof(BranchLocationItem.BranchAddress));

            var openBadge = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Color.FromArgb("#f4f5f0"),
                StrokeThickness = 0,
                Padding = new Thickness(10, 5),
                VerticalOptions = LayoutOptions.Start,
                Content = new Label
                {
                    Text = "● OPEN",
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#616b46"),
                    CharacterSpacing = 0.5
                }
            };

            var nameRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };
            nameRow.Add(new VerticalStackLayout
            {
                Spacing = 3,
                Children = { nameLabel, addressLabel }
            }, 0, 0);
            nameRow.Add(openBadge, 1, 0);

            // ── Distance + ETA row ────────────────────────────
            var distRowLabel = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#616b46"),
                VerticalOptions = LayoutOptions.Center
            };
            distRowLabel.SetBinding(Label.TextProperty,
                nameof(BranchLocationItem.Distance));

            var etaRowLabel = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#616b46"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };
            etaRowLabel.SetBinding(Label.TextProperty,
                nameof(BranchLocationItem.Eta));

            var distRowGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                }
            };
            distRowGrid.Add(new HorizontalStackLayout
            {
                Spacing = 6,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "📍", FontSize = 13,
                                VerticalOptions = LayoutOptions.Center },
                    distRowLabel
                }
            }, 0, 0);
            distRowGrid.Add(new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "🚗", FontSize = 13,
                                VerticalOptions = LayoutOptions.Center },
                    etaRowLabel
                }
            }, 1, 0);

            var distBorder = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Color.FromArgb("#f4f5f0"),
                Stroke = Color.FromArgb("#c8ceaf"),
                StrokeThickness = 1,
                Padding = new Thickness(14, 10),
                Content = distRowGrid
            };

            var directionsBtn = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                BackgroundColor = Color.FromArgb("#616b46"),
                StrokeThickness = 0,
                HeightRequest = 48,
                Content = new Label
                {
                    Text = "🗺  GET DIRECTIONS",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    CharacterSpacing = 1.5,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.White
                }
            };
            var dirTap = new TapGestureRecognizer();
            dirTap.Tapped += OnOpenMapTapped;
            directionsBtn.GestureRecognizers.Add(dirTap);

            return new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                Margin = new Thickness(0, 0, 0, 20),
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Colors.Black),
                    Offset = new Point(0, 4),
                    Radius = 12,
                    Opacity = 0.08f
                },
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children =
                    {
                        _mapGrid,
                        new VerticalStackLayout
                        {
                            Padding  = new Thickness(18, 16, 18, 18),
                            Spacing  = 12,
                            Children =
                            {
                                nameRow,
                                new BoxView
                                {
                                    HeightRequest   = 0.5,
                                    BackgroundColor = Color.FromArgb("#E0E0E0")
                                },
                                distBorder,
                                directionsBtn
                            }
                        }
                    }
                }
            };
        }

        private async void OnOpenMapTapped(object sender, TappedEventArgs e)
        {
            if (_branch == null) return;
            if (string.IsNullOrWhiteSpace(_branch.GoogleMapLink)) return;
            try
            {
                await Browser.OpenAsync(_branch.GoogleMapLink,
                    BrowserLaunchMode.External);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BranchMapCell] {ex.Message}");
            }
        }
    }
}