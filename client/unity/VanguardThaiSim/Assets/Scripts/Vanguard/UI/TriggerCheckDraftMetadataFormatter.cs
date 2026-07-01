using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftMetadataFormatter
    {
        public static string FormatSummary(
            TriggerType triggerType,
            TriggerCheckSource checkSource,
            int checkIndex)
        {
            return triggerType + " / " + checkSource + " / idx " + checkIndex;
        }

        public static string FormatTypeButtonLabel(TriggerType triggerType)
        {
            return "Type " + FormatShortTriggerType(triggerType);
        }

        public static string FormatSourceButtonLabel(TriggerCheckSource checkSource)
        {
            return "Src " + checkSource;
        }

        public static string FormatIndexButtonLabel(int checkIndex)
        {
            return "Idx " + checkIndex;
        }

        public static string FormatShortTriggerType(TriggerType triggerType)
        {
            switch (triggerType)
            {
                case TriggerType.Critical:
                    return "Crit";
                case TriggerType.Draw:
                    return "Draw";
                case TriggerType.Front:
                    return "Front";
                case TriggerType.Heal:
                    return "Heal";
                case TriggerType.Over:
                    return "Over";
                case TriggerType.None:
                    return "None";
                case TriggerType.Unknown:
                default:
                    return "Unknown";
            }
        }
    }
}
