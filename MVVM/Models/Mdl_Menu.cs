namespace FREDLoyalty_App.MVVM.Models
{

    //public class Mdl_MenuItem
    //{
    //    public string DispCategoryCode { get; set; } = string.Empty;
    //    public string DispCategoryName { get; set; } = string.Empty;
    //    public string DispItemCode { get; set; } = string.Empty;
    //    public string DispItemName { get; set; } = string.Empty;
    //    public int? Rate { get; set; } = 0;
    //    public string DispItemDescription { get; set; } = string.Empty;
    //    public string? Snap { get; set; }

    //    // ── Decoded image, cached so it only builds once ──────────
    //    private byte[]? _imageBytes;
    //    private bool _bytesDecoded;

    //    private byte[]? ImageBytes
    //    {
    //        get
    //        {
    //            if (_bytesDecoded) return _imageBytes;
    //            _bytesDecoded = true;

    //            try
    //            {
    //                if (string.IsNullOrWhiteSpace(Snap))
    //                    return _imageBytes = null;

    //                // Strip data URI prefix if present
    //                // e.g. "data:image/jpeg;base64,/9j/4AAQ..."
    //                string base64 = Snap.Contains(',')
    //                    ? Snap.Substring(Snap.IndexOf(',') + 1)
    //                    : Snap;

    //                _imageBytes = Convert.FromBase64String(base64);
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine($"[MenuItem] Image decode error: {ex.Message}");
    //                _imageBytes = null;
    //            }

    //            return _imageBytes;
    //        }
    //    }

    // ── ImageSource for XAML — returns a fresh stream each time
    //    from the cached bytes (no re-decode of Base64) ─────────
    //    public ImageSource? ItemImage
    //    {
    //        get
    //        {
    //            var bytes = ImageBytes;
    //            if (bytes == null) return null;
    //            return ImageSource.FromStream(() => new MemoryStream(bytes));
    //        }
    //    }

    //    // ── Computed — show image or emoji ──────────────────
    //    public bool HasImage => !string.IsNullOrWhiteSpace(Snap);
    //    public bool HasNoImage => !HasImage;
    //    public bool HasRate => Rate.HasValue && Rate > 0;
    //    public bool HasDescription => !string.IsNullOrWhiteSpace(DispItemDescription);
    //}


    public class Mdl_MenuItem
    {
        public string DispCategoryCode { get; set; } = string.Empty;
        public string DispCategoryName { get; set; } = string.Empty;
        public string DispItemCode { get; set; } = string.Empty;
        public string DispItemName { get; set; } = string.Empty;
        public int? Rate { get; set; } = 0;
        public string DispItemDescription { get; set; } = string.Empty;

        // Snap no longer carries the image — kept only if the API still sends it.
        public string? Snap { get; set; }

        // ── Image URL on the server ───────────────────────────
        private static string ImageBaseUrl => $"https://webappwwwrootlink/FoodImages/";

        public string? ItemImageUrl =>
            string.IsNullOrWhiteSpace(DispItemCode)
                ? null
                : $"{ImageBaseUrl}{DispItemCode}.jpeg";

        // ── ImageSource for XAML — loads from URL, cached by MAUI ─
        public ImageSource? ItemImage =>
            string.IsNullOrWhiteSpace(ItemImageUrl)
                ? null
                : ImageSource.FromUri(new Uri(ItemImageUrl));

        // ── Computed — show image or emoji ──────────────────
        public bool HasImage => !string.IsNullOrWhiteSpace(DispItemCode);
        public bool HasNoImage => !HasImage;
        public bool HasRate => Rate.HasValue && Rate > 0;
        public bool HasDescription => !string.IsNullOrWhiteSpace(DispItemDescription);
    }
    public class Mdl_MenuCriteria
    {
        public int  BranchCode { get; set; }
        public bool IsActive   { get; set; } = true;
    }

    public class Mdl_MenuResult
    {
        public bool                IsSuccess { get; set; }
        public string              Message   { get; set; } = string.Empty;
        public List<Mdl_MenuItem>? Data      { get; set; }
    }

    public class Mdl_MenuCategory : List<Mdl_MenuItem>
    {
        public string             CategoryCode { get; set; } = string.Empty;
        public string             CategoryName { get; set; } = string.Empty;
        public List<Mdl_MenuItem> Items        { get; set; } = new();
    }
}