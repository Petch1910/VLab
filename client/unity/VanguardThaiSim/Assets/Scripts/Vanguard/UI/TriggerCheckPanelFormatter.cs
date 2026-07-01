using System.Collections.Generic;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckPanelFormatter
    {
        public const string LocalNoLogsMessage =
            "Trigger panel\nLogs: 0\nDraft: local mode";

        public static string Format(
            IReadOnlyList<NetworkTriggerCheckReplayLogPayload> payloads,
            bool isOnline,
            TriggerType triggerType,
            TriggerCheckSource checkSource,
            int checkIndex,
            string selectedCardId,
            string selectedCardInstanceId,
            GameZone selectedZone)
        {
            string logSummary = FormatLogSummary(payloads);
            if (!isOnline)
            {
                return "Trigger panel\n" + logSummary + "\nDraft: local mode";
            }

            return "Trigger panel" +
                   "\n" + logSummary +
                   "\nDraft: " + TriggerCheckDraftMetadataFormatter.FormatSummary(triggerType, checkSource, checkIndex) +
                   "\nSelected: " + TriggerCheckDraftStatusFormatter.FormatSelectedStatus(
                       selectedCardId,
                       selectedCardInstanceId,
                       selectedZone);
        }

        private static string FormatLogSummary(IReadOnlyList<NetworkTriggerCheckReplayLogPayload> payloads)
        {
            if (payloads == null || payloads.Count == 0)
            {
                return "Logs: 0";
            }

            NetworkTriggerCheckReplayLogPayload latest = payloads[payloads.Count - 1];
            TriggerCheckReplayLog log;
            string rejectionReason;
            if (!TriggerCheckReplayLogPayloadCodec.TryDecode(latest, out log, out rejectionReason))
            {
                return "Logs: " + payloads.Count + "\nLatest: invalid (" + rejectionReason + ")";
            }

            log.EnsureLists();
            if (log.entries.Count == 0)
            {
                return "Logs: " + payloads.Count + "\nLatest: empty";
            }

            return "Logs: " + payloads.Count + "\nLatest: " +
                   FormatLatestEntry(log.entries[log.entries.Count - 1]);
        }

        private static string FormatLatestEntry(TriggerCheckLogEntry entry)
        {
            if (entry == null)
            {
                return "empty entry";
            }

            string status = entry.accepted
                ? "accepted"
                : (entry.needs_manual_resolution ? "manual" : "rejected");
            return entry.check_source +
                   " #" + entry.check_index +
                   " " + entry.trigger_type +
                   " " + status +
                   " mods=" + entry.modifier_count;
        }
    }
}
