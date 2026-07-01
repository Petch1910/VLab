using NUnit.Framework;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Tests
{
    public sealed class CountLineDeckCodecTests
    {
        [Test]
        public void ExportImportRoundTripsMetadataAndZones()
        {
            VanguardDeck deck = VanguardDeck.Create("Text Deck", "D", "vanguard_th", "2026.06");
            deck.AddCard(DeckZone.Main, "BT01-001TH", 4);
            deck.AddCard(DeckZone.Ride, "BT01-002TH", 1);
            deck.AddCard(DeckZone.G, "G-BT01-001TH", 2);

            string text = CountLineDeckCodec.Export(deck, "pack-hash");
            CountLineDeckImportResult result = CountLineDeckCodec.ImportDetailed(text);
            VanguardDeck imported = result.deck;

            StringAssert.Contains(CountLineDeckCodec.Header, text);
            StringAssert.Contains("[Main]", text);
            StringAssert.Contains("PackDefinitionHash: pack-hash", text);
            Assert.AreEqual("pack-hash", result.pack_definition_hash);
            Assert.AreEqual("Text Deck", imported.name);
            Assert.AreEqual("D", imported.format);
            Assert.AreEqual("vanguard_th", imported.card_pack_id);
            Assert.AreEqual("2026.06", imported.card_pack_version);
            Assert.AreEqual(4, imported.GetQuantity(DeckZone.Main, "BT01-001TH"));
            Assert.AreEqual(1, imported.GetQuantity(DeckZone.Ride, "BT01-002TH"));
            Assert.AreEqual(2, imported.GetQuantity(DeckZone.G, "G-BT01-001TH"));
        }

        [Test]
        public void ImportRejectsCardLineBeforeZone()
        {
            Assert.Throws<System.FormatException>(delegate
            {
                CountLineDeckCodec.Import("Name: Bad\n1 CARD-001");
            });
        }

        [Test]
        public void ImportRejectsInvalidQuantity()
        {
            Assert.Throws<System.FormatException>(delegate
            {
                CountLineDeckCodec.Import("[Main]\n0 CARD-001");
            });
        }

        [Test]
        public void ImportIgnoresBlankLinesAndComments()
        {
            VanguardDeck deck = CountLineDeckCodec.Import(
                "# comment\nName: Comment Deck\nFormat: D\n\n[Main]\n# main card\n2 CARD-001\n");

            Assert.AreEqual("Comment Deck", deck.name);
            Assert.AreEqual(2, deck.GetQuantity(DeckZone.Main, "CARD-001"));
        }
    }
}
