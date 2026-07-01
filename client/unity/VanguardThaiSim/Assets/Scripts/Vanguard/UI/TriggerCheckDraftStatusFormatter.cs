using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftStatusFormatter
    {
        public const int MaxCardLabelLength = 18;

        public static string FormatSelectedStatus(
            string selectedCardId,
            string selectedCardInstanceId,
            GameZone selectedZone)
        {
            return "card " +
                   FormatCardLabel(selectedCardId) +
                   " / zone " +
                   FormatZoneLabel(selectedCardInstanceId, selectedZone);
        }

        public static string FormatCardLabel(string selectedCardId)
        {
            if (string.IsNullOrEmpty(selectedCardId))
            {
                return "none";
            }

            if (selectedCardId == GameStateViewFactory.HiddenCardId)
            {
                return "hidden";
            }

            if (selectedCardId.Length <= MaxCardLabelLength)
            {
                return selectedCardId;
            }

            return selectedCardId.Substring(0, MaxCardLabelLength);
        }

        public static string FormatZoneLabel(string selectedCardInstanceId, GameZone selectedZone)
        {
            if (string.IsNullOrEmpty(selectedCardInstanceId))
            {
                return "none";
            }

            return selectedZone.ToString();
        }
    }
}
