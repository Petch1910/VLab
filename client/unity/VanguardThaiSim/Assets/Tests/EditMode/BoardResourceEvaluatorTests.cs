using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class BoardResourceEvaluatorTests
    {
        [Test]
        public void EvaluatorScoresVisibleHandShieldAndResources()
        {
            GameState state = CreateEvaluationState(damageCount: 3, faceUpDamageCount: 2);
            var repository = new FakeCardRepository();
            repository.Add("SHIELD-15000", 15000);
            repository.Add("SHIELD-5000", 5000);

            BoardResourceEvaluation evaluation = BoardResourceEvaluator.Evaluate(
                state,
                0,
                repository);

            Assert.AreEqual(3, evaluation.HandCount);
            Assert.AreEqual(20000, evaluation.VisibleHandShield);
            Assert.AreEqual(1, evaluation.UnknownHandStatCount);
            Assert.AreEqual(2, evaluation.AvailableCounterBlastCount);
            Assert.AreEqual(1, evaluation.EstimatedSoulCount);
            Assert.AreEqual(40, evaluation.DeckCount);
            Assert.AreEqual(7.8d, evaluation.TotalScore, 0.0000001d);
            Assert.NotNull(evaluation.FindTerm("visible_hand_shield"));
            Assert.NotNull(evaluation.FindTerm("available_counter_blast"));
        }

        [Test]
        public void HighDamageLowersEvaluationScore()
        {
            var repository = new FakeCardRepository();
            repository.Add("SHIELD-15000", 15000);
            repository.Add("SHIELD-5000", 5000);
            GameState safer = CreateEvaluationState(damageCount: 2, faceUpDamageCount: 2);
            GameState danger = CreateEvaluationState(damageCount: 5, faceUpDamageCount: 2);

            BoardResourceEvaluation safeEvaluation = BoardResourceEvaluator.Evaluate(safer, 0, repository);
            BoardResourceEvaluation dangerEvaluation = BoardResourceEvaluator.Evaluate(danger, 0, repository);

            Assert.Less(dangerEvaluation.TotalScore, safeEvaluation.TotalScore);
            Assert.NotNull(dangerEvaluation.FindTerm("five_damage_pressure"));
        }

        [Test]
        public void HiddenAndMissingHandStatsBecomeUnknowns()
        {
            var player = new PlayerGameState
            {
                player_id = "p1",
                hand = new List<GameCardInstance>
                {
                    new GameCardInstance("hidden", GameStateViewFactory.HiddenCardId, 0, false),
                    new GameCardInstance("missing", "MISSING-CARD", 0)
                }
            };
            var state = new GameState
            {
                players = new List<PlayerGameState> { player }
            };

            BoardResourceEvaluation evaluation = BoardResourceEvaluator.Evaluate(
                state,
                0,
                new FakeCardRepository());

            Assert.AreEqual(2, evaluation.HandCount);
            Assert.AreEqual(0, evaluation.VisibleHandShield);
            Assert.AreEqual(2, evaluation.UnknownHandStatCount);
        }

        [Test]
        public void EvaluatorIsDeterministic()
        {
            GameState state = CreateEvaluationState(damageCount: 3, faceUpDamageCount: 2);
            var repository = new FakeCardRepository();
            repository.Add("SHIELD-15000", 15000);
            repository.Add("SHIELD-5000", 5000);

            BoardResourceEvaluation first = BoardResourceEvaluator.Evaluate(state, 0, repository);
            BoardResourceEvaluation second = BoardResourceEvaluator.Evaluate(state, 0, repository);

            Assert.AreEqual(first.TotalScore, second.TotalScore, 0.0000001d);
            Assert.AreEqual(first.Terms.Count, second.Terms.Count);
            for (int i = 0; i < first.Terms.Count; i++)
            {
                Assert.AreEqual(first.Terms[i].Key, second.Terms[i].Key);
                Assert.AreEqual(first.Terms[i].Score, second.Terms[i].Score, 0.0000001d);
            }
        }

        private static GameState CreateEvaluationState(int damageCount, int faceUpDamageCount)
        {
            var player = new PlayerGameState
            {
                player_id = "p1",
                hand = new List<GameCardInstance>
                {
                    new GameCardInstance("h1", "SHIELD-15000", 0),
                    new GameCardInstance("h2", "SHIELD-5000", 0),
                    new GameCardInstance("h3", GameStateViewFactory.HiddenCardId, 0, false)
                },
                vanguard = new List<GameCardInstance>
                {
                    new GameCardInstance("v1", "VANGUARD-1", 0),
                    new GameCardInstance("v2", "VANGUARD-2", 0)
                },
                rear_guard = new List<GameCardInstance>
                {
                    new GameCardInstance("r1", "REAR-1", 0),
                    new GameCardInstance("r2", "REAR-2", 0)
                }
            };

            for (int i = 0; i < damageCount; i++)
            {
                player.damage.Add(new GameCardInstance(
                    "d" + i,
                    "DAMAGE-" + i,
                    0,
                    i < faceUpDamageCount));
            }

            for (int i = 0; i < 40; i++)
            {
                player.deck.Add(new GameCardInstance("deck" + i, "DECK-" + i, 0, false));
            }

            return new GameState
            {
                players = new List<PlayerGameState> { player }
            };
        }

        private sealed class FakeCardRepository : ICardRepository
        {
            private readonly Dictionary<string, CardDetail> cards = new Dictionary<string, CardDetail>();

            public void Add(string cardId, int? shield)
            {
                cards[cardId] = new CardDetail
                {
                    CardId = cardId,
                    Shield = shield
                };
            }

            public int CountCards()
            {
                return cards.Count;
            }

            public int CountSeries()
            {
                return 0;
            }

            public int CountClans()
            {
                return 0;
            }

            public CardDetail GetCard(string cardId)
            {
                CardDetail detail;
                return cards.TryGetValue(cardId, out detail) ? detail : null;
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
