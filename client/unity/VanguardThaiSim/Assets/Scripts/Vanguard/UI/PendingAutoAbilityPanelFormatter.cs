using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityPanelFormatter
    {
        public const string NoPayloadsMessage =
            "AUTO queue\nPending: 0\nLatest: none\nSelection: none";

        public static string Format(
            IReadOnlyList<NetworkPendingAutoAbilityQueuePayload> payloads,
            PendingAutoAbilitySelectionState selectionState)
        {
            if (payloads == null || payloads.Count == 0)
            {
                return NoPayloadsMessage;
            }

            NetworkPendingAutoAbilityQueuePayload latest = payloads[payloads.Count - 1];
            PendingAutoAbilityQueue queue;
            string rejectionReason;
            if (!PendingAutoAbilityQueuePayloadCodec.TryDecode(latest, out queue, out rejectionReason))
            {
                return "AUTO queue" +
                       "\nPayloads: " + payloads.Count +
                       "\nLatest: invalid (" + rejectionReason + ")" +
                       "\nSelection: " + FormatSelection(selectionState);
            }

            queue.EnsureLists();
            string payloadId = string.IsNullOrWhiteSpace(latest.payload_id) ? "none" : latest.payload_id;
            string queueId = string.IsNullOrWhiteSpace(queue.queue_id) ? "none" : queue.queue_id;
            return "AUTO queue" +
                   "\nPayloads: " + payloads.Count +
                   "\nLatest: " + payloadId +
                   "\nQueue: " + queueId +
                   "\nPending: " + queue.pending.Count +
                   "\nSelection: " + FormatSelection(selectionState);
        }

        private static string FormatSelection(PendingAutoAbilitySelectionState state)
        {
            if (state == null || !state.has_selection || state.selected_ability == null)
            {
                return "none";
            }

            if (!state.accepted)
            {
                return "rejected " + (state.rejection_reason ?? string.Empty);
            }

            PendingAutoAbility ability = state.selected_ability;
            string pendingId = string.IsNullOrWhiteSpace(ability.pending_id) ? "none" : ability.pending_id;
            string timing = string.IsNullOrWhiteSpace(ability.timing_event) ? "none" : ability.timing_event;
            return "#" + (state.selected_index + 1) +
                   " " + pendingId +
                   " (P" + ability.player_index +
                   " " + timing +
                   " source " + FormatSource(ability) + ")";
        }

        private static string FormatSource(PendingAutoAbility ability)
        {
            if (ability == null ||
                ability.hides_source_card_identity ||
                string.Equals(ability.source_card_id, GameStateViewFactory.HiddenCardId, System.StringComparison.Ordinal))
            {
                return "hidden";
            }

            string cardId = string.IsNullOrWhiteSpace(ability.source_card_id) ? "none" : ability.source_card_id;
            string instanceId =
                string.IsNullOrWhiteSpace(ability.source_card_instance_id) ? "none" : ability.source_card_instance_id;
            return cardId + "@" + instanceId;
        }
    }
}
