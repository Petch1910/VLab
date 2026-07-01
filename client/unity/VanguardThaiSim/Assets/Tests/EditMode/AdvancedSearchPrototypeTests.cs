using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class AdvancedSearchPrototypeTests
    {
        [Test]
        public void DefaultReadinessAllowsOnePlySearch()
        {
            GameState state = CreateState();

            AdvancedSearchPrototypeResult result = AdvancedSearchPrototype.Search(
                state,
                0,
                new FakeCardRepository());

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.readiness_allowed);
            Assert.Greater(result.considered_action_count, 0);
            Assert.Greater(result.candidates.Count, 0);
            Assert.IsTrue(result.candidates[0].selected);
            Assert.IsTrue(result.candidates[0].branch_accepted);
        }

        [Test]
        public void BlockedReadinessRejectsSearch()
        {
            GameState state = CreateState();
            IsmctsReadinessReport blocked = IsmctsReadinessGate.Evaluate(new List<IsmctsReadinessItem>
            {
                new IsmctsReadinessItem
                {
                    requirement_id = "hidden-state",
                    title = "Hidden state",
                    ready = false,
                    evidence = "blocked"
                }
            });

            AdvancedSearchPrototypeResult result = AdvancedSearchPrototype.Search(
                state,
                0,
                new FakeCardRepository(),
                blocked);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                AdvancedSearchPrototypeRejectionReasons.ReadinessGateBlocked,
                result.rejection_reason);
            Assert.IsFalse(result.readiness_allowed);
        }

        [Test]
        public void SearchDoesNotMutateStateOrLeakSecrets()
        {
            GameState state = CreateState();
            state.GetPlayer(1).hand.Add(new GameCardInstance("opp-secret", "OPP-HAND-SECRET", 1));
            state.GetPlayer(0).deck.Add(new GameCardInstance("top-secret", "TOP-DECK-SECRET", 0, false));
            string before = state.ToJson();

            AdvancedSearchPrototypeResult result = AdvancedSearchPrototype.Search(
                state,
                0,
                new FakeCardRepository());
            string after = state.ToJson();
            string json = result.ToJson(false);

            Assert.AreEqual(before, after);
            Assert.IsFalse(json.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(json.Contains("TOP-DECK-SECRET"));
        }

        [Test]
        public void ResultJsonRoundTrips()
        {
            AdvancedSearchPrototypeResult result = AdvancedSearchPrototype.Search(
                CreateState(),
                0,
                new FakeCardRepository());

            AdvancedSearchPrototypeResult roundTrip =
                AdvancedSearchPrototypeResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.search_id, roundTrip.search_id);
            Assert.AreEqual(result.candidates.Count, roundTrip.candidates.Count);
            Assert.AreEqual(result.candidates[0].action_summary, roundTrip.candidates[0].action_summary);
        }

        [Test]
        public void SearchIsDeterministic()
        {
            GameState state = CreateState();

            AdvancedSearchPrototypeResult first = AdvancedSearchPrototype.Search(
                state,
                0,
                new FakeCardRepository());
            AdvancedSearchPrototypeResult second = AdvancedSearchPrototype.Search(
                state,
                0,
                new FakeCardRepository());

            Assert.AreEqual(first.ToJson(false), second.ToJson(false));
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "advanced-search-test",
                turn_number = 2,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "VG", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("call-a", "CALL-A", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                }
            };
        }

        private sealed class FakeCardRepository : ICardRepository
        {
            public int CountCards()
            {
                return 0;
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
