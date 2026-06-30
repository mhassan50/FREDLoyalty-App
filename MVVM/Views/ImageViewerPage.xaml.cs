namespace FREDLoyalty_App.MVVM.Views
{
    public partial class ImageViewerPage : ContentPage
    {
        private double _currentScale = 1.0;
        private double _panX = 0;
        private double _panY = 0;

        private const double MinScale = 1.0;
        private const double TapZoom = 3.0;

        public ImageViewerPage(ImageSource imageSource)
        {
            InitializeComponent();
            ZoomImage.Source = imageSource;
            ZoomImage.AnchorX = 0.5;
            ZoomImage.AnchorY = 0.5;

            AttachGestures();
            DismissHintAfterDelay();

            // Open already zoomed in
            Dispatcher.Dispatch(async () => await AnimateZoomIn());
        }

        // ─────────────────────────────────────────────
        // Gestures
        // Pan on CONTAINER → translates CHILD (fixes Android stutter)
        // Double-tap on CONTAINER → close
        // ─────────────────────────────────────────────
        private void AttachGestures()
        {
            // Pan on parent container, not on ZoomImage
            var pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPan;
            ImageContainer.GestureRecognizers.Add(pan);

            // Double-tap to close
            var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTap.Tapped += async (s, e) => await CloseAsync();
            ImageContainer.GestureRecognizers.Add(doubleTap);
        }

        // ─────────────────────────────────────────────
        // PAN — parent listens, child moves
        // No multiplier needed — direct TotalX/TotalY
        // from committed _panX/_panY base
        // ─────────────────────────────────────────────
        private void OnPan(object sender, PanUpdatedEventArgs e)
        {
            if (_currentScale <= MinScale) return;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    ZoomImage.TranslationX = Math.Clamp(
                        _panX + e.TotalX,
                        -MaxX(), MaxX());
                    ZoomImage.TranslationY = Math.Clamp(
                        _panY + e.TotalY,
                        -MaxY(), MaxY());
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    // Commit so next drag starts from here
                    _panX = ZoomImage.TranslationX;
                    _panY = ZoomImage.TranslationY;
                    break;
            }
        }

        // ─────────────────────────────────────────────
        // Zoom in to 3×
        // ─────────────────────────────────────────────
        private async Task AnimateZoomIn()
        {
            _currentScale = TapZoom;
            _panX = 0;
            _panY = 0;

            await Task.WhenAll(
                ZoomImage.ScaleTo(TapZoom, 200, Easing.CubicOut),
                ZoomImage.TranslateTo(0, 0, 200, Easing.CubicOut));

            UpdateZoomLabel();
        }

        // ─────────────────────────────────────────────
        // Reset to 1× (1:1 button)
        // ─────────────────────────────────────────────
        private async Task AnimateReset()
        {
            _currentScale = MinScale;
            _panX = 0;
            _panY = 0;

            await Task.WhenAll(
                ZoomImage.ScaleTo(MinScale, 200, Easing.CubicOut),
                ZoomImage.TranslateTo(0, 0, 200, Easing.CubicOut));

            UpdateZoomLabel();
        }

        // ─────────────────────────────────────────────
        // Clamp bounds based on current scale
        // ─────────────────────────────────────────────
        private double MaxX()
            => ImageContainer.Width > 0
                ? (ImageContainer.Width * (_currentScale - 1)) / 2
                : 0;

        private double MaxY()
            => ImageContainer.Height > 0
                ? (ImageContainer.Height * (_currentScale - 1)) / 2
                : 0;

        private void UpdateZoomLabel()
            => ZoomLabel.Text = $"{_currentScale:F1}×";

        private async void DismissHintAfterDelay()
        {
            await Task.Delay(2500);
            await HintLabel.FadeTo(0, 500);
        }

        // ─────────────────────────────────────────────
        // Buttons
        // ─────────────────────────────────────────────
        private async void OnResetTapped(object sender, TappedEventArgs e)
            => await AnimateReset();

        private async void OnCloseClicked(object sender, TappedEventArgs e)
            => await CloseAsync();

        protected override bool OnBackButtonPressed()
        {
            _ = CloseAsync();
            return true;
        }

        private async Task CloseAsync()
        {
            await this.FadeTo(0, 180);
            await Navigation.PopModalAsync(false);
        }
    }
}