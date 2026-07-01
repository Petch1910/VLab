using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionDecisionTypeSelector
    {
        public static string Normalize(string decisionType)
        {
            return PendingAutoAbilityManualResolutionDecisionTypes.IsSupported(decisionType)
                ? decisionType
                : PendingAutoAbilityManualResolutionDecisionTypes.Resolve;
        }

        public static string Next(string decisionType)
        {
            string current = Normalize(decisionType);
            if (current == PendingAutoAbilityManualResolutionDecisionTypes.Resolve)
            {
                return PendingAutoAbilityManualResolutionDecisionTypes.Skip;
            }

            if (current == PendingAutoAbilityManualResolutionDecisionTypes.Skip)
            {
                return PendingAutoAbilityManualResolutionDecisionTypes.Defer;
            }

            return PendingAutoAbilityManualResolutionDecisionTypes.Resolve;
        }

        public static string FormatButtonLabel(string decisionType)
        {
            return "Dec:" + Normalize(decisionType);
        }

        public static string FormatStatusMessage(string decisionType)
        {
            return "Pending auto ability manual resolution decision type: " +
                   Normalize(decisionType) +
                   ".";
        }
    }
}
