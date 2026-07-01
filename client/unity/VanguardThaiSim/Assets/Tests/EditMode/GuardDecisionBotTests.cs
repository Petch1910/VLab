using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class GuardDecisionBotTests
    {
        [Test]
        public void LethalAttackRecommendsGuardWhenShieldIsAvailable()
        {
            GameState state = CreateGuardState(damageCount: 5, handCardId: "SHIELD-15000");

            GuardDecisionResult result = GuardDecisionBot.Decide(
                state,
                CreateRequest(attackPower: 18000, defendingPower: 13000),
                CreateShieldRepository());

            Assert.AreEqual(GuardDecisionType.Guard, result.Decision);
            Assert.AreEqual(10000, result.ShieldNeeded);
            Assert.IsTrue(result.LethalRisk);
        }

        [Test]
        public void LowDamageAttackRecommendsNoGuard()
        {
            GameState state = CreateGuardState(damageCount: 1, handCardId: "SHIELD-15000");

            GuardDecisionResult result = GuardDecisionBot.Decide(
                state,
                CreateRequest(attackPower: 18000, defendingPower: 13000),
                CreateShieldRepository());

            Assert.AreEqual(GuardDecisionType.NoGuard, result.Decision);
            Assert.IsFalse(result.LethalRisk);
            Assert.IsFalse(result.HighDamageRisk);
        }

        [Test]
        public void HighTriggerRiskAtDangerDamageRecommendsGuard()
        {
            GameState state = CreateGuardState(damageCount: 3, handCardId: "SHIELD-15000");
            TriggerProbabilityEngine.TryCalculate(10, 8, 2, out TriggerProbabilityResult triggerProbability);

            GuardDecisionResult result = GuardDecisionBot.Decide(
                state,
                CreateRequest(attackPower: 13000, defendingPower: 13000),
                CreateShieldRepository(),
                triggerProbability);

            Assert.AreEqual(GuardDecisionType.Guard, result.Decision);
            Assert.AreEqual(5000, result.ShieldNeeded);
            Assert.IsTrue(result.HighTriggerRisk);
            Assert.GreaterOrEqual(result.TriggerRisk, 0.35d);
        }

        [Test]
        public void InsufficientShieldReturnsCannotGuard()
        {
            GameState state = CreateGuardState(damageCount: 5, handCardId: null);

            GuardDecisionResult result = GuardDecisionBot.Decide(
                state,
                CreateRequest(attackPower: 43000, defendingPower: 13000),
                CreateShieldRepository());

            Assert.AreEqual(GuardDecisionType.CannotGuard, result.Decision);
            Assert.AreEqual(35000, result.ShieldNeeded);
        }

        [Test]
        public void HighShieldNeedPrefersPerfectGuard()
        {
            GameState state = CreateGuardState(damageCount: 5, handCardId: "SHIELD-15000");
            state.GetPlayer(0).hand.Add(new GameCardInstance("h2", "UNKNOWN-SHIELD", 0, false));
            var estimatorOptions = new OpponentGuardEstimatorOptions
            {
                AverageUnknownShield = 10000,
                MaximumUnknownShield = 40000
            };

            GuardDecisionResult result = GuardDecisionBot.Decide(
                state,
                CreateRequest(attackPower: 43000, defendingPower: 13000),
                CreateShieldRepository(),
                null,
                null,
                estimatorOptions);

            Assert.AreEqual(GuardDecisionType.PerfectGuardPreferred, result.Decision);
            Assert.AreEqual(35000, result.ShieldNeeded);
        }

        [Test]
        public void DecisionDoesNotMutateStateOrLeakOpponentSecrets()
        {
            GameState trueState = CreateGuardState(damageCount: 5, handCardId: "SHIELD-15000");
            trueState.GetPlayer(1).hand.Add(new GameCardInstance("opp-secret-hand", "OPP-HAND-SECRET", 1));
            trueState.GetPlayer(1).deck.Add(new GameCardInstance("opp-secret-deck", "OPP-DECK-SECRET", 1, false));
            GameState playerView = GameStateViewFactory.CreatePlayerView(trueState, 0);
            string before = playerView.ToJson();

            GuardDecisionResult result = GuardDecisionBot.Decide(
                playerView,
                CreateRequest(attackPower: 18000, defendingPower: 13000),
                CreateShieldRepository());
            string after = playerView.ToJson();

            Assert.AreEqual(before, after);
            Assert.IsFalse(result.Reason.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(result.Reason.Contains("OPP-DECK-SECRET"));
        }

        [Test]
        public void DecisionsAreDeterministic()
        {
            GameState state = CreateGuardState(damageCount: 5, handCardId: "SHIELD-15000");
            GuardDecisionRequest request = CreateRequest(attackPower: 18000, defendingPower: 13000);
            FakeCardRepository repository = CreateShieldRepository();

            GuardDecisionResult first = GuardDecisionBot.Decide(state, request, repository);
            GuardDecisionResult second = GuardDecisionBot.Decide(state, request, repository);

            Assert.AreEqual(first.Decision, second.Decision);
            Assert.AreEqual(first.ShieldNeeded, second.ShieldNeeded);
            Assert.AreEqual(first.Reason, second.Reason);
        }

        private static GuardDecisionRequest CreateRequest(int attackPower, int defendingPower)
        {
            return new GuardDecisionRequest
            {
                attacker_player_index = 1,
                defender_player_index = 0,
                attack_power = attackPower,
                defending_power = defendingPower,
                incoming_critical = 1,
                attack_can_gain_triggers = true
            };
        }

        private static GameState CreateGuardState(int damageCount, string handCardId)
        {
            var defender = new PlayerGameState
            {
                player_id = "p1"
            };

            if (!string.IsNullOrEmpty(handCardId))
            {
                defender.hand.Add(new GameCardInstance("h1", handCardId, 0));
            }

            for (int i = 0; i < damageCount; i++)
            {
                defender.damage.Add(new GameCardInstance("d" + i, "DAMAGE-" + i, 0));
            }

            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    defender,
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                }
            };
        }

        private static FakeCardRepository CreateShieldRepository()
        {
            var repository = new FakeCardRepository();
            repository.Add("SHIELD-15000", 15000);
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
