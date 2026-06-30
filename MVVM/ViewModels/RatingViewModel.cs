using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FREDLoyalty_App.MVVM.Models;
using FREDLoyalty_App.MVVM.Repos;

namespace FREDLoyalty_App.MVVM.ViewModels
{
    public partial class RatingViewModel : ObservableObject
    {
        // ── Branch info — set by BranchSelectionPage ─────────
        [ObservableProperty] private string branchName = "Polymath Coffee";
        [ObservableProperty] private int branchCode = 0;
        [ObservableProperty] private string branchAddress = string.Empty;
        [ObservableProperty] private bool hasAddress;

        // ── Star rating ───────────────────────────────────────
        [ObservableProperty] private int currentRating = 0;
        [ObservableProperty] private string star1 = "☆";
        [ObservableProperty] private string star2 = "☆";
        [ObservableProperty] private string star3 = "☆";
        [ObservableProperty] private string star4 = "☆";
        [ObservableProperty] private string star5 = "☆";
        [ObservableProperty] private string ratingLabel = "Tap a star to rate";

        // ── Suggestion ────────────────────────────────────────
        [ObservableProperty] private string suggestionText = string.Empty;
        [ObservableProperty] private string charCount = "0 / 500";

        // ── State ─────────────────────────────────────────────
        [ObservableProperty] private bool canSubmit = false;
        [ObservableProperty] private bool cannotSubmit = true;
        [ObservableProperty] private bool isSubmitting = false;
        [ObservableProperty] private bool isNotSubmitting = true;
        [ObservableProperty] private bool isSubmittingSuggestion = false;
        [ObservableProperty] private bool isNotSubmittingSuggestion = true;
        [ObservableProperty] private bool canSubmitSuggestion = false;

        private readonly IFeedback _ratingService;

        public INavigation Navigation { get; set; }

        public RatingViewModel(IFeedback ratingService)
        {
            _ratingService = ratingService;
        }

        // ── Called by BranchSelectionPage before pushing ──────
        public void SetBranch(Branches branch)
        {
            if (branch == null) return;
            BranchName = branch.BranchName ?? "Polymath Coffee";
            BranchCode = branch.BranchCode;
            BranchAddress = branch.BranchAddress ?? string.Empty;
        }

        // ── HasAddress updates when BranchAddress changes ─────
        partial void OnBranchAddressChanged(string value)
            => HasAddress = !string.IsNullOrWhiteSpace(value);

        // ── Suggestion ────────────────────────────────────────
        partial void OnSuggestionTextChanged(string value)
        {
            int len = value?.Length ?? 0;
            CharCount = $"{len} / 500";
            if (len > 500)
                SuggestionText = value[..500];
            CanSubmitSuggestion = len > 0;
        }

        // ─────────────────────────────────────────────────────
        // SUBMIT SUGGESTION
        // ─────────────────────────────────────────────────────
        [RelayCommand]
        private async Task SubmitSuggestion()
        {
            if (string.IsNullOrWhiteSpace(SuggestionText)) return;

            try
            {
                IsSubmittingSuggestion = true;
                IsNotSubmittingSuggestion = false;

                await _ratingService.InsertSuggestionAsync(new Mdl_CustomerSuggestion
                {
                    CustomerCode = App._Customer?.CustomerCode ?? 0,
                    Suggestion = SuggestionText,
                    EntryTime = DateTime.Now
                });

                SuggestionText = string.Empty;

                await Shell.Current.DisplayAlert(
                    "Thank You! 💡",
                    "Your suggestion has been sent. We appreciate your feedback!",
                    "OK");

                if (Navigation != null)
                    await Navigation.PopModalAsync(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RatingVM] SubmitSuggestion: {ex.Message}");
                await Shell.Current.DisplayAlert(
                    "Error", "Could not send suggestion. Please try again.", "OK");
            }
            finally
            {
                IsSubmittingSuggestion = false;
                IsNotSubmittingSuggestion = true;
            }
        }

        // ─────────────────────────────────────────────────────
        // SET RATING
        // ─────────────────────────────────────────────────────
        [RelayCommand]
        private void SetRating(string param)
        {
            if (!int.TryParse(param, out int rating)) return;

            CurrentRating = rating;

            Star1 = rating >= 1 ? "★" : "☆";
            Star2 = rating >= 2 ? "★" : "☆";
            Star3 = rating >= 3 ? "★" : "☆";
            Star4 = rating >= 4 ? "★" : "☆";
            Star5 = rating >= 5 ? "★" : "☆";

            RatingLabel = rating switch
            {
                1 => "😞  Poor",
                2 => "😐  Fair",
                3 => "🙂  Good",
                4 => "😊  Great",
                5 => "🤩  Excellent!",
                _ => "Tap a star to rate"
            };

            CanSubmit = true;
            CannotSubmit = false;
        }

        // ─────────────────────────────────────────────────────
        // SUBMIT FEEDBACK
        // ─────────────────────────────────────────────────────
        [RelayCommand]
        private async Task Submit()
        {
            if (CurrentRating == 0) return;

            try
            {
                IsSubmitting = true;
                IsNotSubmitting = false;

                await _ratingService.InsertFeedbackAsync(new Mdl_CustomerFeedback
                {
                    CustomerCode = App._Customer?.CustomerCode ?? 0,
                    BranchCode = BranchCode,
                    Rating = CurrentRating,
                    Feedback = SuggestionText,
                    EntryTime = DateTime.Now
                });

                await Shell.Current.DisplayAlert(
                    "Thank You! ☕",
                    "Your feedback has been submitted. We appreciate it!",
                    "OK");

            if (Navigation != null)
                await Shell.Current.GoToAsync("BranchSelection");
            else
                await Shell.Current.GoToAsync("..");            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RatingVM] Submit: {ex.Message}");
                await Shell.Current.DisplayAlert(
                    "Error", "Could not submit feedback. Please try again.", "OK");
            }
            finally
            {
                IsSubmitting = false;
                IsNotSubmitting = true;
            }
        }

        // ─────────────────────────────────────────────────────
        // GO BACK — pops to BranchSelectionPage
        // ─────────────────────────────────────────────────────
        [RelayCommand]
        private async Task GoBack()
        {
            if (IsSubmitting) return;
            if (Navigation != null)
                await Shell.Current.GoToAsync("BranchSelection");
            else
                await Shell.Current.GoToAsync("..");
        }
    }
}