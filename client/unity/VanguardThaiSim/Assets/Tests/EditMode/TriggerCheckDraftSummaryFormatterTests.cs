using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftSummaryFormatterTests
    {
        [Test]
        public void SummaryFormatsLocalMode()
        {
            Assert.AreEqual(
                "Draft: local mode",
                TriggerCheckDraftSummaryFormatter.FormatSummary(
                    false,
                    TriggerType.Critical,
                    TriggerCheckSource.Drive,
                    2,
                    "BT01-001TH",
                    "instance-1",
                    GameZone.Hand));
        }

        [Test]
        public void SummaryFormatsOnlineDefault()
        {
            Assert.AreEqual(
                "Draft: Unknown / Manual / idx 0 / card none / zone none",
                TriggerCheckDraftSummaryFormatter.FormatSummary(
                    true,
                    TriggerType.Unknown,
                    TriggerCheckSource.Manual,
                    0,
                    null,
                    null,
                    GameZone.Hand));
        }

        [Test]
        public void SummaryFormatsOnlineSelectedCardStatus()
        {
            Assert.AreEqual(
                "Draft: Unknown / Manual / idx 0 / card BT01-001TH / zone Hand",
                TriggerCheckDraftSummaryFormatter.FormatSummary(
                    true,
                    TriggerType.Unknown,
                    TriggerCheckSource.Manual,
                    0,
                    "BT01-001TH",
                    "instance-1",
                    GameZone.Hand));
        }

        [Test]
        public void SummaryFormatsOnlineMetadataUpdates()
        {
            Assert.AreEqual(
                "Draft: Critical / Drive / idx 1 / card none / zone none",
                TriggerCheckDraftSummaryFormatter.FormatSummary(
                    true,
                    TriggerType.Critical,
                    TriggerCheckSource.Drive,
                    1,
                    string.Empty,
                    string.Empty,
                    GameZone.Damage));
        }
    }
}
