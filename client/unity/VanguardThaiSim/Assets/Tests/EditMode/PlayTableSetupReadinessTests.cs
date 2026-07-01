using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableSetupReadinessTests
    {
        [Test]
        public void MissingDeckBlocksStart()
        {
            PlayTableSetupReadinessResult result = PlayTableSetupReadiness.Evaluate(null, null);

            Assert.IsFalse(result.can_start);
            Assert.AreEqual(PlayTableSetupReadiness.MissingDeckReason, result.rejection_reason);
            Assert.IsTrue(result.status_message.Contains("select a deck"));
            Assert.AreEqual(GameStateFactory.OpeningHandSize, result.opening_hand_count);
        }

        [Test]
        public void MissingValidationBlocksStart()
        {
            VanguardDeck deck = VanguardDeck.Create("Draft", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 1);

            PlayTableSetupReadinessResult result = PlayTableSetupReadiness.Evaluate(deck, null);

            Assert.IsFalse(result.can_start);
            Assert.AreEqual(PlayTableSetupReadiness.MissingValidationReason, result.rejection_reason);
            Assert.AreEqual(1, result.main_count);
            Assert.IsTrue(result.status_message.Contains("validation is not ready"));
        }

        [Test]
        public void IncompleteDeckBlocksStartWithCountsAndIssues()
        {
            VanguardDeck deck = VanguardDeck.Create("Incomplete", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 1);
            DeckValidationResult validation = new DeckValidator(new FakeRepository()).Validate(deck);

            PlayTableSetupReadinessResult result = PlayTableSetupReadiness.Evaluate(deck, validation);

            Assert.IsFalse(result.can_start);
            Assert.AreEqual(PlayTableSetupReadiness.NotPlayableReason, result.rejection_reason);
            Assert.IsTrue(result.status_message.Contains("Main 1/50"));
            Assert.IsTrue(result.status_message.Contains("Ride 0/4"));
            Assert.IsTrue(result.status_message.Contains("G 0/16"));
            Assert.IsTrue(result.status_message.Contains("0 errors / 1 warnings"));
        }

        [Test]
        public void PlayableDeckAllowsStartWithMulliganSetupSummary()
        {
            VanguardDeck deck = CreatePlayableDeck("Ready Deck");
            DeckValidationResult validation = new DeckValidator(new FakeRepository()).Validate(deck);

            PlayTableSetupReadinessResult result = PlayTableSetupReadiness.Evaluate(deck, validation);

            Assert.IsTrue(result.can_start);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.AreEqual(50, result.main_count);
            Assert.AreEqual(GameStateFactory.OpeningHandSize, result.opening_hand_count);
            Assert.IsTrue(result.status_message.Contains("Ready Deck"));
            Assert.IsTrue(result.status_message.Contains("Main 50/50"));
            Assert.IsTrue(result.status_message.Contains("opening hand 5"));
            Assert.IsTrue(result.status_message.Contains("phase Mulligan"));
        }

        [Test]
        public void EvaluationDoesNotMutateValidationResult()
        {
            VanguardDeck deck = VanguardDeck.Create("Incomplete", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 1);
            DeckValidationResult validation = new DeckValidator(new FakeRepository()).Validate(deck);
            int issueCount = validation.Issues.Count;
            int errorCount = validation.ErrorCount;
            int warningCount = validation.WarningCount;

            PlayTableSetupReadiness.Evaluate(deck, validation);

            Assert.AreEqual(issueCount, validation.Issues.Count);
            Assert.AreEqual(errorCount, validation.ErrorCount);
            Assert.AreEqual(warningCount, validation.WarningCount);
        }

        private static VanguardDeck CreatePlayableDeck(string name)
        {
            VanguardDeck deck = VanguardDeck.Create(name, "D", "pack", "version");
            for (int i = 1; i <= 12; i++)
            {
                deck.AddCard(DeckZone.Main, "CARD-" + i.ToString("D3"), 4);
            }

            deck.AddCard(DeckZone.Main, "CARD-013", 2);
            return deck;
        }

        private sealed class FakeRepository : ICardRepository
        {
            public int CountCards()
            {
                return 13;
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
                if (!string.IsNullOrWhiteSpace(cardId) &&
                    cardId.StartsWith("CARD-", StringComparison.OrdinalIgnoreCase))
                {
                    return new CardDetail
                    {
                        CardId = cardId,
                        NameTh = cardId,
                        DeckLimit = 4
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
