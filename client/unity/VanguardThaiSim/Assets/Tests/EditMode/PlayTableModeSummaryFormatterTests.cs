using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableModeSummaryFormatterTests
    {
        [Test]
        public void LocalModeFormatsExistingText()
        {
            Assert.AreEqual(
                PlayTableModeSummaryFormatter.LocalModeMessage,
                PlayTableModeSummaryFormatter.Format(false, "Connected", "Photon", 9, 3, 2, 7));
        }

        [Test]
        public void LocalModeCanShowSoloPracticeDetail()
        {
            string formatted = PlayTableModeSummaryFormatter.FormatLocal(
                "Solo Practice | Easy - guided legal actions | Bot deck: Mirror player deck");

            Assert.IsTrue(formatted.StartsWith("Local | Solo Practice"));
            Assert.IsTrue(formatted.Contains("Bot deck"));
        }

        [Test]
        public void LocalModeDetailIsTrimmed()
        {
            string formatted = PlayTableModeSummaryFormatter.FormatLocal(
                "Solo Practice " + new string('x', 160));

            Assert.LessOrEqual(formatted.Length, PlayTableModeSummaryFormatter.LocalModeMessage.Length + 3 + 96);
            Assert.IsTrue(formatted.EndsWith("..."));
        }

        [Test]
        public void OnlineModeFormatsStatusTransportAndCursorWithoutDebugCounts()
        {
            Assert.AreEqual(
                "Online | Status: Connected | Transport: Photon | Cursor: 12",
                PlayTableModeSummaryFormatter.Format(true, "Connected", "Photon", 12, 4, 0, -1));
        }

        [Test]
        public void OnlineModeDoesNotAppendReconnectSyncToPrimarySummary()
        {
            Assert.AreEqual(
                "Online | Status: Connected | Transport: Photon | Cursor: 12",
                PlayTableModeSummaryFormatter.Format(true, "Connected", "Photon", 12, 4, 3, 8));
        }

        [Test]
        public void OnlineModeFormatsStatusEnumLikePreviousStringConcatenation()
        {
            Assert.AreEqual(
                "Online | Status: InRoom | Transport: Photon | Cursor: 12",
                PlayTableModeSummaryFormatter.Format(
                    true,
                    MultiplayerTransportStatus.InRoom,
                    "Photon",
                    12,
                    4,
                    0,
                    -1));
        }

        [Test]
        public void NullStatusAndTransportPreserveStringConcatenation()
        {
            Assert.AreEqual(
                "Online | Status: unknown | Transport: unknown | Cursor: 0",
                PlayTableModeSummaryFormatter.Format(true, null, null, 0, 0, 0, 0));
        }

        [Test]
        public void AdvancedDetailsShowDebugCountersAwayFromPrimarySummary()
        {
            string formatted = PlayTableModeSummaryFormatter.FormatAdvancedDetails(true, 12, 4, 3, 8);

            Assert.IsTrue(formatted.Contains("Online debug"));
            Assert.IsTrue(formatted.Contains("Event cursor: 12"));
            Assert.IsTrue(formatted.Contains("Trigger logs: 4"));
            Assert.IsTrue(formatted.Contains("Reconnect: +3 from 8"));
        }
    }
}
