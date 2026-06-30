using FREDLoyalty_App.MVVM.ViewModels;

namespace FREDLoyalty_App.MVVM.Views;

public partial class RatingPage : ContentPage
{
    private CancellationTokenSource _animCts;

    public RatingPage(RatingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.Navigation = Navigation;
        // Restart ticker when branch changes
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartTicker();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTicker();
    }

    private void StopTicker()
    {
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = null;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TickerLabel.CancelAnimations();
            TickerLabel.TranslationX = 0;
        });
    }

    private void StartTicker()
    {
        StopTicker();

        _animCts = new CancellationTokenSource();
        var token = _animCts.Token;

        _ = Task.Run(async () =>
        {
            // ── Longer wait — let layout fully settle ─────────
            await Task.Delay(1200);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    double scrollDistance = 0;

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // ── Unconstrained measure ─────────────────
                        var desired = TickerLabel.Measure(
                                               double.PositiveInfinity,
                                               double.PositiveInfinity);
                        double textWidth = desired.Width;

                        // ── Container = ContentView width ─────────
                        double containerWidth = TickerLabel.Parent is View p
                            ? p.Width : 300;

                        Console.WriteLine(
                            $"[Ticker] text={textWidth:F1} " +
                            $"container={containerWidth:F1}");

                        scrollDistance = textWidth > containerWidth
                            ? textWidth - containerWidth
                            : 0;

                        TickerLabel.TranslationX = 0;
                    });

                    if (scrollDistance <= 0)
                    {
                        await Task.Delay(1000, token);
                        continue;
                    }

                    await Task.Delay(1200, token);
                    if (token.IsCancellationRequested) break;

                    // ── Scroll left ───────────────────────────────
                    var tcs1 = new TaskCompletionSource();
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await TickerLabel.TranslateTo(
                            -scrollDistance, 0,
                            (uint)(scrollDistance * 25),
                            Easing.Linear);
                        tcs1.TrySetResult();
                    });
                    await tcs1.Task;

                    await Task.Delay(1200, token);
                    if (token.IsCancellationRequested) break;

                    // ── Scroll right ──────────────────────────────
                    var tcs2 = new TaskCompletionSource();
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await TickerLabel.TranslateTo(
                            0, 0,
                            (uint)(scrollDistance * 25),
                            Easing.Linear);
                        tcs2.TrySetResult();
                    });
                    await tcs2.Task;

                    await Task.Delay(800, token);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Ticker] {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }, token);
    }
    protected override bool OnBackButtonPressed()
    {
        // Intercept back button → navigate to home
        _ = OnBackButtonClicked(this, EventArgs.Empty);
        return true; // handled
    }

    private async Task OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//More");
    }
}