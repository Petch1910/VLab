using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class OpponentGuardEstimatorTests
    {
        [Test]
        public void MaskedOpponentHandCountsAsUnknownShield()
        {
            GameState trueState = CreateTwoPlayerState();
            GameState playerView = GameStateViewFactory.CreatePlayerView(trueState, 0);
            FakeCardRepository repository = CreateShieldRepository();

            OpponentGuardEstimate estimate = OpponentGuardEstimator.Estimate(
                playerView,
                1,
                repository);

            Assert.AreEqual(2, estimate.HandCount);
            Assert.AreEqual(0, estimate.VisibleKnownShield);
            Assert.AreEqual(2, estimate.UnknownHandCardCount);
            Assert.AreEqual(0, estimate.ConservativeShieldEstimate);
            Assert.AreEqual(20000, estimate.ExpectedShieldEstimate);
            Assert.AreEqual(30000, estimate.MaximumShieldEstimate);
            Assert.AreEqual(0.35d, estimate.Confidence, 0.0000001d);
        }

        [Test]
        public void VisibleKnownHandUsesRepositoryShieldAndMissingStatsBecomeUnknown()
        {
            GameState state = new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("h1", "SHIELD-15000", 0),
                            new GameCardInstance("h2", "SHIELD-5000", 0),
                            new GameCardInstance("h3", "MISSING", 0)
                        }
                    }
                }
            };
            FakeCardRepository repository = CreateShieldRepository();

            OpponentGuardEstimate estimate = OpponentGuardEstimator.Estimate(state, 0, repository);

            Assert.AreEqual(3, estimate.HandCount);
            Assert.AreEqual(20000, estimate.VisibleKnownShield);
            Assert.AreEqual(1, estimate.UnknownHandCardCount);
            Assert.AreEqual(20000, estimate.ConservativeShieldEstimate);
            Assert.AreEqual(30000, estimate.ExpectedShieldEstimate);
            Assert.AreEqual(35000, estimate.MaximumShieldEstimate);
            Assert.Greater(estimate.Confidence, 0.7d);
            Assert.Less(estimate.Confidence, 0.8d);
        }

        [Test]
        public void CustomUnknownShieldOptionsAffectExpectedAndMaximumEstimates()
        {
            GameState trueState = CreateTwoPlayerState();
            GameState playerView = GameStateViewFactory.CreatePlayerView(trueState, 0);
            var options = new OpponentGuardEstimatorOptions
            {
                AverageUnknownShield = 8000,
                MaximumUnknownShield = 20000
            };

            OpponentGuardEstimate estimate = OpponentGuardEstimator.Estimate(
                playerView,
                1,
                CreateShieldRepository(),
                options);

            Assert.AreEqual(16000, estimate.ExpectedShieldEstimate);
            Assert.AreEqual(40000, estimate.MaximumShieldEstimate);
        }

        [Test]
        public void EstimatorIsDeterministicAndDoesNotMutateState()
        {
            GameState playerView = GameStateViewFactory.CreatePlayerView(CreateTwoPlayerState(), 0);
            FakeCardRepository repository = CreateShieldRepository();
            string before = playerView.ToJson();

            OpponentGuardEstimate first = OpponentGuardEstimator.Estimate(playerView, 1, repository);
            OpponentGuardEstimate second = OpponentGuardEstimator.Estimate(playerView, 1, repository);
            string after = playerView.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.ExpectedShieldEstimate, second.ExpectedShieldEstimate);
            Assert.AreEqual(first.MaximumShieldEstimate, second.MaximumShieldEstimate);
            Assert.AreEqual(first.Confidence, second.Confidence, 0.0000001d);
        }

        private static GameState CreateTwoPlayerState()
        {
            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        hand = new List<GameCardInstance>()
                    },
                    new PlayerGameState
                    {
                        player_id = "p2",
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("opp-h1", "SHIELD-15000", 1),
                            new GameCardInstance("opp-h2", "SHIELD-5000", 1)
                        }
                    }
                }
            };
        }

        private static FakeCardRepository CreateShieldRepository()
        {
            var repository = new FakeCardRepository();
            repository.Add("SHIELD-15000", 15000);
            repository.Add("SHIELD-5000", 5000);
            return repository;
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
