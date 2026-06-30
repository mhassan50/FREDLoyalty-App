using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace FREDLoyalty_App.MVVM.Views;

public class ImageCropPage : ContentPage
{
    public event Action<byte[]>? OnImageCropped;

    private readonly SKBitmap _bitmap;
    private readonly SKCanvasView _canvas;

    // Transform state
    private float _scale = 1f;
    private float _minScale = 1f;
    private float _translateX = 0f;
    private float _translateY = 0f;
    private float _startPanX = 0f;
    private float _startPanY = 0f;
    private float _rotation = 0f;

    // Physical pixel size of the canvas surface (set on first paint)
    private float _surfaceSize = 0f;
    private bool _initialized = false;

    private const float CanvasSize = 300f;
    private const float MaxScale = 3f;
    private const float ScaleStep = 0.5f;
    private const float RotateStep = 90f;
    private const float RotateFine = 1f;

    private readonly Slider _zoomSlider;
    private readonly Label _zoomLabel;
    private readonly Label _rotateLabel;

    public ImageCropPage(byte[] imageBytes)
    {
        Shell.SetNavBarIsVisible(this, false);
        BackgroundColor = Color.FromArgb("#111111");

        _bitmap = SKBitmap.Decode(imageBytes);

        // ── Canvas ───────────────────────────────────────────────
        _canvas = new SKCanvasView
        {
            WidthRequest = CanvasSize,
            HeightRequest = CanvasSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        _canvas.PaintSurface += OnPaintSurface;

        // ── Pan — committed base pattern ─────────────────────────
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += (s, e) =>
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    _translateX = _startPanX + (float)e.TotalX;
                    _translateY = _startPanY + (float)e.TotalY;
                    _canvas.InvalidateSurface();
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _startPanX = _translateX;
                    _startPanY = _translateY;
                    break;
            }
        };
        _canvas.GestureRecognizers.Add(pan);

        // ── Zoom slider ──────────────────────────────────────────
        _zoomSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            MinimumTrackColor = Color.FromArgb("#616b46"),
            MaximumTrackColor = Color.FromArgb("#444444"),
            ThumbColor = Colors.White,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
        };
        _zoomSlider.ValueChanged += OnSliderChanged;

        _zoomLabel = new Label
        {
            Text = "1.0×",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 40,
        };

        _rotateLabel = new Label
        {
            Text = "0°",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 40,
        };

        // ── Zoom row ─────────────────────────────────────────────
        var zoomOutBtn = BuildIconButton("−", () =>
            ApplyScale(Math.Max(_minScale, _scale - ScaleStep)));
        var zoomInBtn = BuildIconButton("+", () =>
            ApplyScale(Math.Min(MaxScale, _scale + ScaleStep)));
        var zoomRow = BuildControlRow(zoomOutBtn, _zoomSlider, _zoomLabel, zoomInBtn, "ZOOM");

        // ── Rotate row ───────────────────────────────────────────
        var rotateCCW = BuildIconButton("↺", () => ApplyRotation(_rotation - RotateStep));
        var rotateCW = BuildIconButton("↻", () => ApplyRotation(_rotation + RotateStep));
        var rotateMin = BuildIconButton("−", () => ApplyRotation(_rotation - RotateFine));
        var rotatePls = BuildIconButton("+", () => ApplyRotation(_rotation + RotateFine));

        var rotateRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },  // ROTATE label
                new ColumnDefinition { Width = GridLength.Auto },  // ↺
                new ColumnDefinition { Width = GridLength.Auto },  // −
                new ColumnDefinition { Width = GridLength.Star },  // degree label
                new ColumnDefinition { Width = GridLength.Auto },  // +
                new ColumnDefinition { Width = GridLength.Auto },  // ↻
            },
            ColumnSpacing = 8,
            Padding = new Thickness(24, 0, 24, 20),
            BackgroundColor = Color.FromArgb("#111111"),
        };
        rotateRow.Add(new Label
        {
            Text = "ROTATE",
            FontSize = 9,
            CharacterSpacing = 1.5,
            TextColor = Color.FromArgb("#777777"),
            VerticalOptions = LayoutOptions.Center,
        }, 0, 0);
        rotateRow.Add(rotateCCW, 1, 0);
        rotateRow.Add(rotateMin, 2, 0);
        rotateRow.Add(_rotateLabel, 3, 0);
        rotateRow.Add(rotatePls, 4, 0);
        rotateRow.Add(rotateCW, 5, 0);

        // ── Action buttons ───────────────────────────────────────
        var cancelBtn = new Button
        {
            Text = "Cancel",
            BackgroundColor = Color.FromArgb("#2E2E2E"),
            TextColor = Colors.White,
            CornerRadius = 22,
            HeightRequest = 50,
            WidthRequest = 130,
            FontSize = 14,
        };
        cancelBtn.Clicked += async (s, e) =>
        {
            _bitmap.Dispose();
            await Navigation.PopModalAsync();
        };

        var useBtn = new Button
        {
            Text = "Use Photo",
            BackgroundColor = Color.FromArgb("#616b46"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 22,
            HeightRequest = 50,
            WidthRequest = 130,
            FontSize = 14,
        };
        useBtn.Clicked += OnUsePhotoClicked;

        // ── Layout ───────────────────────────────────────────────
        Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 0,
            BackgroundColor = Color.FromArgb("#111111"),
            Children =
            {
                new Label
                {
                    Text              = "Adjust Photo",
                    FontSize          = 18,
                    FontAttributes    = FontAttributes.Bold,
                    TextColor         = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin            = new Thickness(0, 60, 0, 28),
                },

                new Frame
                {
                    CornerRadius      = CanvasSize / 2,
                    IsClippedToBounds = true,
                    Padding           = 0,
                    HasShadow         = false,
                    BorderColor       = Color.FromArgb("#616b46"),
                    WidthRequest      = CanvasSize,
                    HeightRequest     = CanvasSize,
                    HorizontalOptions = LayoutOptions.Center,
                    Content           = _canvas,
                },

                new Label
                {
                    Text              = "Drag to reposition",
                    FontSize          = 11,
                    TextColor         = Color.FromArgb("#777777"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin            = new Thickness(0, 12, 0, 0),
                },

                zoomRow,
                rotateRow,

                new HorizontalStackLayout
                {
                    Spacing           = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin            = new Thickness(0, 8, 0, 40),
                    Children          = { cancelBtn, useBtn }
                },
            }
        };
    }

    // ── Build control row (zoom) ──────────────────────────────────
    private static Grid BuildControlRow(
        View leftBtn, View middle, View valueLabel, View rightBtn, string label)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 8,
            Padding = new Thickness(24, 20, 24, 8),
            BackgroundColor = Color.FromArgb("#111111"),
        };
        grid.Add(new Label
        {
            Text = label,
            FontSize = 9,
            CharacterSpacing = 1.5,
            TextColor = Color.FromArgb("#777777"),
            VerticalOptions = LayoutOptions.Center,
        }, 0, 0);
        grid.Add(leftBtn, 1, 0);
        grid.Add(middle, 2, 0);
        grid.Add(valueLabel, 3, 0);
        grid.Add(rightBtn, 4, 0);
        return grid;
    }

    // ── Build icon button ─────────────────────────────────────────
    private static Button BuildIconButton(string text, Action onTap)
    {
        var btn = new Button
        {
            Text = text,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#2E2E2E"),
            CornerRadius = 18,
            HeightRequest = 36,
            WidthRequest = 36,
            Padding = new Thickness(0),
        };
        btn.Clicked += (s, e) => onTap();
        return btn;
    }

    // ── Slider → zoom ────────────────────────────────────────────
    private void OnSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        float t = (float)(e.NewValue / 100.0);
        _scale = _minScale + t * (MaxScale - _minScale);

        if (_scale <= _minScale + 0.05f)
            ResetPan();

        UpdateZoomLabel();
        _canvas.InvalidateSurface();
    }

    // ── Button → zoom ────────────────────────────────────────────
    private void ApplyScale(float newScale)
    {
        _scale = newScale;

        if (_scale <= _minScale + 0.05f)
            ResetPan();

        _zoomSlider.ValueChanged -= OnSliderChanged;
        _zoomSlider.Value = ((_scale - _minScale) / (MaxScale - _minScale)) * 100.0;
        _zoomSlider.ValueChanged += OnSliderChanged;

        UpdateZoomLabel();
        _canvas.InvalidateSurface();
    }

    // ── Button → rotate ──────────────────────────────────────────
    private void ApplyRotation(float degrees)
    {
        _rotation = ((degrees % 360f) + 360f) % 360f;
        if (_rotation > 180f) _rotation -= 360f;
        UpdateRotateLabel();
        _canvas.InvalidateSurface();
    }

    // ── Reset pan ────────────────────────────────────────────────
    private void ResetPan()
    {
        _translateX = 0f;
        _translateY = 0f;
        _startPanX = 0f;
        _startPanY = 0f;
    }

    private void UpdateZoomLabel()
        => _zoomLabel.Text = $"{_scale / _minScale:F1}×";

    private void UpdateRotateLabel()
        => _rotateLabel.Text = $"{_rotation:F0}°";

    // ── SkiaSharp render ─────────────────────────────────────────
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var canvas = e.Surface.Canvas;

        // ── Initialize using PHYSICAL pixel dimensions ────────────
        // This runs once — after this _minScale and _scale are in physical px space
        if (!_initialized)
        {
            _surfaceSize = info.Width;   // square canvas so width == height

            // Cover: image fills the circle completely, no black gaps
            _minScale = Math.Max(
                (float)info.Width / _bitmap.Width,
                (float)info.Height / _bitmap.Height);

            _scale = _minScale;
            _initialized = true;

            // Sync slider without firing event
            _zoomSlider.ValueChanged -= OnSliderChanged;
            _zoomSlider.Value = 0;
            _zoomSlider.ValueChanged += OnSliderChanged;
            UpdateZoomLabel();
        }

        canvas.Clear(SKColors.Black);
        canvas.Save();

        canvas.Translate(
            info.Width / 2f + _translateX,
            info.Height / 2f + _translateY);
        canvas.RotateDegrees(_rotation);
        canvas.Scale(_scale);
        canvas.Translate(-_bitmap.Width / 2f, -_bitmap.Height / 2f);
        canvas.DrawBitmap(_bitmap, 0, 0);

        canvas.Restore();
    }

    // ── Crop and return ──────────────────────────────────────────
    private async void OnUsePhotoClicked(object? sender, EventArgs e)
    {
        const int outputSize = 400;

        using var surface = SKSurface.Create(
            new SKImageInfo(outputSize, outputSize, SKColorType.Rgba8888));

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // Clip output to circle — matches what Frame shows on screen
        using var circlePath = new SKPath();
        circlePath.AddCircle(outputSize / 2f, outputSize / 2f, outputSize / 2f);
        canvas.ClipPath(circlePath, SKClipOperation.Intersect, antialias: true);

        // _scale/_translateX/_translateY are in physical pixel space
        // Map them to outputSize space
        float renderScale = (float)outputSize / _surfaceSize;

        canvas.Save();
        canvas.Translate(
            outputSize / 2f + _translateX * renderScale,
            outputSize / 2f + _translateY * renderScale);
        canvas.RotateDegrees(_rotation);
        canvas.Scale(_scale * renderScale);
        canvas.Translate(-_bitmap.Width / 2f, -_bitmap.Height / 2f);
        canvas.DrawBitmap(_bitmap, 0, 0);
        canvas.Restore();

        // PNG preserves the transparent corners
        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        var croppedBytes = data.ToArray();

        _bitmap.Dispose();
        await Navigation.PopModalAsync();
        OnImageCropped?.Invoke(croppedBytes);
    }
}