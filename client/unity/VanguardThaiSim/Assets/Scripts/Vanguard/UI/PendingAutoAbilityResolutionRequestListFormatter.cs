using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityResolutionRequestListFormatter
    {
        public const string NoPayloadsMessage = "Pending resolve request list: 0";
        public const int DefaultMaxItems = 5;

        public static string Format(
            IReadOnlyList<NetworkPendingAutoAbilityResolutionRequestPayload> payloads,
            int maxItems = DefaultMaxItems)
        {
            if (payloads == null || payloads.Count == 0)
            {
                return NoPayloadsMessage;
            }

            int safeMax = maxItems <= 0 ? DefaultMaxItems : maxItems;
            int shown = payloads.Count < safeMax ? payloads.Count : safeMax;
            var lines = new List<string>
            {
                "Pending resolve request list: " + payloads.Count
            };

            for (int i = 0; i < shown; i++)
            {
                int payloadIndex = payloads.Count - 1 - i;
                lines.Add(FormatPayload(i + 1, payloads[payloadIndex]));
            }

            int remaining = payloads.Count - shown;
            if (remaining > 0)
            {
                lines.Add("... +" + remaining + " older");
            }

            return string.Join("\n", lines.ToArray());
        }

        private static string FormatPayload(
            int displayIndex,
            NetworkPendingAutoAbilityResolutionRequestPayload payload)
        {
            if (payload == null)
            {
                return displayIndex + ". null";
            }

            PendingAutoAbilityResolutionRequest request;
            string rejectionReason;
            string payloadId = string.IsNullOrWhiteSpace(payload.payload_id) ? "none" : payload.payload_id;
            if (!PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                    payload,
                    out request,
                    out rejectionReason))
            {
                return displayIndex + ". payload=" + payloadId + " invalid (" + rejectionReason + ")";
            }

            return displayIndex + ". payload=" + payloadId + " " +
                   PendingAutoAbilityResolutionRequestFormatter.Format(request);
        }
    }
}
