namespace VanguardThaiSim.UI
{
    public static class PlayTableModeSummaryFormatter
    {
        public const string LocalModeMessage = "Local";
        private const int DetailMaxLength = 96;

        public static string Format(
            bool isOnline,
            object status,
            object transportName,
            int eventCursor,
            int triggerCheckReplayLogCount,
            int lastReconnectAppliedCount,
            int lastReconnectFromEventIndex)
        {
            if (!isOnline)
            {
                return LocalModeMessage;
            }

            string summary = "Online" +
                             " | Status: " +
                             FormatNullable(status) +
                             " | Transport: " +
                             FormatNullable(transportName) +
                             " | Cursor: " +
                             eventCursor;
            return summary;
        }

        public static string FormatLocal(string detail)
        {
            if (string.IsNullOrWhiteSpace(detail))
            {
                return LocalModeMessage;
            }

            return LocalModeMessage + " | " + TrimDetail(detail);
        }

        public static string FormatAdvancedDetails(
            bool isOnline,
            int eventCursor,
            int triggerCheckReplayLogCount,
            int lastReconnectAppliedCount,
            int lastReconnectFromEventIndex)
        {
            if (!isOnline)
            {
                return "Online debug: local table.";
            }

            string summary = "Online debug" +
                             "\nEvent cursor: " +
                             eventCursor +
                             "\nTrigger logs: " +
                             triggerCheckReplayLogCount;
            if (lastReconnectAppliedCount > 0)
            {
                summary += "\nReconnect: +" +
                           lastReconnectAppliedCount +
                           " from " +
                           lastReconnectFromEventIndex;
            }
            else
            {
                summary += "\nReconnect: none applied";
            }

            return summary;
        }

        private static string FormatNullable(object value)
        {
            string text = value == null ? string.Empty : value.ToString();
            return string.IsNullOrWhiteSpace(text) ? "unknown" : text;
        }

        private static string TrimDetail(string value)
        {
            string compact = string.Join(
                " ",
                value.Trim().Split(
                    new[] { ' ', '\r', '\n', '\t' },
                    System.StringSplitOptions.RemoveEmptyEntries));
            if (compact.Length <= DetailMaxLength)
            {
                return compact;
            }

            return compact.Substring(0, DetailMaxLength - 3) + "...";
        }
    }
}
