using System;
using System.Collections.Generic;
using System.Text;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableEventReplayPanelFormatter
    {
        public const string EmptyPanelMessage =
            "Match log\nEvents: 0\nLatest: none\nRecent: none";
        public const string EmptyCompactPanelMessage =
            "Match log\nEvents: 0\nLatest: none\nRecent: none\nFull log: Advanced";
        public const int DefaultMaxEntries = 8;
        public const int CompactMaxEntries = 3;

        public static string Format(IReadOnlyList<GameEvent> eventLog, int maxEntries = DefaultMaxEntries)
        {
            return FormatInternal(eventLog, maxEntries, false);
        }

        public static string FormatCompact(IReadOnlyList<GameEvent> eventLog)
        {
            return FormatInternal(eventLog, CompactMaxEntries, true);
        }

        private static string FormatInternal(IReadOnlyList<GameEvent> eventLog, int maxEntries, bool compact)
        {
            if (eventLog == null || eventLog.Count == 0)
            {
                return compact ? EmptyCompactPanelMessage : EmptyPanelMessage;
            }

            int safeMax = Math.Max(0, maxEntries);
            int start = Math.Max(0, eventLog.Count - safeMax);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Match log");
            builder.AppendLine("Events: " + eventLog.Count);
            builder.AppendLine("Latest: " + PlayTableEventLogFormatter.FormatEventLine(
                eventLog[eventLog.Count - 1],
                eventLog.Count - 1));
            builder.AppendLine("Recent:");
            for (int i = eventLog.Count - 1; i >= start; i--)
            {
                builder.AppendLine(PlayTableEventLogFormatter.FormatEventLine(eventLog[i], i));
            }

            if (compact)
            {
                builder.AppendLine("Full log: Advanced");
            }

            return builder.ToString();
        }
    }
}
