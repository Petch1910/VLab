using System.Text;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PlayTableReplaySyncStatusFormatter
    {
        public static string FormatOnlineStatus(
            int onlineEventCursor,
            int publicReplayEventCount,
            int lastReconnectAppliedCount,
            int lastReconnectFromEventIndex)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Replay Sync");
            builder.AppendLine("Online cursor: " + ClampNonNegative(onlineEventCursor));
            builder.AppendLine("Public replay events: " + ClampNonNegative(publicReplayEventCount));
            builder.AppendLine("Status: " + FormatCursorStatus(onlineEventCursor, publicReplayEventCount));
            builder.Append("Reconnect: ");
            if (lastReconnectAppliedCount > 0)
            {
                builder.Append("applied ");
                builder.Append(lastReconnectAppliedCount);
                builder.Append(" from event ");
                builder.Append(lastReconnectFromEventIndex < 0 ? 0 : lastReconnectFromEventIndex);
            }
            else
            {
                builder.Append("none applied");
            }

            return builder.ToString();
        }

        public static string FormatReconnectApplyResult(NetworkPublicReconnectApplyResult result)
        {
            if (result == null)
            {
                return "Replay sync: no reconnect batch applied.";
            }

            if (result.accepted)
            {
                return "Replay sync: applied " + result.applied_count + " public events.";
            }

            return "Replay sync blocked: " + FormatRejectionReason(result.rejection_reason);
        }

        private static string FormatCursorStatus(int onlineEventCursor, int publicReplayEventCount)
        {
            int online = ClampNonNegative(onlineEventCursor);
            int replay = ClampNonNegative(publicReplayEventCount);
            if (online == replay)
            {
                return "synced";
            }

            if (replay < online)
            {
                return "public replay behind by " + (online - replay) + " event(s)";
            }

            return "public replay ahead by " + (replay - online) + " event(s)";
        }

        private static string FormatRejectionReason(string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                return "unknown reconnect issue.";
            }

            if (rejectionReason == "PUBLIC_REPLAY_CURSOR_MISMATCH")
            {
                return "batch cursor does not match the current replay cursor.";
            }

            if (rejectionReason == "PUBLIC_REPLAY_NOT_AT_CURSOR")
            {
                return "replay must be at the current cursor before applying a batch.";
            }

            if (rejectionReason == "PROTOCOL_VERSION_MISMATCH")
            {
                return "batch protocol version does not match this client.";
            }

            return rejectionReason;
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
