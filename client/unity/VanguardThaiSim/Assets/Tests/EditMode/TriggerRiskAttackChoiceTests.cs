using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerRiskAttackChoiceTests
    {
        [Test]
        public void HighTriggerRiskCanChangeChoiceToVanguardFirst()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            FakeCardRepository repository = CreatePowerRepository();
            var options = new TriggerRiskAttackChoiceOptions
            {
                BattleOptions = new BattleSequenceSearchOptions
                {
                    VanguardTriggerPressureWeight = 20d,
                    MaxCandidates = 4
                }
            };
            TriggerProbabilityEngine.TryCalculate(10, 10, 1, out TriggerProbabilityResult certainTrigger);

            TriggerRiskAttackChoiceResult withoutTrigger = TriggerRiskAttackChoice.Choose(
                state,
                0,
                repository,
                null,
                options);
            TriggerRiskAttackChoiceResult withTrigger = TriggerRiskAttackChoice.Choose(
                state,
                0,
                repository,
                certainTrigger,
                options);

            Assert.AreEqual("high_power_first", withoutTrigger.CandidateId);
            Assert.AreEqual(GameZone.Vanguard, withTrigger.Attackers[0].Zone);
            Assert.Greater(withTrigger.TriggerPressureContribution, withoutTrigger.TriggerPressureContribution);
            Assert.IsTrue(withTrigger.UsesProbabilityAsPlanningSignal);
            Assert.IsFalse(withTrigger.AppliesTriggerOutcome);
        }

        [Test]
        public void ChoiceDoesNotMutateStateOrApplyTriggerOutcome()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            string before = state.ToJson();
            TriggerProbabilityEngine.TryCalculate(10, 10, 1, out TriggerProbabilityResult certainTrigger);

            TriggerRiskAttackChoiceResult result = TriggerRiskAttackChoice.Choose(
                state,
                0,
                CreatePowerRepository(),
                certainTrigger);
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.IsFalse(result.AppliesTriggerOutcome);
            Assert.AreEqual(0, state.event_log.Count);
            Assert.AreEqual(0, state.GetPlayer(0).vanguard[0].power_delta);
        }

        [Test]
        public void InvalidProbabilityFallsBackToZeroTriggerRisk()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            TriggerProbabilityEngine.TryCalculate(0, 0, 1, out TriggerProbabilityResult invalidProbability);

            TriggerRiskAttackChoiceResult result = TriggerRiskAttackChoice.Choose(
                state,
                0,
                CreatePowerRepository(),
                invalidProbability);

            Assert.IsTrue(result.HasChoice);
            Assert.AreEqual(0d, result.TriggerRisk, 0.0000001d);
            Assert.IsFalse(result.UsesProbabilityAsPlanningSignal);
            Assert.AreEqual(0d, result.TriggerPressureContribution, 0.0000001d);
        }

        [Test]
        public void HiddenAttackersAreSkippedAndReasonDoesNotLeakHiddenIds()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: true);

            TriggerRiskAttackChoiceResult result = TriggerRiskAttackChoice.Choose(
                state,
                0,
                CreatePowerRepository());

            Assert.IsTrue(result.HasChoice);
            Assert.IsFalse(result.Reason.Contains("HIDDEN-SECRET"));
            for (int i = 0; i < result.Attackers.Count; i++)
            {
                Assert.AreNotEqual(GameStateViewFactory.HiddenCardId, result.Attackers[i].CardId);
                Assert.AreNotEqual("hidden-rg", result.Attackers[i].CardInstanceId);
            }
        }

        [Test]
        public void ChoiceIsDeterministic()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            FakeCardRepository repository = CreatePowerRepository();
            TriggerProbabilityEngine.TryCalculate(10, 8, 2, out TriggerProbabilityResult triggerProbability);

            TriggerRiskAttackChoiceResult first = TriggerRiskAttackChoice.Choose(
                state,
                0,
                repository,
                triggerProbability);
            TriggerRiskAttackChoiceResult second = TriggerRiskAttackChoice.Choose(
                state,
                0,
                repository,
                triggerProbability);

            Assert.AreEqual(first.CandidateId, second.CandidateId);
            Assert.AreEqual(first.TotalScore, second.TotalScore, 0.0000001d);
            Assert.AreEqual(first.Reason, second.Reason);
        }

        private static GameState CreateBattleState(bool includeHiddenRearGuard)
        {
            var player = new PlayerGameState
            {
                player_id = "p1",
                vanguard = new List<GameCardInstance>
                {
                    new GameCardInstance("vg", "VG", 0)
                },
                rear_guard = new List<GameCardInstance>
                {
                    new GameCardInstance("rg-high", "RG-HIGH", 0)
                }
            };

            if (includeHiddenRearGuard)
            {
                player.rear_guard.Add(new GameCardInstance(
                    "hidden-rg",
                    GameStateViewFactory.HiddenCardId,
                    0,
                    false));
            }

            return new GameState
            {
                phase = GamePhase.Battle,
                players = new List<PlayerGameState> { player }
            };
        }

        private static FakeCardRepository CreatePowerRepository()
        {
            var repository = new FakeCardRepository();
            repository.Add("VG", 13000);
            repository.Add("RG-HIGH", 18000);
            return repository;
        }

        private sealed class FakeCardRepository : ICardRepository
        {
            private readonly Dictionary<string, CardDetail> cards = new Dictionary<string, CardDetail>();

            public void Add(string cardId, int? power)
            {
                cards[cardId] = new CardDetail
                {
                    CardId = cardId,
                    Power = power
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
