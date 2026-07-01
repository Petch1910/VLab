namespace VanguardThaiSim.UI
{
    public static class CardImageFallbackStatusFormatter
    {
        public const string TileFallbackSuffix = "[image fallback]";
        public const string MissingPathDetailStatus = "Image: fallback (missing path)";
        public const string MissingFileDetailStatus = "Image: fallback (file missing or unreadable)";

        public static string FormatTileLabel(string cardLabel, bool usesFallback)
        {
            string label = string.IsNullOrWhiteSpace(cardLabel) ? "Unknown card" : cardLabel.Trim();
            return usesFallback ? label + "\n" + TileFallbackSuffix : label;
        }

        public static string FormatDetailStatus(bool usesFallback, string imageRelativePath)
        {
            if (!usesFallback)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(imageRelativePath)
                ? MissingPathDetailStatus
                : MissingFileDetailStatus;
        }

        public static string FormatDetailStatusWithTip(bool usesFallback, string imageRelativePath)
        {
            string status = FormatDetailStatus(usesFallback, imageRelativePath);
            return string.IsNullOrEmpty(status)
                ? string.Empty
                : LoadingTipCatalog.AppendTip(status, LoadingTipCatalog.CardImages);
        }
    }
}
