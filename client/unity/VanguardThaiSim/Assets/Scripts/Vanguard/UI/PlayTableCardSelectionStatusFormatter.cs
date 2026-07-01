using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableCardSelectionStatusFormatter
    {
        public const string SelectCardFirstMessage = "Select a card first.";
        public const string NoCardSelectedMessage = "No card selected.";

        public static string FormatSelectCardFirst()
        {
            return SelectCardFirstMessage;
        }

        public static string FormatSelectedCard(string cardId, GameZone zone)
        {
            return "Selected " + (cardId ?? string.Empty) + " from " + zone + ".";
        }

        public static string FormatSelectedTarget(string cardId, GameZone zone)
        {
            return "Target " + (cardId ?? string.Empty) + " from opponent " + zone + ".";
        }

        public static string FormatNoCardSelected()
        {
            return NoCardSelectedMessage;
        }
    }
}
