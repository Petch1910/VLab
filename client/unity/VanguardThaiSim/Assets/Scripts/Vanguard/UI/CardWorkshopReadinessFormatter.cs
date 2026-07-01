using System;
using System.Text;

namespace VanguardThaiSim.UI
{
    public static class CardWorkshopReadinessFormatter
    {
        public const string ClearFiltersAction = "Use Clear or adjust search and filters.";

        public static string FormatPreparing(CardBrowserScreenMode mode)
        {
            return CardBrowserModeFormatter.FormatTitle(mode) + "\n" +
                   "Preparing local card pack and filters.";
        }

        public static string FormatFailure(CardBrowserScreenMode mode, string reason, string expectedPack)
        {
            return CardBrowserModeFormatter.FormatTitle(mode) + "\n" +
                   "Card data is unavailable.\n" +
                   "Expected pack: " + FormatValue(expectedPack) + "\n" +
                   FormatValue(reason);
        }

        public static string FormatReady(
            CardBrowserScreenMode mode,
            int totalCards,
            int showingCards,
            string searchText,
            string series,
            string group)
        {
            int safeTotal = Math.Max(0, totalCards);
            int safeShowing = Math.Max(0, showingCards);
            string filters = DeckBuilderFilterPanelFormatter.FormatFilters(searchText, series, group);

            StringBuilder builder = new StringBuilder();
            builder.Append(CardBrowserModeFormatter.FormatTitle(mode));
            builder.AppendLine(safeShowing == 0 ? " ready - no matching cards" : " ready");
            builder.Append("Showing ");
            builder.Append(safeShowing);
            builder.Append(" of ");
            builder.Append(safeTotal);
            builder.Append(" cards.");
            builder.AppendLine();
            builder.Append("Filters: ");
            builder.Append(filters);

            if (safeShowing == 0)
            {
                builder.AppendLine();
                builder.Append(ClearFiltersAction);
                return builder.ToString();
            }

            builder.AppendLine();
            builder.Append(mode == CardBrowserScreenMode.DeckBuilder
                ? "Select a card, then add it to Main or Ride."
                : "Select a card to read details.");
            return builder.ToString();
        }

        private static string FormatValue(string value)
        {
            string compact = Compact(value, 80);
            return string.IsNullOrEmpty(compact) ? "unknown" : compact;
        }

        private static string Compact(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string compact = string.Join(" ", value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            if (maxLength > 3 && compact.Length > maxLength)
            {
                return compact.Substring(0, maxLength - 3) + "...";
            }

            return compact;
        }
    }
}
