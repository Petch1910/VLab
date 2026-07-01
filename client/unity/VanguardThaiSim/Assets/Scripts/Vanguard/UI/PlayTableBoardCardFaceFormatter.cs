using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableBoardCardFaceFormatter
    {
        public const string HiddenCardTitle = "Hidden card";
        public const string MissingCardTitle = "Unknown card";

        public static string FormatTitle(GameCardInstance card, CardDetail detail)
        {
            if (card == null)
            {
                return MissingCardTitle;
            }

            if (!card.face_up || card.card_id == GameStateViewFactory.HiddenCardId)
            {
                return HiddenCardTitle;
            }

            if (detail != null && !string.IsNullOrWhiteSpace(detail.NameTh))
            {
                return detail.NameTh.Trim();
            }

            if (detail != null && !string.IsNullOrWhiteSpace(detail.CardId))
            {
                return detail.CardId.Trim();
            }

            return string.IsNullOrWhiteSpace(card.card_id) ? MissingCardTitle : card.card_id.Trim();
        }

        public static string FormatStats(CardDetail detail)
        {
            if (detail == null)
            {
                return string.Empty;
            }

            string grade = detail.Grade >= 0 ? "G" + detail.Grade : "G?";
            string power = detail.Power > 0 ? "P" + detail.Power : "P?";
            string shield = detail.Shield > 0 ? "S" + detail.Shield : string.Empty;
            string trigger = string.IsNullOrWhiteSpace(detail.Trigger) ? string.Empty : detail.Trigger.Trim();

            string stats = grade + " " + power;
            if (!string.IsNullOrEmpty(shield))
            {
                stats += " " + shield;
            }

            if (!string.IsNullOrEmpty(trigger))
            {
                stats += " | " + trigger;
            }

            return stats;
        }

        public static bool ShouldShowThumbnail(GameCardInstance card, CardDetail detail)
        {
            return card != null &&
                   card.face_up &&
                   card.card_id != GameStateViewFactory.HiddenCardId &&
                   detail != null &&
                   detail.ImageExists &&
                   !string.IsNullOrWhiteSpace(detail.ImageRelativePath);
        }
    }
}
