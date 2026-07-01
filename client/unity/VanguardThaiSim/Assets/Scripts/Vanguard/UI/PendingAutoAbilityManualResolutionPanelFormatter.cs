using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionPanelFormatter
    {
        public const string NoDecisionsMessage =
            "Manual resolution\nDecisions: 0\nValidation: none\nApply preview: none";

        public static string Format(
            IReadOnlyList<NetworkPendingAutoAbilityManualResolutionDecisionPayload> decisionPayloads,
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry latestApplyPreviewLog)
        {
            if (decisionPayloads == null || decisionPayloads.Count == 0)
            {
                return "Manual resolution" +
                       "\nDecisions: 0" +
                       "\nValidation: none" +
                       "\nApply preview: " + FormatApplyPreview(latestApplyPreviewLog);
            }

            NetworkPendingAutoAbilityManualResolutionDecisionPayload latest =
                decisionPayloads[decisionPayloads.Count - 1];
            PendingAutoAbilityManualResolutionDecision decision;
            string rejectionReason;
            if (!PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    latest,
                    out decision,
                    out rejectionReason))
            {
                return "Manual resolution" +
                       "\nDecisions: " + decisionPayloads.Count +
                       "\nLatest: invalid (" + rejectionReason + ")" +
                       "\nValidation: rejected " + rejectionReason +
                       "\nApply preview: " + FormatApplyPreview(latestApplyPreviewLog);
            }

            PendingAutoAbilityManualResolutionDecisionValidationResult validation =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(latest);
            string payloadId = string.IsNullOrWhiteSpace(latest.payload_id) ? "none" : latest.payload_id;
            return "Manual resolution" +
                   "\nDecisions: " + decisionPayloads.Count +
                   "\nLatest: " + payloadId +
                   "\nDecision: " + FormatDecision(decision) +
                   "\nValidation: " + FormatValidation(validation) +
                   "\nApply preview: " + FormatApplyPreview(latestApplyPreviewLog);
        }

        private static string FormatDecision(PendingAutoAbilityManualResolutionDecision decision)
        {
            if (decision == null)
            {
                return "none";
            }

            string decisionType = string.IsNullOrWhiteSpace(decision.decision_type)
                ? "none"
                : decision.decision_type;
            string pendingId = string.IsNullOrWhiteSpace(decision.pending_id) ? "none" : decision.pending_id;
            string timing = string.IsNullOrWhiteSpace(decision.timing_event) ? "none" : decision.timing_event;
            return decisionType +
                   " #" + (decision.selected_index + 1) +
                   " " + pendingId +
                   " P" + decision.player_index +
                   " " + timing +
                   " source " + FormatSource(decision);
        }

        private static string FormatValidation(
            PendingAutoAbilityManualResolutionDecisionValidationResult validation)
        {
            if (validation == null)
            {
                return "none";
            }

            if (!validation.accepted)
            {
                return "rejected " + (string.IsNullOrWhiteSpace(validation.rejection_reason)
                    ? "UNKNOWN"
                    : validation.rejection_reason);
            }

            return "valid";
        }

        private static string FormatApplyPreview(
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry)
        {
            if (entry == null)
            {
                return "none";
            }

            if (!entry.accepted)
            {
                return "rejected " + (string.IsNullOrWhiteSpace(entry.rejection_reason)
                    ? "UNKNOWN"
                    : entry.rejection_reason);
            }

            string decisionType = string.IsNullOrWhiteSpace(entry.decision_type)
                ? "none"
                : entry.decision_type;
            string pendingId = string.IsNullOrWhiteSpace(entry.pending_id) ? "none" : entry.pending_id;
            return "accepted " + decisionType + " " + pendingId;
        }

        private static string FormatSource(PendingAutoAbilityManualResolutionDecision decision)
        {
            if (decision.hides_source_card_identity ||
                string.Equals(decision.source_card_id, GameStateViewFactory.HiddenCardId, System.StringComparison.Ordinal))
            {
                return "hidden";
            }

            return string.IsNullOrWhiteSpace(decision.source_card_id) ? "none" : decision.source_card_id;
        }
    }
}
