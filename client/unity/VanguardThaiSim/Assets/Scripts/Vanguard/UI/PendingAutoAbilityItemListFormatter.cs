using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityItemListFormatter
    {
        public const string NoPayloadsMessage = "Pending ability items: 0";
        public const int DefaultMaxItems = 5;

        public static string FormatLatest(
            IReadOnlyList<NetworkPendingAutoAbilityQueuePayload> payloads,
            int maxItems = DefaultMaxItems)
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
                return "Pending ability items: invalid (" + rejectionReason + ")";
            }

            queue.EnsureLists();
            string queueId = string.IsNullOrWhiteSpace(queue.queue_id) ? "none" : queue.queue_id;
            if (queue.pending.Count == 0)
            {
                return "Pending ability items: 0\nQueue: " + queueId;
            }

            int safeMax = maxItems <= 0 ? DefaultMaxItems : maxItems;
            int count = queue.pending.Count;
            int shown = count < safeMax ? count : safeMax;

            var lines = new List<string>
            {
                "Pending ability items: " + count,
                "Queue: " + queueId
            };

            for (int i = 0; i < shown; i++)
            {
                PendingAutoAbility ability = queue.pending[i];
                lines.Add(FormatItem(i + 1, ability));
            }

            int remaining = count - shown;
            if (remaining > 0)
            {
                lines.Add("... +" + remaining + " more");
            }

            return string.Join("\n", lines.ToArray());
        }

        private static string FormatItem(int index, PendingAutoAbility ability)
        {
            if (ability == null)
            {
                return index + ". null";
            }

            string pendingId = string.IsNullOrWhiteSpace(ability.pending_id) ? "none" : ability.pending_id;
            string timingEvent = string.IsNullOrWhiteSpace(ability.timing_event) ? "none" : ability.timing_event;
            string summary = string.IsNullOrWhiteSpace(ability.summary) ? "none" : ability.summary;

            return index + ". " + pendingId +
                   " player=" + ability.player_index +
                   " timing=" + timingEvent +
                   " source=" + FormatSource(ability) +
                   " summary=" + summary;
        }

        private static string FormatSource(PendingAutoAbility ability)
        {
            if (ability.hides_source_card_identity ||
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
