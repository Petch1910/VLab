using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckBuilderFilterPanelFormatterTests
    {
        [Test]
        public void CardPoolStatusFormatsNoFilters()
        {
            string formatted = DeckBuilderFilterPanelFormatter.FormatCardPoolStatus(
                10836,
                24,
                null,
                null,
                null,
                "2026.06",
                CreateAcceptedPackStatus());

            Assert.IsTrue(formatted.Contains("Card pool"));
            Assert.IsTrue(formatted.Contains("Total 10836"));
            Assert.IsTrue(formatted.Contains("Showing 24"));
            Assert.IsTrue(formatted.Contains("Filters: none"));
            Assert.IsTrue(formatted.Contains("Pack 2026.06"));
            Assert.IsTrue(formatted.Contains("Pack validation: OK"));
        }

        [Test]
        public void CardPoolStatusFormatsActiveFilters()
        {
            string formatted = DeckBuilderFilterPanelFormatter.FormatCardPoolStatus(
                10836,
                7,
                "  dragon\nempire  ",
                "[BT01] VG-BT01 : Descent of the King of Knights",
                "Royal Paladin",
                "2026.06",
                CreateAcceptedPackStatus());

            Assert.IsTrue(formatted.Contains("Showing 7"));
            Assert.IsTrue(formatted.Contains("search \"dragon empire\""));
            Assert.IsTrue(formatted.Contains("series [BT01] VG-BT01 : Descent"));
            Assert.IsTrue(formatted.Contains("group Royal Paladin"));
            Assert.IsFalse(formatted.Contains("\n"));
        }

        [Test]
        public void BlankFiltersNormalizeAway()
        {
            Assert.AreEqual(
                DeckBuilderFilterPanelFormatter.NoFiltersLabel,
                DeckBuilderFilterPanelFormatter.FormatFilters("  ", "\t", "\n"));
        }

        [Test]
        public void DeckStatusIncludesCountsIssuesAndPlayableState()
        {
            VanguardDeck deck = VanguardDeck.Create("Playable", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            DeckValidationResult result = new DeckValidator(new FakeRepository()).Validate(deck);

            string formatted = DeckBuilderFilterPanelFormatter.FormatDeckStatus(result);

            Assert.IsTrue(formatted.Contains("Deck status"));
            Assert.IsTrue(formatted.Contains("Main 50/50"));
            Assert.IsTrue(formatted.Contains("Ride 0/4"));
            Assert.IsTrue(formatted.Contains("G 0/16"));
            Assert.IsTrue(formatted.Contains("Issues: 0 errors / 0 warnings"));
            Assert.IsTrue(formatted.Contains("Playable: yes"));
        }

        [Test]
        public void RuleBadgeShowsFormatAndPlayableState()
        {
            VanguardDeck deck = VanguardDeck.Create("Playable", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            DeckValidationResult result = new DeckValidator(new FakeRepository()).Validate(deck);

            string formatted = DeckBuilderFilterPanelFormatter.FormatRuleBadge(deck, result);

            Assert.AreEqual("Rule: D | Playable", formatted);
        }

        [Test]
        public void DeckCountersShowMainRideGAndIssues()
        {
            VanguardDeck deck = VanguardDeck.Create("Playable", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            DeckValidationResult result = new DeckValidator(new FakeRepository()).Validate(deck);

            string formatted = DeckBuilderFilterPanelFormatter.FormatDeckCounters(result);

            Assert.IsTrue(formatted.Contains("Counters"));
            Assert.IsTrue(formatted.Contains("Main 50/50"));
            Assert.IsTrue(formatted.Contains("Ride 0/4"));
            Assert.IsTrue(formatted.Contains("G 0/16"));
            Assert.IsTrue(formatted.Contains("Issues | 0 errors / 0 warnings"));
        }

        [Test]
        public void IssueSummaryIsBoundedAndKeepsSeverity()
        {
            VanguardDeck deck = VanguardDeck.Create("Bad", "D", "pack", "version");
            deck.main.Add(new DeckCardEntry("", 1));
            deck.main.Add(new DeckCardEntry("NO-001", 1));
            deck.main.Add(new DeckCardEntry("NO-002", 1));
            deck.main.Add(new DeckCardEntry("NO-003", 1));
            deck.main.Add(new DeckCardEntry("NO-004", 1));
            DeckValidationResult result = new DeckValidator(new FakeRepository()).Validate(deck);

            string formatted = DeckBuilderFilterPanelFormatter.FormatIssues(result, 3);

            Assert.IsTrue(formatted.Contains("Error: MISSING_CARD_ID (Main)"));
            Assert.IsTrue(formatted.Contains("Error: UNKNOWN_CARD NO-001 (Main)"));
            Assert.IsTrue(formatted.Contains("Error: UNKNOWN_CARD NO-002 (Main)"));
            Assert.IsTrue(formatted.Contains("+"));
            Assert.IsTrue(formatted.Contains("more issues"));
        }

        [Test]
        public void FormattingDoesNotMutateValidationResult()
        {
            VanguardDeck deck = VanguardDeck.Create("Bad", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "NO-001", 1);
            DeckValidationResult result = new DeckValidator(new FakeRepository()).Validate(deck);
            int issueCount = result.Issues.Count;
            string firstCode = result.Issues[0].Code;
            int errorCount = result.ErrorCount;
            int warningCount = result.WarningCount;

            DeckBuilderFilterPanelFormatter.FormatDeckStatus(result);
            DeckBuilderFilterPanelFormatter.FormatIssues(result);

            Assert.AreEqual(issueCount, result.Issues.Count);
            Assert.AreEqual(firstCode, result.Issues[0].Code);
            Assert.AreEqual(errorCount, result.ErrorCount);
            Assert.AreEqual(warningCount, result.WarningCount);
        }

        private static CardPackValidationStatus CreateAcceptedPackStatus()
        {
            return new CardPackValidationStatus
            {
                accepted = true,
                pack_id = "vanguard_th",
                source_version = "2026.06",
                schema_version = 1,
                source_schema_version = 2,
                card_count = 10836,
                source_ability_count = 20,
                capabilities_summary = "cards,abilities",
                issues = new List<CardPackValidationIssue>()
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
                if (cardId == "CARD-001")
                {
                    return new CardDetail
                    {
                        CardId = cardId,
                        NameTh = "Test Card",
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
