using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckImportCompatibilityTests
    {
        [Test]
        public void AnalyzerAcceptsMatchingDeck()
        {
            VanguardDeck deck = VanguardDeck.Create("Match", "D", "vanguard_th", "2026.06");
            deck.AddCard(DeckZone.Main, "CARD-001", 4);
            DeckValidationResult validation = new DeckValidator(new FakeRepository()).Validate(deck);

            DeckImportCompatibilityReport report = DeckImportCompatibilityAnalyzer.Analyze(
                deck,
                validation,
                Manifest("vanguard_th", "2026.06", "hash-a"),
                "hash-a");

            Assert.IsTrue(report.accepted);
            Assert.AreEqual(0, report.issues.Count);
            Assert.AreEqual("Compatibility: active pack match.", DeckImportCompatibilityFormatter.Format(report));
        }

        [Test]
        public void AnalyzerReportsMissingCardAndPackVersionMismatch()
        {
            VanguardDeck deck = VanguardDeck.Create("Mismatch", "D", "vanguard_th", "old-version");
            deck.AddCard(DeckZone.Main, "NO-SUCH", 1);
            DeckValidationResult validation = new DeckValidator(new FakeRepository()).Validate(deck);

            DeckImportCompatibilityReport report = DeckImportCompatibilityAnalyzer.Analyze(
                deck,
                validation,
                Manifest("vanguard_th", "2026.06", "hash-a"),
                "hash-a");
            string formatted = DeckImportCompatibilityFormatter.Format(report);

            Assert.IsFalse(report.accepted);
            StringAssert.Contains("PACK_VERSION_MISMATCH", formatted);
            StringAssert.Contains("MISSING_CARD NO-SUCH", formatted);
        }

        [Test]
        public void AnalyzerReportsPackHashMismatchWhenHashIsProvided()
        {
            VanguardDeck deck = VanguardDeck.Create("Hash Mismatch", "D", "vanguard_th", "2026.06");
            deck.AddCard(DeckZone.Main, "CARD-001", 4);
            DeckValidationResult validation = new DeckValidator(new FakeRepository()).Validate(deck);

            DeckImportCompatibilityReport report = DeckImportCompatibilityAnalyzer.Analyze(
                deck,
                validation,
                Manifest("vanguard_th", "2026.06", "hash-a"),
                "hash-b");
            string formatted = DeckImportCompatibilityFormatter.Format(report);

            Assert.IsTrue(report.accepted);
            StringAssert.Contains("PACK_HASH_MISMATCH", formatted);
            Assert.IsFalse(formatted.Contains("NO-SUCH"));
        }

        private static CardPackManifest Manifest(string packId, string sourceVersion, string definitionHash)
        {
            return new CardPackManifest
            {
                pack_id = packId,
                source_version = sourceVersion,
                definition_hash = definitionHash
            };
        }

        private sealed class FakeRepository : ICardRepository
        {
            public int CountCards()
            {
                return 1;
            }

            public int CountSeries()
            {
                return 1;
            }

            public int CountClans()
            {
                return 1;
            }

            public CardDetail GetCard(string cardId)
            {
                if (string.Equals(cardId, "CARD-001", StringComparison.OrdinalIgnoreCase))
                {
                    return new CardDetail
                    {
                        CardId = cardId,
                        NameTh = "Card 001",
                        DeckLimit = 99
                    };
                }

                return null;
            }

            public IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options)
            {
                return new List<CardSummary>();
            }

            public IReadOnlyList<SeriesOption> ListSeries()
            {
                return new List<SeriesOption>();
            }

            public IReadOnlyList<ClanOption> ListClans()
            {
                return new List<ClanOption>();
            }
        }
    }
}
