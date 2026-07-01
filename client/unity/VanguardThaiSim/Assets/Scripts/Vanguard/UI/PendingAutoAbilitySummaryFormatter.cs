using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilitySummaryFormatter
    {
        public const string NoPayloadsMessage = "Pending abilities: 0";

        public static string FormatSummary(IReadOnlyList<NetworkPendingAutoAbilityQueuePayload> payloads)
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
                return "Pending abilities: " + payloads.Count + "\nLatest: invalid (" + rejectionReason + ")";
            }

            queue.EnsureLists();
            if (queue.pending.Count == 0)
            {
                return "Pending abilities: " + payloads.Count + "\nLatest: empty";
            }

            string payloadId = string.IsNullOrWhiteSpace(latest.payload_id) ? "none" : latest.payload_id;
            string queueId = string.IsNullOrWhiteSpace(queue.queue_id) ? "none" : queue.queue_id;
            return "Pending abilities: " + payloads.Count +
                   "\nLatest: " + payloadId +
                   "\nQueue: " + queueId +
                   " pending=" + queue.pending.Count;
        }
    }
}
