using System;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityManualResolutionApplyCommandValidator
    {
        public static PendingAutoAbilityManualResolutionApplyResult Validate(
            PendingAutoAbilityQueue queue,
            PendingAutoAbilityManualResolutionDecision decision)
        {
            if (queue == null || queue.pending == null || queue.pending.Count == 0 || queue.pending[0] == null)
            {
                return PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing);
            }

            if (decision == null)
            {
                return PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing);
            }

            if (!PendingAutoAbilityManualResolutionDecisionTypes.IsSupported(decision.decision_type))
            {
                return PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionTypeInvalid);
            }

            PendingAutoAbility pending = queue.pending[0];
            if (!string.Equals(pending.pending_id ?? string.Empty, decision.pending_id ?? string.Empty, StringComparison.Ordinal))
            {
                return PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch);
            }

            return PendingAutoAbilityManualResolutionApplyResult.Accepted(
                queue.queue_id ?? string.Empty,
                pending.pending_id ?? string.Empty,
                decision.decision_type ?? string.Empty,
                "Validated pending auto ability manual resolution decision " +
                decision.decision_type +
                " for " +
                (pending.pending_id ?? string.Empty) +
                ".");
        }
    }
}
