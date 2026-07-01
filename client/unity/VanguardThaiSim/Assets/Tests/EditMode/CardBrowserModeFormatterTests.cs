using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class CardBrowserModeFormatterTests
    {
        [Test]
        public void DeckBuilderModeUsesDedicatedLayoutLanguage()
        {
            Assert.AreEqual("Deck Builder", CardBrowserModeFormatter.FormatTitle(CardBrowserScreenMode.DeckBuilder));
            string summary = CardBrowserModeFormatter.FormatLayoutSummary(CardBrowserScreenMode.DeckBuilder);
            Assert.IsTrue(summary.Contains("left preview"));
            Assert.IsTrue(summary.Contains("center card grid"));
            Assert.IsTrue(summary.Contains("right deck list"));
        }

        [Test]
        public void BrowserModeUsesReadOnlyLanguage()
        {
            Assert.AreEqual("Card Browser", CardBrowserModeFormatter.FormatTitle(CardBrowserScreenMode.Browser));
            string summary = CardBrowserModeFormatter.FormatLayoutSummary(CardBrowserScreenMode.Browser);
            Assert.IsTrue(summary.Contains("read-only detail preview"));
            Assert.IsFalse(summary.Contains("right deck list"));
        }
    }
}
