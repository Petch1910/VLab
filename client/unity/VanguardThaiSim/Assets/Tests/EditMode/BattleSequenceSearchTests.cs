using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class BattleSequenceSearchTests
    {
        [Test]
        public void HighPowerFirstSequenceRanksAboveLowPowerFirst()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: false);
            FakeCardRepository repository = CreatePowerRepository();

            IReadOnlyList<BattleSequenceCandidate> candidates = BattleSequenceSearch.Search(
                state,
                0,
                repository);

            Assert.GreaterOrEqual(candidates.Count, 2);
            Assert.AreEqual("high_power_first", candidates[0].CandidateId);
            Assert.AreEqual("RG-HIGH", candidates[0].Attackers[0].CardId);
            Assert.Greater(candidates[0].TotalScore, FindCandidate(candidates, "low_power_first").TotalScore);
        }

        [Test]
        public void TriggerProbabilityIncreasesVanguardCandidateScore()
        {
            GameState state = CreateVanguardOnlyState();
            FakeCardRepository repository = CreatePowerRepository();
            TriggerProbabilityEngine.TryCalculate(50, 16, 2, out TriggerProbabilityResult triggerProbability);

            IReadOnlyList<BattleSequenceCandidate> withoutTrigger = BattleSequenceSearch.Search(
                state,
                0,
                repository,
                null,
                null);
            IReadOnlyList<BattleSequenceCandidate> withTrigger = BattleSequenceSearch.Search(
                state,
                0,
                repository,
                null,
                triggerProbability);

            Assert.AreEqual(1, withoutTrigger.Count);
            Assert.AreEqual(1, withTrigger.Count);
            Assert.Greater(withTrigger[0].TotalScore, withoutTrigger[0].TotalScore);
            Assert.Greater(withTrigger[0].TriggerPressureContribution, 0d);
        }

        [Test]
        public void HiddenAttackersAreSkipped()
        {
            GameState state = CreateBattleState(includeHiddenRearGuard: true);
            FakeCardRepository repository = CreatePowerRepository();

            IReadOnlyList<BattleSequenceCandidate> candidates = BattleSequenceSearch.Search(
                state,
                0,
                repository);

            Assert.Greater(candidates.Count, 0);
            foreach (BattleSequenceCandidate candidate in candidates)
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
            FakeCardRepository repository = CreatePowerRepository();
            string before = state.ToJson();

            IReadOnlyList<BattleSequenceCandidate> first = BattleSequenceSearch.Search(state, 0, repository);
            IReadOnlyList<BattleSequenceCandidate> second = BattleSequenceSearch.Search(state, 0, repository);
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].CandidateId, second[i].CandidateId);
                Assert.AreEqual(first[i].TotalScore, second[i].TotalScore, 0.0000001d);
            }
        }

        private static BattleSequenceCandidate FindCandidate(
            IReadOnlyList<BattleSequenceCandidate> candidates,
            string candidateId)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].CandidateId == candidateId)
                {
                    return candidates[i];
                }
            }

            Assert.Fail("Missing candidate: " + candidateId);
            return null;
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
