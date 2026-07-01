using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class BotDebugTraceTests
    {
        [Test]
        public void TraceIncludesSelectedActionAndRankedLines()
        {
            GameState state = CreateState();

            BotDebugTrace trace = BotDebugTracer.CreatePlaybookTrace(
                state,
                0,
                new FakeCardRepository(),
                CreateLibrary());

            Assert.Greater(trace.candidate_count, 0);
            Assert.Greater(trace.lines.Count, 0);
            Assert.IsTrue(trace.lines[0].selected);
            Assert.AreEqual(trace.selected_score, trace.lines[0].total_score, 0.0000001d);
            Assert.IsTrue(trace.selected_action_summary.Contains("playbook=youth_line"));
            Assert.IsTrue(trace.sanitized);
        }

        [Test]
        public void TraceJsonRoundTrips()
        {
            BotDebugTrace trace = BotDebugTracer.CreatePlaybookTrace(
                CreateState(),
                0,
                new FakeCardRepository(),
                CreateLibrary());

            BotDebugTrace roundTrip = BotDebugTrace.FromJson(trace.ToJson(false));

            Assert.AreEqual(trace.trace_id, roundTrip.trace_id);
            Assert.AreEqual(trace.candidate_count, roundTrip.candidate_count);
            Assert.AreEqual(trace.lines.Count, roundTrip.lines.Count);
            Assert.AreEqual(trace.lines[0].action_summary, roundTrip.lines[0].action_summary);
        }

        [Test]
        public void TraceDoesNotMutateStateOrLeakSecrets()
        {
            GameState state = CreateState();
            state.GetPlayer(1).hand.Add(new GameCardInstance("opp-secret", "OPP-HAND-SECRET", 1));
            state.GetPlayer(0).deck.Add(new GameCardInstance("top-secret", "TOP-DECK-SECRET", 0, false));
            string before = state.ToJson();

            BotDebugTrace trace = BotDebugTracer.CreatePlaybookTrace(
                state,
                0,
                new FakeCardRepository(),
                CreateLibrary());
            string after = state.ToJson();
            string json = trace.ToJson(false);

            Assert.AreEqual(before, after);
            Assert.IsFalse(json.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(json.Contains("TOP-DECK-SECRET"));
            Assert.IsFalse(json.Contains("CALL-A"));
        }

        [Test]
        public void TraceIsDeterministic()
        {
            GameState state = CreateState();
            BotPlaybookLibrary library = CreateLibrary();

            BotDebugTrace first = BotDebugTracer.CreatePlaybookTrace(
                state,
                0,
                new FakeCardRepository(),
                library);
            BotDebugTrace second = BotDebugTracer.CreatePlaybookTrace(
                state,
                0,
                new FakeCardRepository(),
                library);

            Assert.AreEqual(first.ToJson(false), second.ToJson(false));
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "trace-test",
                turn_number = 2,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "RIDELINE-VG", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("call-a-inst", "CALL-A", 0),
                            new GameCardInstance("call-z-inst", "CALL-Z", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                }
            };
        }

        private static BotPlaybookLibrary CreateLibrary()
        {
            return new BotPlaybookLibrary
            {
                playbooks = new List<BotPlaybook>
                {
                    new BotPlaybook
                    {
                        playbook_id = "youth_line",
                        display_name = "Youth Line",
                        preferred_profile = BotProfileType.Aggro,
                        rideline_card_ids = new List<string>
                        {
                            "RIDELINE-VG"
                        },
                        priority_call_card_ids = new List<string>
                        {
                            "CALL-A"
                        }
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
