using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftMetadataFormatterTests
    {
        [Test]
        public void SummaryFormatsDefaultMetadata()
        {
            Assert.AreEqual(
                "Unknown / Manual / idx 0",
                TriggerCheckDraftMetadataFormatter.FormatSummary(
                    TriggerType.Unknown,
                    TriggerCheckSource.Manual,
                    0));
        }

        [Test]
        public void SummaryFormatsCycledMetadata()
        {
            Assert.AreEqual(
                "Critical / Drive / idx 1",
                TriggerCheckDraftMetadataFormatter.FormatSummary(
                    TriggerType.Critical,
                    TriggerCheckSource.Drive,
                    1));
        }

        [Test]
        public void ButtonLabelsFormatDeterministically()
        {
            Assert.AreEqual(
                "Type Crit",
                TriggerCheckDraftMetadataFormatter.FormatTypeButtonLabel(TriggerType.Critical));
            Assert.AreEqual(
                "Src Damage",
                TriggerCheckDraftMetadataFormatter.FormatSourceButtonLabel(TriggerCheckSource.Damage));
            Assert.AreEqual(
                "Idx 3",
                TriggerCheckDraftMetadataFormatter.FormatIndexButtonLabel(3));
        }

        [Test]
        public void ShortTriggerTypeLabelsMatchPlayTableLabels()
        {
            Assert.AreEqual("Unknown", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.Unknown));
            Assert.AreEqual("Crit", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.Critical));
            Assert.AreEqual("Draw", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.Draw));
            Assert.AreEqual("Front", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.Front));
            Assert.AreEqual("Heal", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.Heal));
            Assert.AreEqual("Over", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.Over));
            Assert.AreEqual("None", TriggerCheckDraftMetadataFormatter.FormatShortTriggerType(TriggerType.None));
        }
    }
}
