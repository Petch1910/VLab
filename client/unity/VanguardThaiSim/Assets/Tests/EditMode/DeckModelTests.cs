using System;
using System.IO;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckModelTests
    {
        private const string KnownCardId = "BT01-001TH";

        [Test]
        public void DeckCanAddRemoveAndSerialize()
        {
            VanguardDeck deck = VanguardDeck.Create("Royal Test", "D", "vanguard_th", "test");

            deck.AddCard(DeckZone.Main, KnownCardId, 2);
            deck.AddCard(DeckZone.Ride, KnownCardId, 1);

            Assert.AreEqual(2, deck.GetQuantity(DeckZone.Main, KnownCardId));
            Assert.AreEqual(1, deck.GetQuantity(DeckZone.Ride, KnownCardId));
            Assert.AreEqual(3, deck.TotalCards());

            Assert.IsTrue(deck.RemoveCard(DeckZone.Main, KnownCardId));
            Assert.AreEqual(1, deck.GetQuantity(DeckZone.Main, KnownCardId));

            string json = deck.ToJson();
            VanguardDeck roundTrip = VanguardDeck.FromJson(json);
            Assert.AreEqual(deck.deck_id, roundTrip.deck_id);
            Assert.AreEqual(1, roundTrip.GetQuantity(DeckZone.Main, KnownCardId));
            Assert.AreEqual(1, roundTrip.GetQuantity(DeckZone.Ride, KnownCardId));
        }

        [Test]
        public void ValidatorFlagsUnknownCardsAndCopyLimit()
        {
            using (SqliteCardRepository repository = CreateRepository())
            {
                VanguardDeck deck = VanguardDeck.Create("Bad Deck", "D", "vanguard_th", "test");
                deck.AddCard(DeckZone.Main, KnownCardId, 5);
                deck.AddCard(DeckZone.Main, "NO-SUCH-CARD", 1);

                DeckValidationResult result = new DeckValidator(repository).Validate(deck);

                Assert.IsTrue(result.HasErrors);
                AssertHasIssue(result, "COPY_LIMIT_EXCEEDED");
                AssertHasIssue(result, "UNKNOWN_CARD");
            }
        }

        [Test]
        public void ValidatorReportsCountsForIncompleteDeck()
        {
            using (SqliteCardRepository repository = CreateRepository())
            {
                VanguardDeck deck = VanguardDeck.Create("Incomplete Deck", "D", "vanguard_th", "test");
                deck.AddCard(DeckZone.Main, KnownCardId, 4);

                DeckValidationResult result = new DeckValidator(repository).Validate(deck);

                Assert.AreEqual(4, result.MainCount);
                Assert.IsFalse(result.HasErrors);
                Assert.IsTrue(result.HasWarnings);
                Assert.IsFalse(result.IsComplete);
                Assert.IsFalse(result.IsPlayable);
                AssertHasIssue(result, "MAIN_DECK_INCOMPLETE");
            }
        }

        [Test]
        public void DeckCodeRoundTrips()
        {
            VanguardDeck deck = VanguardDeck.Create("Code Deck", "D", "vanguard_th", "test");
            deck.AddCard(DeckZone.Main, KnownCardId, 4);
            deck.AddCard(DeckZone.Ride, KnownCardId, 1);

            string code = DeckCodeCodec.Export(deck);
            VanguardDeck imported = DeckCodeCodec.Import(code);

            Assert.IsTrue(code.StartsWith(DeckCodeCodec.Prefix));
            Assert.AreEqual(deck.deck_id, imported.deck_id);
            Assert.AreEqual(4, imported.GetQuantity(DeckZone.Main, KnownCardId));
            Assert.AreEqual(1, imported.GetQuantity(DeckZone.Ride, KnownCardId));
        }

        [Test]
        public void DeckStorageSavesLoadsListsAndDeletes()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "VanguardThaiSimTests", Guid.NewGuid().ToString("N"));
            try
            {
                DeckStorage storage = new DeckStorage(tempRoot);
                VanguardDeck deck = VanguardDeck.Create("Stored Deck", "D", "vanguard_th", "test");
                deck.AddCard(DeckZone.Main, KnownCardId, 3);

                string path = storage.Save(deck);
                Assert.IsTrue(File.Exists(path), path);
                Assert.Contains(deck.deck_id, (System.Collections.ICollection)storage.ListDeckIds());

                VanguardDeck loaded = storage.Load(deck.deck_id);
                Assert.AreEqual(deck.deck_id, loaded.deck_id);
                Assert.AreEqual(3, loaded.GetQuantity(DeckZone.Main, KnownCardId));

                VanguardDeck latest = storage.LoadLatest();
                Assert.NotNull(latest);
                Assert.AreEqual(deck.deck_id, latest.deck_id);

                Assert.IsTrue(storage.Delete(deck.deck_id));
                Assert.IsFalse(File.Exists(path));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        private static SqliteCardRepository CreateRepository()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
            return new SqliteCardRepository(databasePath);
        }

        private static void AssertHasIssue(DeckValidationResult result, string code)
        {
            foreach (DeckValidationIssue issue in result.Issues)
            {
                if (issue.Code == code)
                {
                    return;
                }
            }

            Assert.Fail("Expected validation issue: " + code);
        }
    }
}
