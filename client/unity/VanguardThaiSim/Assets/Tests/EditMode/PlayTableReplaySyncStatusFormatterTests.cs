using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableReplaySyncStatusFormatterTests
    {
        [Test]
        public void OnlineStatusShowsSyncedCursor()
        {
            string formatted = PlayTableReplaySyncStatusFormatter.FormatOnlineStatus(3, 3, 0, -1);

            Assert.IsTrue(formatted.Contains("Replay Sync"));
            Assert.IsTrue(formatted.Contains("Online cursor: 3"));
            Assert.IsTrue(formatted.Contains("Public replay events: 3"));
            Assert.IsTrue(formatted.Contains("Status: synced"));
            Assert.IsTrue(formatted.Contains("Reconnect: none applied"));
        }

        [Test]
        public void OnlineStatusShowsBehindAndAheadStates()
        {
            string behind = PlayTableReplaySyncStatusFormatter.FormatOnlineStatus(5, 2, 0, -1);
            string ahead = PlayTableReplaySyncStatusFormatter.FormatOnlineStatus(2, 5, 0, -1);

            Assert.IsTrue(behind.Contains("behind by 3"));
            Assert.IsTrue(ahead.Contains("ahead by 3"));
        }

        [Test]
        public void OnlineStatusShowsReconnectApplied()
        {
            string formatted = PlayTableReplaySyncStatusFormatter.FormatOnlineStatus(5, 5, 2, 3);

            Assert.IsTrue(formatted.Contains("Reconnect: applied 2 from event 3"));
        }

        [Test]
        public void ReconnectApplyResultFormatsAcceptedAndRejected()
        {
            string accepted = PlayTableReplaySyncStatusFormatter.FormatReconnectApplyResult(
                NetworkPublicReconnectApplyResult.Accepted(4));
            string rejected = PlayTableReplaySyncStatusFormatter.FormatReconnectApplyResult(
                NetworkPublicReconnectApplyResult.Rejected("PUBLIC_REPLAY_CURSOR_MISMATCH"));

            Assert.IsTrue(accepted.Contains("applied 4 public events"));
            Assert.IsTrue(rejected.Contains("batch cursor does not match"));
            Assert.IsFalse(rejected.Contains("payload"));
        }
    }
}
