using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionApplyResultFormatter
    {
        public const string NullResultMessage = "Pending manual decision apply: none";

        public static string Format(PendingAutoAbilityManualResolutionApplyResult result)
        {
            if (result == null)
            {
                return NullResultMessage;
            }

            if (!result.accepted)
            {
                string reason = string.IsNullOrWhiteSpace(result.rejection_reason)
                    ? "UNKNOWN"
                    : result.rejection_reason;
                return "Pending manual decision apply: rejected " + reason;
            }

            string decisionType = string.IsNullOrWhiteSpace(result.decision_type)
                ? "none"
                : result.decision_type;
            string pendingId = string.IsNullOrWhiteSpace(result.pending_id)
                ? "none"
                : result.pending_id;

            return "Pending manual decision apply: accepted type=" + decisionType +
                   " id=" + pendingId;
        }
    }
}
