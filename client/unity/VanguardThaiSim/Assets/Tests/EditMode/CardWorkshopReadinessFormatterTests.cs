using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class CardWorkshopReadinessFormatterTests
    {
        [Test]
        public void PreparingMessageNamesModeAndLocalPackWork()
        {
            string formatted = CardWorkshopReadinessFormatter.FormatPreparing(CardBrowserScreenMode.DeckBuilder);

            Assert.IsTrue(formatted.Contains("Deck Builder"));
            Assert.IsTrue(formatted.Contains("Preparing local card pack"));
        }

        [Test]
        public void ReadyDeckBuilderMessageShowsCountsAndNextAction()
        {
            string formatted = CardWorkshopReadinessFormatter.FormatReady(
                CardBrowserScreenMode.DeckBuilder,
                10836,
                24,
                null,
                null,
                null);

            Assert.IsTrue(formatted.Contains("Deck Builder ready"));
            Assert.IsTrue(formatted.Contains("Showing 24 of 10836 cards."));
            Assert.IsTrue(formatted.Contains("Filters: none"));
            Assert.IsTrue(formatted.Contains("add it to Main or Ride"));
        }

        [Test]
        public void ReadyBrowserMessageUsesReadOnlyAction()
        {
            string formatted = CardWorkshopReadinessFormatter.FormatReady(
                CardBrowserScreenMode.Browser,
                10836,
                12,
                "Blaster",
                "[BT01]",
                "Royal Paladin");

            Assert.IsTrue(formatted.Contains("Card Browser ready"));
            Assert.IsTrue(formatted.Contains("search \"Blaster\""));
            Assert.IsTrue(formatted.Contains("series [BT01]"));
            Assert.IsTrue(formatted.Contains("group Royal Paladin"));
            Assert.IsTrue(formatted.Contains("read details"));
        }

        [Test]
        public void NoResultsMessageGivesClearRecoveryAction()
        {
            string formatted = CardWorkshopReadinessFormatter.FormatReady(
                CardBrowserScreenMode.DeckBuilder,
                10836,
                0,
                "missing",
                null,
                "Dragon Empire");

            Assert.IsTrue(formatted.Contains("no matching cards"));
            Assert.IsTrue(formatted.Contains("Showing 0 of 10836 cards."));
            Assert.IsTrue(formatted.Contains(CardWorkshopReadinessFormatter.ClearFiltersAction));
        }

        [Test]
        public void FailureMessageNamesExpectedPack()
        {
            string formatted = CardWorkshopReadinessFormatter.FormatFailure(
                CardBrowserScreenMode.Browser,
                "File not found",
                "data/packs/vanguard_th");

            Assert.IsTrue(formatted.Contains("Card Browser"));
            Assert.IsTrue(formatted.Contains("Card data is unavailable."));
            Assert.IsTrue(formatted.Contains("data/packs/vanguard_th"));
            Assert.IsTrue(formatted.Contains("File not found"));
        }
    }
}
