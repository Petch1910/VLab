using System;
using System.Text;
using VanguardThaiSim.Cards;

namespace VanguardThaiSim.UI
{
    public static class CardBrowserSearchPanelFormatter
    {
        public const string EmptyTitle = "No cards found";
        public const string NoActiveFiltersMessage = "No active search filters.";
        public const string ClearFiltersHint = "Try clearing search or filters.";

        public static string FormatEmptyResult(CardQueryOptions options)
        {
            if (options == null)
            {
                return NoActiveFiltersMessage;
            }

            return FormatEmptyResult(options.SearchText, options.Series, FormatTaxonomy(options.Clan, options.Nation));
        }

        public static string FormatEmptyResult(string searchText, string series, string group)
        {
            StringBuilder builder = new StringBuilder();
            AppendLine(builder, "Query", Normalize(searchText), true);
            AppendLine(builder, "Series", Normalize(series), false);
            AppendLine(builder, "Group", Normalize(group), false);

            if (builder.Length == 0)
            {
                return NoActiveFiltersMessage;
            }

            builder.Append(ClearFiltersHint);
            return builder.ToString();
        }

        public static string Normalize(string value, int maxLength = 64)
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

        private static string FormatTaxonomy(string clan, string nation)
        {
            string normalizedClan = Normalize(clan);
            if (!string.IsNullOrEmpty(normalizedClan))
            {
                return "Clan " + normalizedClan;
            }

            string normalizedNation = Normalize(nation);
            if (!string.IsNullOrEmpty(normalizedNation))
            {
                return "Nation " + normalizedNation;
            }

            return string.Empty;
        }

        private static void AppendLine(StringBuilder builder, string label, string value, bool quoteValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(label);
            builder.Append(": ");
            if (quoteValue)
            {
                builder.Append('"');
            }

            builder.Append(value);
            if (quoteValue)
            {
                builder.Append('"');
            }
        }
    }
}
