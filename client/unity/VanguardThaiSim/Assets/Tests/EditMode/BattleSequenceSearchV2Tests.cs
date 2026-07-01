using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class BattleSequenceSearchV2Tests
    {
        [Test]
        public void GuardEstimateAddsPressureContribution()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            OpponentGuardEstimate guardEstimate = CreateGuardEstimate(expectedShield: 5000, maximumShield: 15000);

            IReadOnlyList<BattleSequenceV2Candidate> candidates = BattleSequenceSearchV2.Search(
                state,
                0,
                CreatePowerRepository(),
                guardEstimate);

            Assert.Greater(candidates.Count, 0);
            Assert.Greater(candidates[0].GuardPressureContribution, 0d);
            Assert.Greater(candidates[0].TotalScore, candidates[0].BaseScore);
        }

        [Test]
        public void TriggerRiskIsCarriedIntoV2Candidates()
        {
            GameState state = CreateVanguardOnlyState();
            TriggerProbabilityEngine.TryCalculate(10, 8, 2, out TriggerProbabilityResult triggerProbability);

            IReadOnlyList<BattleSequenceV2Candidate> candidates = BattleSequenceSearchV2.Search(
                state,
                0,
                CreatePowerRepository(),
                null,
                triggerProbability);

            Assert.AreEqual(1, candidates.Count);
            Assert.Greater(candidates[0].TriggerRisk, 0d);
            Assert.Greater(candidates[0].TriggerPressureContribution, 0d);
        }

        [Test]
        public void HiddenAttackersAreSkipped()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: true);

            IReadOnlyList<BattleSequenceV2Candidate> candidates = BattleSequenceSearchV2.Search(
                state,
                0,
                CreatePowerRepository(),
                CreateGuardEstimate(expectedShield: 5000, maximumShield: 15000));

            Assert.Greater(candidates.Count, 0);
            foreach (BattleSequenceV2Candidate candidate in candidates)
            {
                foreach (BattleAttackCandidate attacker in candidate.Attackers)
                {
                    Assert.AreNotEqual(GameStateViewFactory.HiddenCardId, attacker.CardId);
                    Assert.AreNotEqual("hidden-rg", attacker.CardInstanceId);
                }
            }
        }

        [Test]
        public void SearchIsDeterministicAndDoesNotMutateState()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            string before = state.ToJson();
            OpponentGuardEstimate guardEstimate = CreateGuardEstimate(expectedShield: 5000, maximumShield: 15000);
            TriggerProbabilityEngine.TryCalculate(10, 8, 2, out TriggerProbabilityResult triggerProbability);

            IReadOnlyList<BattleSequenceV2Candidate> first = BattleSequenceSearchV2.Search(
                state,
                0,
                CreatePowerRepository(),
                guardEstimate,
                triggerProbability);
            IReadOnlyList<BattleSequenceV2Candidate> second = BattleSequenceSearchV2.Search(
                state,
                0,
                CreatePowerRepository(),
                guardEstimate,
                triggerProbability);
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].CandidateId, second[i].CandidateId);
                Assert.AreEqual(first[i].TotalScore, second[i].TotalScore, 0.0000001d);
                Assert.AreEqual(first[i].Explanation, second[i].Explanation);
            }
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
                    new GameCardInstance("rg-high", "RG-HIGH", 0),
                    new GameCardInstance("rg-low", "RG-LOW", 0)
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

        private static GameState CreateVanguardOnlyState()
        {
            return new GameState
            {
                phase = GamePhase.Battle,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "VG", 0)
                        }
                    }
                }
            };
        }

        private static OpponentGuardEstimate CreateGuardEstimate(int expectedShield, int maximumShield)
        {
            return new OpponentGuardEstimate(
                1,
                2,
                0,
                2,
                0,
                expectedShield,
                maximumShield,
                0.35d,
                "test estimate");
        }

        private static FakeCardRepository CreatePowerRepository()
        {
            var repository = new FakeCardRepository();
            repository.Add("VG", 13000);
            repository.Add("RG-HIGH", 18000);
            repository.Add("RG-LOW", 8000);
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
