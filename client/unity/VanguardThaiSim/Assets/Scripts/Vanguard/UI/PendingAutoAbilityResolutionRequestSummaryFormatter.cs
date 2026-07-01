using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityResolutionRequestSummaryFormatter
    {
        public const string NoPayloadsMessage = "Pending resolve requests: 0";

        public static string FormatLatest(
            IReadOnlyList<NetworkPendingAutoAbilityResolutionRequestPayload> payloads)
        {
            if (payloads == null || payloads.Count == 0)
            {
                return NoPayloadsMessage;
            }

            NetworkPendingAutoAbilityResolutionRequestPayload latest = payloads[payloads.Count - 1];
            PendingAutoAbilityResolutionRequest request;
            string rejectionReason;
            if (!PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                    latest,
                    out request,
                    out rejectionReason))
            {
                return "Pending resolve requests: " + payloads.Count +
                       "\nLatest: invalid (" + rejectionReason + ")";
            }

            string payloadId = string.IsNullOrWhiteSpace(latest.payload_id) ? "none" : latest.payload_id;
            return "Pending resolve requests: " + payloads.Count +
                   "\nLatest: " + payloadId +
                   "\n" + PendingAutoAbilityResolutionRequestFormatter.Format(request);
        }
    }
}
