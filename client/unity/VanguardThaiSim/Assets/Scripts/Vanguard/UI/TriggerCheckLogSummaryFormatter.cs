using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckLogSummaryFormatter
    {
        public const string NoLogsMessage = "Trigger logs: 0";

        public static string FormatSummary(IReadOnlyList<NetworkTriggerCheckReplayLogPayload> payloads)
        {
            if (payloads == null || payloads.Count == 0)
            {
                return NoLogsMessage;
            }

            NetworkTriggerCheckReplayLogPayload latest = payloads[payloads.Count - 1];
            TriggerCheckReplayLog log;
            string rejectionReason;
            if (!TriggerCheckReplayLogPayloadCodec.TryDecode(latest, out log, out rejectionReason))
            {
                return "Trigger logs: " + payloads.Count + "\nLatest: invalid (" + rejectionReason + ")";
            }

            log.EnsureLists();
            if (log.entries.Count == 0)
            {
                return "Trigger logs: " + payloads.Count + "\nLatest: empty";
            }

            TriggerCheckLogEntry entry = log.entries[log.entries.Count - 1];
            string latestSummary = entry == null ? "empty entry" : entry.summary;
            if (string.IsNullOrWhiteSpace(latestSummary) && entry != null)
            {
                latestSummary = entry.check_source + " check " + entry.check_index + " " + entry.trigger_type;
            }

            return "Trigger logs: " + payloads.Count + "\nLatest: " + latestSummary;
        }
    }
}
