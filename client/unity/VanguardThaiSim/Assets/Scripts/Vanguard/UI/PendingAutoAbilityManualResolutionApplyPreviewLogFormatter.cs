using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityManualResolutionApplyPreviewLogFormatter
    {
        public const string NullEntryMessage = "Pending manual decision apply preview log: none";
        public const string NoEntriesMessage = "Pending manual decision apply preview log: 0";
        public const int DefaultMaxItems = 5;

        public static string Format(PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry)
        {
            if (entry == null)
            {
                return NullEntryMessage;
            }

            return "Pending manual decision apply preview log: " + FormatEntrySummary(entry);
        }

        public static string FormatList(
            IReadOnlyList<PendingAutoAbilityManualResolutionApplyPreviewLogEntry> entries,
            int maxItems = DefaultMaxItems)
        {
            if (entries == null || entries.Count == 0)
            {
                return NoEntriesMessage;
            }

            int safeMax = maxItems <= 0 ? DefaultMaxItems : maxItems;
            int shown = entries.Count < safeMax ? entries.Count : safeMax;
            var lines = new List<string>
            {
                "Pending manual decision apply preview log: " + entries.Count
            };

            for (int i = 0; i < shown; i++)
            {
                int entryIndex = entries.Count - 1 - i;
                lines.Add((i + 1) + ". " + FormatEntrySummary(entries[entryIndex]));
            }

            int remaining = entries.Count - shown;
            if (remaining > 0)
            {
                lines.Add("... +" + remaining + " older");
            }

            return string.Join("\n", lines.ToArray());
        }

        private static string FormatEntrySummary(PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry)
        {
            if (entry == null)
            {
                return "null";
            }

            if (!entry.accepted)
            {
                string reason = string.IsNullOrWhiteSpace(entry.rejection_reason)
                    ? "UNKNOWN"
                    : entry.rejection_reason;
                return "rejected " + reason;
            }

            string decisionType = string.IsNullOrWhiteSpace(entry.decision_type)
                ? "none"
                : entry.decision_type;
            string pendingId = string.IsNullOrWhiteSpace(entry.pending_id)
                ? "none"
                : entry.pending_id;

            return "accepted type=" + decisionType + " id=" + pendingId;
        }
    }
}
