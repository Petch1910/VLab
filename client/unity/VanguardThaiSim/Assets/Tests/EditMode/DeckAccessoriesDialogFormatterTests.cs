using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckAccessoriesDialogFormatterTests
    {
        [Test]
        public void SummaryShowsDeckFormatAndAppearance()
        {
            VanguardDeck deck = VanguardDeck.Create("Accessory Deck", "D", "pack", "version");
            deck.appearance = new DeckAppearanceMetadata
            {
                sleeve_key = "blue",
                card_back_key = "standard",
                playmat_key = "table-grid",
                crest_key = "royal-paladin",
                persona_shield_key = "persona-standard",
                gift_marker_key = "force",
                quick_shield_key = "quick-shield"
            };

            string summary = DeckAccessoriesDialogFormatter.FormatSummary(deck);

            Assert.IsTrue(summary.Contains("Accessory Deck"));
            Assert.IsTrue(summary.Contains("Format: D"));
            Assert.IsTrue(summary.Contains("Sleeve: blue"));
            Assert.IsTrue(summary.Contains("Back: standard"));
            Assert.IsTrue(summary.Contains("Playmat: table-grid"));
            Assert.IsTrue(summary.Contains("Crest: royal-paladin"));
            Assert.IsTrue(summary.Contains("Gift: force"));
            Assert.IsTrue(summary.Contains("Quick: quick-shield"));
        }

        [Test]
        public void SummaryHandlesMissingDeck()
        {
            Assert.AreEqual(
                "Deck accessories: no deck loaded.",
                DeckAccessoriesDialogFormatter.FormatSummary(null));
        }

        [Test]
        public void FormatCycleUsesDeckFormats()
        {
            Assert.AreEqual("V", DeckAccessoriesDialogFormatter.NextFormat("D"));
            Assert.AreEqual("Premium", DeckAccessoriesDialogFormatter.NextFormat("V"));
            Assert.AreEqual("D", DeckAccessoriesDialogFormatter.NextFormat("Premium"));
            Assert.AreEqual("D", DeckAccessoriesDialogFormatter.NextFormat("unknown"));
        }

        [Test]
        public void AccessoryCyclesReturnKnownKeys()
        {
            Assert.AreEqual("blue", DeckAccessoriesDialogFormatter.NextSleeve("default"));
            Assert.AreEqual("standard", DeckAccessoriesDialogFormatter.NextCardBack("default"));
            Assert.AreEqual("table-grid", DeckAccessoriesDialogFormatter.NextPlaymat("default"));
            Assert.AreEqual("royal-paladin", DeckAccessoriesDialogFormatter.NextCrest("default"));
            Assert.AreEqual("persona-standard", DeckAccessoriesDialogFormatter.NextPersonaShield("default"));
            Assert.AreEqual("force", DeckAccessoriesDialogFormatter.NextGiftMarker("default"));
            Assert.AreEqual("quick-shield", DeckAccessoriesDialogFormatter.NextQuickShield("default"));
        }
    }
}
