using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionDecisionSummaryFormatter
    {
        public const string NoPayloadsMessage = "Pending manual decisions: 0";

        public static string FormatLatest(
            IReadOnlyList<NetworkPendingAutoAbilityManualResolutionDecisionPayload> payloads)
        {
            if (payloads == null || payloads.Count == 0)
            {
                return NoPayloadsMessage;
            }

            NetworkPendingAutoAbilityManualResolutionDecisionPayload latest = payloads[payloads.Count - 1];
            PendingAutoAbilityManualResolutionDecision decision;
            string rejectionReason;
            if (!PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    latest,
                    out decision,
                    out rejectionReason))
            {
                return "Pending manual decisions: " + payloads.Count +
                       "\nLatest: invalid (" + rejectionReason + ")";
            }

            string payloadId = string.IsNullOrWhiteSpace(latest.payload_id) ? "none" : latest.payload_id;
            return "Pending manual decisions: " + payloads.Count +
                   "\nLatest: " + payloadId +
                   "\n" + Format(decision);
        }

        public static string Format(PendingAutoAbilityManualResolutionDecision decision)
        {
            if (decision == null)
            {
                return "Pending manual decision: none";
            }

            string decisionType = string.IsNullOrWhiteSpace(decision.decision_type)
                ? "none"
                : decision.decision_type;
            string pendingId = string.IsNullOrWhiteSpace(decision.pending_id) ? "none" : decision.pending_id;
            string timingEvent = string.IsNullOrWhiteSpace(decision.timing_event)
                ? "none"
                : decision.timing_event;

            return "Pending manual decision: type=" + decisionType +
                   " index=" + decision.selected_index +
                   " id=" + pendingId +
                   " player=" + decision.player_index +
                   " timing=" + timingEvent +
                   " source=" + FormatSource(decision);
        }

        private static string FormatSource(PendingAutoAbilityManualResolutionDecision decision)
        {
            if (decision.hides_source_card_identity ||
                string.Equals(decision.source_card_id, GameStateViewFactory.HiddenCardId, System.StringComparison.Ordinal))
            {
                return "hidden";
            }

            string cardId = string.IsNullOrWhiteSpace(decision.source_card_id) ? "none" : decision.source_card_id;
            string instanceId =
                string.IsNullOrWhiteSpace(decision.source_card_instance_id) ? "none" : decision.source_card_instance_id;
            return cardId + "@" + instanceId;
        }
    }
}
