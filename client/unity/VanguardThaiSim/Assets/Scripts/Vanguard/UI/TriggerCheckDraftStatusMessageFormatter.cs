using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftStatusMessageFormatter
    {
        public const string SelectionClearedMessage = "Draft selection cleared.";

        public static string FormatTriggerTypeChanged(TriggerType triggerType)
        {
            return "Draft trigger type: " + triggerType + ".";
        }

        public static string FormatCheckSourceChanged(TriggerCheckSource checkSource)
        {
            return "Draft check source: " + checkSource + ".";
        }

        public static string FormatCheckIndexChanged(int checkIndex)
        {
            return "Draft check index: " + checkIndex + ".";
        }

        public static string FormatSelectionCleared()
        {
            return SelectionClearedMessage;
        }
    }
}
