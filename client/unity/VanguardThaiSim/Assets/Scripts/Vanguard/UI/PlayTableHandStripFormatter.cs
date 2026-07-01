using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableHandStripFormatter
    {
        public const string EmptyHandText = "Hand is empty.";
        public const string HiddenCardLabel = "Hidden card";
        public const int DefaultMaxNameCharacters = 18;
        public const float CardButtonWidth = 74f;
        public const float CardButtonHeight = 52f;
        public const float CardThumbnailWidth = 34f;
        public const float CardThumbnailHeight = 44f;

        public static string FormatHeader(int handCount)
        {
            return "Hand (" + handCount + ")";
        }

        public static string FormatCardButtonLabel(
            GameCardInstance card,
            CardDetail detail,
            int displayIndex,
            int maxNameCharacters = DefaultMaxNameCharacters)
        {
            string prefix = "#" + displayIndex;
            if (card == null)
            {
                return prefix + "\nMissing";
            }

            if (IsHidden(card))
            {
                return prefix + "\n" + HiddenCardLabel;
            }

            string cardName = detail == null ? string.Empty : detail.NameTh;
            string displayName = FirstNonEmpty(cardName, card.card_id, "Unknown");
            string grade = detail != null && detail.Grade.HasValue
                ? "G" + detail.Grade.Value
                : "G-";
            string trigger = detail == null ? string.Empty : detail.Trigger;
            string lineOne = prefix + " " + grade;
            if (!string.IsNullOrWhiteSpace(trigger))
            {
                lineOne += " " + trigger;
            }

            return lineOne + "\n" + Truncate(displayName, maxNameCharacters);
        }

        private static bool IsHidden(GameCardInstance card)
        {
            return card == null ||
                   !card.face_up ||
                   card.card_id == GameStateViewFactory.HiddenCardId;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string Truncate(string value, int maxCharacters)
        {
            if (string.IsNullOrEmpty(value) || maxCharacters <= 0 || value.Length <= maxCharacters)
            {
                return value ?? string.Empty;
            }

            if (maxCharacters <= 3)
            {
                return value.Substring(0, maxCharacters);
            }

            return value.Substring(0, maxCharacters - 3) + "...";
        }
    }
}
