using System.Text;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public static class HomeLobbyStatusFormatter
    {
        public const string TaxonomyBaseline =
            "Card filters: Vanguard Area-style clan/nation buckets from the local database.";

        public static string FormatPackStatus(CardPackManifest manifest, string loadError)
        {
            if (!string.IsNullOrWhiteSpace(loadError))
            {
                return "Pack: unavailable\n" + Trim(loadError, 120);
            }

            if (manifest == null)
            {
                return "Pack: loading";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("Pack: ");
            builder.Append(string.IsNullOrWhiteSpace(manifest.display_name) ? manifest.pack_id : manifest.display_name);
            builder.Append(" / ");
            builder.Append(string.IsNullOrWhiteSpace(manifest.source_version) ? "unknown version" : manifest.source_version);
            builder.Append('\n');
            builder.Append("Cards ");
            builder.Append(manifest.card_count);
            builder.Append(" / Series ");
            builder.Append(manifest.series_count);
            builder.Append(" / Clans ");
            builder.Append(manifest.clan_count);
            return builder.ToString();
        }

        public static string FormatDeckStatus(VanguardDeck deck, DeckValidationResult validation)
        {
            if (deck == null)
            {
                return "Deck: no saved deck\nUse Deck Builder / Cards first.";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("Deck: ");
            builder.Append(string.IsNullOrWhiteSpace(deck.name) ? "Untitled Deck" : deck.name);
            builder.Append('\n');
            builder.Append("Main ");
            builder.Append(deck.TotalCards(DeckZone.Main));
            builder.Append(" / Ride ");
            builder.Append(deck.TotalCards(DeckZone.Ride));
            builder.Append(" / G ");
            builder.Append(deck.TotalCards(DeckZone.G));

            if (validation != null)
            {
                builder.Append('\n');
                builder.Append(validation.ErrorCount);
                builder.Append(" errors / ");
                builder.Append(validation.WarningCount);
                builder.Append(" warnings");
            }

            return builder.ToString();
        }

        public static string FormatModeStatus(string lastActionMessage)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Mode: Local manual table\nOnline: trusted-client room\nCPU: locked until bot milestones");
            if (!string.IsNullOrWhiteSpace(lastActionMessage))
            {
                builder.Append('\n');
                builder.Append(Trim(lastActionMessage, 100));
            }

            return builder.ToString();
        }

        public static string FormatIconPackStatus(UserIconPackValidationResult result)
        {
            if (result == null)
            {
                return "Icons: default text badges\nPrivate overrides: not checked";
            }

            if (!result.accepted)
            {
                return "Icons: default text badges\nPrivate overrides rejected: " + FirstIssueCode(result, "error");
            }

            if (result.accepted_icon_count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Icons: user overrides ");
                builder.Append(result.accepted_icon_count);
                builder.Append('/');
                builder.Append(result.declared_icon_count);
                if (result.fallback_icon_count > 0)
                {
                    builder.Append('\n');
                    builder.Append("Fallbacks: ");
                    builder.Append(result.fallback_icon_count);
                    builder.Append(" defaults");
                }

                return builder.ToString();
            }

            if (result.declared_icon_count > 0)
            {
                return "Icons: default text badges\nPrivate overrides: 0/" + result.declared_icon_count + " loaded";
            }

            return "Icons: default text badges\nPrivate overrides: none loaded";
        }

        private static string Trim(string value, int maxLength)
        {
            string compact = string.Join(" ", value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries));
            if (compact.Length <= maxLength)
            {
                return compact;
            }

            return compact.Substring(0, maxLength - 3) + "...";
        }

        private static string FirstIssueCode(UserIconPackValidationResult result, string severity)
        {
            if (result == null || result.issues == null)
            {
                return "UNKNOWN";
            }

            for (int i = 0; i < result.issues.Count; i++)
            {
                UserIconPackValidationIssue issue = result.issues[i];
                if (issue != null &&
                    string.Equals(issue.severity, severity, System.StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(issue.code))
                {
                    return Trim(issue.code, 40);
                }
            }

            return "UNKNOWN";
        }
    }
}
