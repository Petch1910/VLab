using System;
using System.Text;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public static class DeckAccessoriesDialogFormatter
    {
        private static readonly string[] FormatOptions = { "D", "V", "Premium" };
        private static readonly string[] SleeveOptions = { "default", "blue", "red", "black" };
        private static readonly string[] CardBackOptions = { "default", "standard", "dark" };
        private static readonly string[] PlaymatOptions = { "default", "table-grid", "plain-dark" };
        private static readonly string[] CrestOptions = { "default", "royal-paladin", "dragon-empire" };
        private static readonly string[] PersonaShieldOptions = { "default", "persona-standard", "persona-dark" };
        private static readonly string[] GiftMarkerOptions = { "default", "force", "accel", "protect" };
        private static readonly string[] QuickShieldOptions = { "default", "quick-shield", "quick-shield-dark" };

        public static string FormatSummary(VanguardDeck deck)
        {
            if (deck == null)
            {
                return "Deck accessories: no deck loaded.";
            }

            DeckAppearanceMetadata appearance = DeckAppearanceMetadata.Normalize(deck.appearance);
            StringBuilder builder = new StringBuilder();
            builder.Append("Deck: ");
            builder.Append(string.IsNullOrWhiteSpace(deck.name) ? "Untitled Deck" : Compact(deck.name, 40));
            builder.Append('\n');
            builder.Append("Format: ");
            builder.Append(string.IsNullOrWhiteSpace(deck.format) ? "D" : deck.format.Trim());
            builder.Append('\n');
            builder.Append("Sleeve: ");
            builder.Append(appearance.sleeve_key);
            builder.Append(" / Back: ");
            builder.Append(appearance.card_back_key);
            builder.Append('\n');
            builder.Append("Playmat: ");
            builder.Append(appearance.playmat_key);
            builder.Append(" / Crest: ");
            builder.Append(appearance.crest_key);
            builder.Append('\n');
            builder.Append("Persona: ");
            builder.Append(appearance.persona_shield_key);
            builder.Append(" / Gift: ");
            builder.Append(appearance.gift_marker_key);
            builder.Append(" / Quick: ");
            builder.Append(appearance.quick_shield_key);
            return builder.ToString();
        }

        public static string NextFormat(string current)
        {
            return NextOption(current, FormatOptions);
        }

        public static string NextSleeve(string current)
        {
            return NextOption(current, SleeveOptions);
        }

        public static string NextCardBack(string current)
        {
            return NextOption(current, CardBackOptions);
        }

        public static string NextPlaymat(string current)
        {
            return NextOption(current, PlaymatOptions);
        }

        public static string NextCrest(string current)
        {
            return NextOption(current, CrestOptions);
        }

        public static string NextPersonaShield(string current)
        {
            return NextOption(current, PersonaShieldOptions);
        }

        public static string NextGiftMarker(string current)
        {
            return NextOption(current, GiftMarkerOptions);
        }

        public static string NextQuickShield(string current)
        {
            return NextOption(current, QuickShieldOptions);
        }

        private static string NextOption(string current, string[] options)
        {
            if (options == null || options.Length == 0)
            {
                return DeckAppearanceMetadata.DefaultKey;
            }

            string normalized = string.IsNullOrWhiteSpace(current) ? options[0] : current.Trim();
            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i], normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return options[(i + 1) % options.Length];
                }
            }

            return options[0];
        }

        private static string Compact(string value, int maxLength)
        {
            string compact = string.Join(
                " ",
                value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            if (compact.Length <= maxLength)
            {
                return compact;
            }

            return compact.Substring(0, maxLength - 3) + "...";
        }
    }
}
