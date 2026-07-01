using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class BotPlaybookTests
    {
        [Test]
        public void MatchesPlaybookFromVisibleRidelineCard()
        {
            BotPlaybookLibrary library = CreateLibrary();
            GameState state = CreateStateWithVanguard("RIDELINE-VG");

            BotPlaybook match = library.MatchFromState(state, 0);

            Assert.AreEqual("youth_line", match.playbook_id);
            Assert.AreEqual(BotProfileType.Aggro, match.preferred_profile);
        }

        [Test]
        public void NoMatchReturnsDefaultBalancedPlaybook()
        {
            BotPlaybookLibrary library = CreateLibrary();
            GameState state = CreateStateWithVanguard("UNKNOWN-VG");

            BotPlaybook match = library.MatchFromState(state, 0);

            Assert.AreEqual("default_balanced", match.playbook_id);
            Assert.AreEqual(BotProfileType.Balanced, match.preferred_profile);
        }

        [Test]
        public void PriorityCallIdsPreserveDeterministicOrder()
        {
            BotPlaybookLibrary library = CreateLibrary();
            BotPlaybook match = library.MatchFromVisibleCards(new[] { "RIDELINE-G2", "RIDELINE-VG" });

            Assert.AreEqual(3, match.priority_call_card_ids.Count);
            Assert.AreEqual("CALL-A", match.priority_call_card_ids[0]);
            Assert.AreEqual("CALL-B", match.priority_call_card_ids[1]);
            Assert.AreEqual("CALL-C", match.priority_call_card_ids[2]);
        }

        [Test]
        public void PlaybookLibraryJsonRoundTrips()
        {
            BotPlaybookLibrary library = CreateLibrary();
            string json = library.ToJson();
            BotPlaybookLibrary roundTrip = BotPlaybookLibrary.FromJson(json);

            BotPlaybook match = roundTrip.MatchFromVisibleCards(new[] { "RIDELINE-VG" });

            Assert.AreEqual("youth_line", match.playbook_id);
            Assert.AreEqual(3, match.priority_call_card_ids.Count);
            Assert.AreEqual("open with vanguard pressure", match.battle_plan_notes[0]);
        }

        [Test]
        public void MatchingDoesNotMutateState()
        {
            BotPlaybookLibrary library = CreateLibrary();
            GameState state = CreateStateWithVanguard("RIDELINE-VG");
            string before = state.ToJson();

            library.MatchFromState(state, 0);
            library.MatchFromState(state, 0);
            string after = state.ToJson();

            Assert.AreEqual(before, after);
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
                            "RIDELINE-G1",
                            "RIDELINE-G2",
                            "RIDELINE-VG"
                        },
                        mulligan_keep_card_ids = new List<string>
                        {
                            "RIDELINE-G1"
                        },
                        priority_call_card_ids = new List<string>
                        {
                            "CALL-A",
                            "CALL-B",
                            "CALL-C"
                        },
                        battle_plan_notes = new List<string>
                        {
                            "open with vanguard pressure"
                        }
                    }
                }
            };
        }

        private static GameState CreateStateWithVanguard(string cardId)
        {
            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", cardId, 0)
                        }
                    }
                }
            };
        }
    }
}
