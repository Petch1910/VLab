using System;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public sealed class PendingAutoAbilityManualResolutionDecisionValidationResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityManualResolutionDecision decision;
    }

    public static class PendingAutoAbilityManualResolutionDecisionValidator
    {
        public const string PendingIdMissingReason =
            "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PENDING_ID_MISSING";

        public static PendingAutoAbilityManualResolutionDecisionValidationResult Validate(
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
        {
            PendingAutoAbilityManualResolutionDecision decision;
            string rejectionReason;
            if (!PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    payload,
                    out decision,
                    out rejectionReason))
            {
                return Reject(rejectionReason);
            }

            if (!PendingAutoAbilityManualResolutionDecisionTypes.IsSupported(decision.decision_type))
            {
                return Reject(PendingAutoAbilityManualResolutionDecisionFactory.DecisionTypeInvalidReason);
            }

            if (string.IsNullOrWhiteSpace(decision.pending_id))
            {
                return Reject(PendingIdMissingReason);
            }

            return new PendingAutoAbilityManualResolutionDecisionValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                decision = CloneAndSanitizeDecision(decision)
            };
        }

        private static PendingAutoAbilityManualResolutionDecisionValidationResult Reject(string rejectionReason)
        {
            return new PendingAutoAbilityManualResolutionDecisionValidationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                decision = null
            };
        }

        private static PendingAutoAbilityManualResolutionDecision CloneAndSanitizeDecision(
            PendingAutoAbilityManualResolutionDecision source)
        {
            bool hidesSource =
                source.hides_source_card_identity ||
                string.Equals(source.source_card_id, GameStateViewFactory.HiddenCardId, StringComparison.Ordinal);

            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = source.decision_id ?? string.Empty,
                decision_type = source.decision_type ?? string.Empty,
                selected_index = source.selected_index,
                pending_id = source.pending_id ?? string.Empty,
                player_index = source.player_index,
                timing_event = source.timing_event ?? string.Empty,
                source_card_instance_id = hidesSource ? string.Empty : source.source_card_instance_id ?? string.Empty,
                source_card_id = hidesSource
                    ? GameStateViewFactory.HiddenCardId
                    : source.source_card_id ?? string.Empty,
                hides_source_card_identity = hidesSource,
                reason = source.reason ?? string.Empty,
                summary = source.summary ?? string.Empty
            };
        }
    }
}
