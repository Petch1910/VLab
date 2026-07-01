using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionDecisionDraftResultFormatter
    {
        public const string SuccessMessage =
            "Created pending auto ability manual resolution decision draft.";
        public const string NullResultMessage =
            "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_DRAFT_RESULT_MISSING";

        public static string Format(PendingAutoAbilityManualResolutionDecisionDraftResult result)
        {
            if (result == null)
            {
                return NullResultMessage;
            }

            if (result.accepted)
            {
                return SuccessMessage;
            }

            return string.IsNullOrWhiteSpace(result.rejection_reason)
                ? NullResultMessage
                : result.rejection_reason;
        }
    }
}
