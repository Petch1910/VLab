using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckToolsDialogFormatterTests
    {
        [Test]
        public void DialogStatusShowsDeckCounts()
        {
            VanguardDeck deck = VanguardDeck.Create("Tools Deck", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            deck.AddCard(DeckZone.Ride, "CARD-002", 4);

            string formatted = DeckToolsDialogFormatter.FormatDialogStatus(deck);

            Assert.IsTrue(formatted.Contains("Tools Deck"));
            Assert.IsTrue(formatted.Contains("Main 50"));
            Assert.IsTrue(formatted.Contains("Ride 4"));
            Assert.IsTrue(formatted.Contains("G 0"));
        }

        [Test]
        public void OperationResultCompactsErrors()
        {
            string formatted = DeckToolsDialogFormatter.FormatOperationResult(
                "Apply",
                false,
                "bad\n deck\tcode");

            Assert.AreEqual("Rejected [Apply]: bad deck code", formatted);
        }

        [Test]
        public void OperationResultWithTipAddsDeckLoadTip()
        {
            string formatted = DeckToolsDialogFormatter.FormatOperationResultWithTip(
                "Load",
                true,
                "Tools Deck",
                LoadingTipCatalog.DeckLoad);

            StringAssert.Contains("OK [Load]: Tools Deck", formatted);
            StringAssert.Contains("Tip:", formatted);
            StringAssert.Contains("cosmetics separate from legality", formatted);
        }
    }
}
