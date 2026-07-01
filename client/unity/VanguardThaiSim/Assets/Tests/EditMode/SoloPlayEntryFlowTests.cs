using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class SoloPlayEntryFlowTests
    {
        [Test]
        public void DifficultyCyclesEasyNormalHard()
        {
            Assert.AreEqual(BotDifficulty.Easy, SoloPlayEntryFlow.DifficultyFromIndex(0));
            Assert.AreEqual(BotDifficulty.Normal, SoloPlayEntryFlow.DifficultyFromIndex(1));
            Assert.AreEqual(BotDifficulty.Hard, SoloPlayEntryFlow.DifficultyFromIndex(2));
            Assert.AreEqual(1, SoloPlayEntryFlow.NextDifficultyIndex(0));
            Assert.AreEqual(2, SoloPlayEntryFlow.NextDifficultyIndex(1));
            Assert.AreEqual(0, SoloPlayEntryFlow.NextDifficultyIndex(2));
        }

        [Test]
        public void OpponentChoiceIncludesMirrorRandomAndSavedDecks()
        {
            List<VanguardDeck> saved = new List<VanguardDeck>
            {
                CreatePlayableDeck("Bot A"),
                CreatePlayableDeck("Bot B")
            };

            Assert.AreEqual("Mirror player deck", SoloPlayEntryFlow.FormatOpponentChoiceLabel(0, saved));
            Assert.AreEqual("Random saved deck", SoloPlayEntryFlow.FormatOpponentChoiceLabel(1, saved));
            Assert.AreEqual("Saved deck: Bot A", SoloPlayEntryFlow.FormatOpponentChoiceLabel(2, saved));
            Assert.AreEqual("Saved deck: Bot B", SoloPlayEntryFlow.FormatOpponentChoiceLabel(3, saved));
            Assert.AreEqual(0, SoloPlayEntryFlow.NextOpponentChoiceIndex(3, saved));
        }

        [Test]
        public void MissingPlayerDeckRejectsBeforeOpponentSelection()
        {
            SoloPlayEntryFlowStartResult result = SoloPlayEntryFlow.CreateStartRequest(
                null,
                null,
                new DeckValidator(new FakeRepository()),
                new List<VanguardDeck>(),
                new SoloPlayEntryFlowOptions());

            Assert.IsFalse(result.can_start);
            Assert.AreEqual(SoloPlayEntryFlow.MissingPlayerDeckReason, result.rejection_reason);
            Assert.IsNull(result.player_deck);
            Assert.IsNull(result.opponent_deck);
        }

        [Test]
        public void MirrorPlayerDeckCreatesClonedPracticeDecks()
        {
            VanguardDeck player = CreatePlayableDeck("Player");
            DeckValidator validator = new DeckValidator(new FakeRepository());
            DeckValidationResult validation = validator.Validate(player);

            SoloPlayEntryFlowStartResult result = SoloPlayEntryFlow.CreateStartRequest(
                player,
                validation,
                validator,
                new List<VanguardDeck>(),
                new SoloPlayEntryFlowOptions
                {
                    difficulty = BotDifficulty.Normal,
                    opponent_deck_mode = SoloPlayOpponentDeckMode.MirrorPlayerDeck
                });

            Assert.IsTrue(result.can_start);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.AreEqual("Normal - balanced profile", result.difficulty_label);
            Assert.AreEqual("Mirror player deck", result.opponent_deck_label);
            Assert.AreNotSame(player, result.player_deck);
            Assert.AreNotSame(player, result.opponent_deck);
            Assert.AreEqual(player.TotalCards(DeckZone.Main), result.player_deck.TotalCards(DeckZone.Main));
            Assert.IsTrue(result.playtable_mode_detail.Contains("Solo Practice"));
        }

        [Test]
        public void RandomSavedDeckChoosesOnlyPlayableDecksDeterministically()
        {
            VanguardDeck player = CreatePlayableDeck("Player");
            VanguardDeck incomplete = VanguardDeck.Create("Incomplete Bot", "D", "pack", "version");
            incomplete.AddCard(DeckZone.Main, "CARD-001", 1);
            VanguardDeck playable = CreatePlayableDeck("Playable Bot");
            DeckValidator validator = new DeckValidator(new FakeRepository());

            SoloPlayEntryFlowStartResult result = SoloPlayEntryFlow.CreateStartRequest(
                player,
                validator.Validate(player),
                validator,
                new List<VanguardDeck> { incomplete, playable },
                new SoloPlayEntryFlowOptions
                {
                    difficulty = BotDifficulty.Hard,
                    opponent_deck_mode = SoloPlayOpponentDeckMode.RandomSavedDeck,
                    random_seed = 99
                });

            Assert.IsTrue(result.can_start);
            Assert.AreEqual("Random saved deck: Playable Bot", result.opponent_deck_label);
            Assert.AreEqual("Hard - strongest available heuristic", result.difficulty_label);
            Assert.AreEqual("Playable Bot", result.opponent_deck.name);
        }

        [Test]
        public void SavedDeckRejectsWhenDeckIsNotPlayable()
        {
            VanguardDeck player = CreatePlayableDeck("Player");
            VanguardDeck incomplete = VanguardDeck.Create("Incomplete Bot", "D", "pack", "version");
            incomplete.AddCard(DeckZone.Main, "CARD-001", 1);
            DeckValidator validator = new DeckValidator(new FakeRepository());

            SoloPlayEntryFlowStartResult result = SoloPlayEntryFlow.CreateStartRequest(
                player,
                validator.Validate(player),
                validator,
                new List<VanguardDeck> { incomplete },
                new SoloPlayEntryFlowOptions
                {
                    opponent_deck_mode = SoloPlayOpponentDeckMode.SavedDeck,
                    opponent_deck_id = incomplete.deck_id
                });

            Assert.IsFalse(result.can_start);
            Assert.AreEqual(SoloPlayEntryFlow.OpponentDeckNotPlayableReason, result.rejection_reason);
            Assert.IsTrue(result.status_message.Contains("bot deck is not playable"));
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
