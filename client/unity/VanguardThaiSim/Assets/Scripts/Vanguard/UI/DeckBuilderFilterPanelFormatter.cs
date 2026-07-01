using System;
using System.Text;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public static class DeckBuilderFilterPanelFormatter
    {
        public const string NoFiltersLabel = "none";
        public const string MissingDeckStatusMessage = "Deck status unavailable.";
        public const string MissingIssuesMessage = "No validation result.";
        public const string NoIssuesMessage = "No validation issues.";

        public static string FormatCardPoolStatus(
            int totalCards,
            int showingCards,
            string searchText,
            string series,
            string group,
            string packVersion,
            CardPackValidationStatus packValidationStatus)
        {
            return "Card pool" +
                   " | Total " + Math.Max(0, totalCards) +
                   " | Showing " + Math.Max(0, showingCards) +
                   " | Filters: " + FormatFilters(searchText, series, group) +
                   " | Pack " + FormatValue(packVersion) +
                   " | " + CardPackValidationStatusFormatter.FormatCompact(packValidationStatus);
        }

        public static string FormatDeckStatus(
            DeckValidationResult result,
            int mainTarget = 50,
            int rideMax = 4,
            int gMax = 16)
        {
            if (result == null)
            {
                return MissingDeckStatusMessage;
            }

            return "Deck status" +
                   "\nMain " + result.MainCount + "/" + Math.Max(0, mainTarget) +
                   " | Ride " + result.RideCount + "/" + Math.Max(0, rideMax) +
                   " | G " + result.GCount + "/" + Math.Max(0, gMax) +
                   "\nIssues: " + result.ErrorCount + " errors / " + result.WarningCount + " warnings" +
                   "\nPlayable: " + (result.IsPlayable ? "yes" : "no");
        }

        public static string FormatRuleBadge(VanguardDeck deck, DeckValidationResult result)
        {
            string format = deck == null ? "unknown" : FormatValue(deck.format);
            string playable = result != null && result.IsPlayable ? "Playable" : "Needs check";
            return "Rule: " + format + " | " + playable;
        }

        public static string FormatDeckCounters(
            DeckValidationResult result,
            int mainTarget = 50,
            int rideMax = 4,
            int gMax = 16)
        {
            if (result == null)
            {
                return MissingDeckStatusMessage;
            }

            return "Counters | Main " + result.MainCount + "/" + Math.Max(0, mainTarget) +
                   " | Ride " + result.RideCount + "/" + Math.Max(0, rideMax) +
                   " | G " + result.GCount + "/" + Math.Max(0, gMax) +
                   "\nIssues | " + result.ErrorCount + " errors / " + result.WarningCount + " warnings";
        }

        public static string FormatIssues(DeckValidationResult result, int maxIssues = 4)
        {
            if (result == null)
            {
                return MissingIssuesMessage;
            }

            if (result.Issues.Count == 0)
            {
                return NoIssuesMessage;
            }

            int limit = Math.Max(0, Math.Min(maxIssues, result.Issues.Count));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < limit; i++)
            {
                DeckValidationIssue issue = result.Issues[i];
                builder.Append(issue.Severity == DeckValidationSeverity.Error ? "Error: " : "Warn: ");
                builder.Append(FormatValue(issue.Code));
                if (!string.IsNullOrWhiteSpace(issue.CardId))
                {
                    builder.Append(" ");
                    builder.Append(Compact(issue.CardId, 36));
                }

                if (issue.Zone.HasValue)
                {
                    builder.Append(" (");
                    builder.Append(issue.Zone.Value);
                    builder.Append(")");
                }

                if (i + 1 < limit)
                {
                    builder.AppendLine();
                }
            }

            if (result.Issues.Count > limit)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append("+");
                builder.Append(result.Issues.Count - limit);
                builder.Append(" more issues");
            }

            return builder.ToString();
        }

        public static string FormatFilters(string searchText, string series, string group)
        {
            StringBuilder builder = new StringBuilder();
            AppendFilter(builder, "search", searchText, true);
            AppendFilter(builder, "series", series, false);
            AppendFilter(builder, "group", group, false);
            return builder.Length == 0 ? NoFiltersLabel : builder.ToString();
        }

        private static void AppendFilter(StringBuilder builder, string label, string value, bool quoteValue)
        {
            string compact = Compact(value, 48);
            if (string.IsNullOrEmpty(compact))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append("; ");
            }

            builder.Append(label);
            builder.Append(" ");
            if (quoteValue)
            {
                builder.Append('"');
            }

            builder.Append(compact);
            if (quoteValue)
            {
                builder.Append('"');
            }
        }

        private static string FormatValue(string value)
        {
            string compact = Compact(value, 48);
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
