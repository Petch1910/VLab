using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class HeuristicBotV2Tests
    {
        [Test]
        public void DecideNextReturnsLegalActionWithoutMutatingState()
        {
            GameState state = CreateDrawState("TOP-A");
            string before = state.ToJson();

            BotDecision decision = HeuristicBotV2.DecideNext(state, 0, new FakeCardRepository());
            string after = state.ToJson();

            Assert.NotNull(decision);
            Assert.NotNull(decision.Action);
            Assert.IsTrue(RulesCore.CanExecute(state, decision.Action));
            Assert.AreEqual(before, after);
        }

        [Test]
        public void PrefersVanguardPlacementWhenVanguardIsEmpty()
        {
            GameState state = new GameState
            {
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("h1", "KNOWN-A", 0),
                            new GameCardInstance("h2", "KNOWN-B", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                }
            };

            BotDecision decision = HeuristicBotV2.DecideNext(state, 0, new FakeCardRepository());

            Assert.NotNull(decision.Action);
            Assert.AreEqual(GameActionType.MoveCard, decision.Action.action_type);
            Assert.AreEqual(GameZone.Hand, decision.Action.from_zone);
            Assert.AreEqual(GameZone.Vanguard, decision.Action.to_zone);
        }

        [Test]
        public void DrawScoreDoesNotUseTopDeckCardStats()
        {
            GameState highShieldTopDeck = CreateDrawState("TOP-HIGH-SHIELD");
            GameState lowShieldTopDeck = CreateDrawState("TOP-LOW-SHIELD");
            var repository = new FakeCardRepository();
            repository.Add("TOP-HIGH-SHIELD", 50000);
            repository.Add("TOP-LOW-SHIELD", 0);

            HeuristicBotActionEvaluation high = FindEvaluation(
                HeuristicBotV2.EvaluateLegalActions(highShieldTopDeck, 0, repository),
                GameActionType.Draw);
            HeuristicBotActionEvaluation low = FindEvaluation(
                HeuristicBotV2.EvaluateLegalActions(lowShieldTopDeck, 0, repository),
                GameActionType.Draw);

            Assert.AreEqual(high.TotalScore, low.TotalScore, 0.0000001d);
            Assert.AreEqual(high.OwnScore, low.OwnScore, 0.0000001d);
        }

        [Test]
        public void DecisionReasonDoesNotLeakOpponentOrDeckSecrets()
        {
            GameState state = CreateDrawState("P0-TOP-SECRET");
            state.GetPlayer(1).hand.Add(new GameCardInstance("opp-hand-secret", "OPP-HAND-SECRET", 1));
            state.GetPlayer(1).deck.Add(new GameCardInstance("opp-deck-secret", "OPP-DECK-SECRET", 1, false));

            BotDecision decision = HeuristicBotV2.DecideNext(state, 0, new FakeCardRepository());

            Assert.NotNull(decision);
            Assert.IsFalse(decision.Reason.Contains("P0-TOP-SECRET"));
            Assert.IsFalse(decision.Reason.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(decision.Reason.Contains("OPP-DECK-SECRET"));
        }

        [Test]
        public void EvaluationsAreDeterministic()
        {
            GameState state = CreateDrawState("TOP-A");
            var repository = new FakeCardRepository();

            IReadOnlyList<HeuristicBotActionEvaluation> first =
                HeuristicBotV2.EvaluateLegalActions(state, 0, repository);
            IReadOnlyList<HeuristicBotActionEvaluation> second =
                HeuristicBotV2.EvaluateLegalActions(state, 0, repository);

            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Accepted, second[i].Accepted);
                Assert.AreEqual(first[i].TotalScore, second[i].TotalScore, 0.0000001d);
                Assert.AreEqual(first[i].Summary, second[i].Summary);
            }
        }

        private static GameState CreateDrawState(string topDeckCardId)
        {
            return new GameState
            {
                phase = GamePhase.StandAndDraw,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-top", topDeckCardId, 0, false)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                }
            };
        }

        private static HeuristicBotActionEvaluation FindEvaluation(
            IReadOnlyList<HeuristicBotActionEvaluation> evaluations,
            GameActionType actionType)
        {
            for (int i = 0; i < evaluations.Count; i++)
            {
                if (evaluations[i].Action != null &&
                    evaluations[i].Action.action_type == actionType)
                {
                    return evaluations[i];
                }
            }

            Assert.Fail("Missing evaluation for " + actionType);
            return null;
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
