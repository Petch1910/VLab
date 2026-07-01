using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class UiStateMessageFormatterTests
    {
        [Test]
        public void RawLoadingAndEmptyLabelsAreReplaced()
        {
            Assert.AreEqual("Preparing card pool", UiStateMessageFormatter.CardPoolPreparing);
            Assert.AreEqual("Select", UiStateMessageFormatter.FilterPreparing);
            Assert.AreEqual("No cards", UiStateMessageFormatter.ZoneEmpty);
        }

        [Test]
        public void CardPackFailureCompactsDetailsAndShowsExpectedPack()
        {
            string formatted = UiStateMessageFormatter.FormatCardPackLoadFailure(
                "FileNotFound\nmissing manifest",
                "data/packs/vanguard_th");

            Assert.IsTrue(formatted.Contains("FileNotFound missing manifest"));
            Assert.IsTrue(formatted.Contains("Expected pack: data/packs/vanguard_th"));
        }
    }
}
