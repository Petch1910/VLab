using System;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionDecisionValidationResultFormatter
    {
        public const string NullResultMessage = "Pending manual decision validation: none";

        public static string Format(PendingAutoAbilityManualResolutionDecisionValidationResult result)
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
                return "Pending manual decision validation: rejected " + reason;
            }

            PendingAutoAbilityManualResolutionDecision decision = result.decision;
            if (decision == null)
            {
                return NullResultMessage;
            }

            string decisionType = string.IsNullOrWhiteSpace(decision.decision_type)
                ? "none"
                : decision.decision_type;
            string pendingId = string.IsNullOrWhiteSpace(decision.pending_id)
                ? "none"
                : decision.pending_id;

            return "Pending manual decision validation: valid type=" + decisionType +
                   " id=" + pendingId +
                   " source=" + FormatSource(decision);
        }

        private static string FormatSource(PendingAutoAbilityManualResolutionDecision decision)
        {
            if (decision.hides_source_card_identity ||
                string.Equals(decision.source_card_id, GameStateViewFactory.HiddenCardId, StringComparison.Ordinal))
            {
                return "hidden";
            }

            string cardId = string.IsNullOrWhiteSpace(decision.source_card_id)
                ? "none"
                : decision.source_card_id;
            string instanceId = string.IsNullOrWhiteSpace(decision.source_card_instance_id)
                ? "none"
                : decision.source_card_instance_id;
            return cardId + "@" + instanceId;
        }
    }
}
