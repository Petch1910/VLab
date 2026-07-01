using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    [Serializable]
    public sealed class BotAutomationSafetyRegressionCheck
    {
        public string check_id;
        public bool passed;
        public string summary;
    }

    [Serializable]
    public sealed class BotAutomationSafetyRegressionReport
    {
        public int schema_version = 1;
        public string milestone = "M26-07";
        public bool accepted;
        public int check_count;
        public int passed_count;
        public int failed_count;
        public string summary;
        public List<BotAutomationSafetyRegressionCheck> checks =
            new List<BotAutomationSafetyRegressionCheck>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static BotAutomationSafetyRegressionReport FromJson(string json)
        {
            BotAutomationSafetyRegressionReport report =
                JsonUtility.FromJson<BotAutomationSafetyRegressionReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Bot automation safety regression report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (checks == null)
            {
                checks = new List<BotAutomationSafetyRegressionCheck>();
            }
        }
    }

    public static class BotAutomationSafetyRegressionGate
    {
        public const string HiddenStateNoLeakCheck = "hidden_state_no_leak";
        public const string SnapshotSimulationNoLiveMutationCheck = "snapshot_simulation_no_live_mutation";
        public const string ReplayDeterminismCheck = "replay_determinism";

        public static BotAutomationSafetyRegressionReport RunDefault()
        {
            var checks = new List<BotAutomationSafetyRegressionCheck>();
            HiddenStateViewHardeningVerificationResult hidden =
                HiddenStateViewHardeningVerifier.Verify(CreateHiddenStateFixture());
            checks.Add(Check(
                HiddenStateNoLeakCheck,
                hidden.accepted,
                hidden.summary));

            GameState simulationState = CreateCommandFixture("m26-07-snapshot");
            LegalGameAction draw = FirstLegalAction(simulationState, GameActionType.Draw);
            SnapshotSimulationPathResult simulation =
                SnapshotSimulationPath.SimulateSingle(simulationState, draw);
            checks.Add(Check(
                SnapshotSimulationNoLiveMutationCheck,
                simulation.accepted && simulation.live_unchanged,
                simulation.summary));

            GameState initial = CreateCommandFixture("m26-07-replay");
            GameState live = CloneState(initial);
            RulesCore.ExecuteOrThrow(live, FirstLegalAction(live, GameActionType.Draw));
            RulesCore.ExecuteOrThrow(live, FirstMove(live, GameZone.Hand, GameZone.Vanguard));
            ReplayDeterminismVerificationResult replay =
                ReplayDeterminismVerifier.Verify(initial, live, live.event_log);
            checks.Add(Check(
                ReplayDeterminismCheck,
                replay.accepted,
                replay.summary));

            return Build(checks);
        }

        public static BotAutomationSafetyRegressionReport Build(
            IReadOnlyList<BotAutomationSafetyRegressionCheck> checks)
        {
            var report = new BotAutomationSafetyRegressionReport();
            if (checks != null)
            {
                for (int i = 0; i < checks.Count; i++)
                {
                    if (checks[i] != null)
                    {
                        report.checks.Add(new BotAutomationSafetyRegressionCheck
                        {
                            check_id = checks[i].check_id ?? string.Empty,
                            passed = checks[i].passed,
                            summary = checks[i].summary ?? string.Empty
                        });
                    }
                }
            }

            RefreshCounts(report);
            return report;
        }

        private static BotAutomationSafetyRegressionCheck Check(
            string checkId,
            bool passed,
            string summary)
        {
            return new BotAutomationSafetyRegressionCheck
            {
                check_id = checkId,
                passed = passed,
                summary = summary ?? string.Empty
            };
        }

        private static void RefreshCounts(BotAutomationSafetyRegressionReport report)
        {
            report.EnsureLists();
            report.check_count = report.checks.Count;
            report.passed_count = 0;
            report.failed_count = 0;
            for (int i = 0; i < report.checks.Count; i++)
            {
                if (report.checks[i] != null && report.checks[i].passed)
                {
                    report.passed_count++;
                }
                else
                {
                    report.failed_count++;
                }
            }

            report.accepted = report.check_count == 3 && report.failed_count == 0;
            report.summary = report.accepted
                ? "Bot automation safety regressions passed."
                : "Bot automation safety regressions failed.";
        }

        private static GameState CreateHiddenStateFixture()
        {
            return new GameState
            {
                game_id = "m26-07-hidden",
                format = "D",
                random_seed = 2607,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p0",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-deck-secret", "P0-DECK", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-hand-open", "P0-HAND", 0)
                        },
                        ride_deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-ride-open", "P0-RIDE", 0)
                        },
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-vg-open", "P0-VG", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p1",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-deck-secret", "P1-DECK", 1)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-hand-open", "P1-HAND", 1)
                        },
                        ride_deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-ride-open", "P1-RIDE", 1)
                        },
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-vg-open", "P1-VG", 1)
                        }
                    }
                },
                event_log = new List<GameEvent>
                {
                    new GameEvent
                    {
                        event_id = "m26-07-opponent-hand",
                        actor_index = 1,
                        card_instance_id = "p1-hand-open",
                        from_zone = GameZone.Hand,
                        to_zone = GameZone.Vanguard
                    },
                    new GameEvent
                    {
                        event_id = "m26-07-own-hand",
                        actor_index = 0,
                        card_instance_id = "p0-hand-open",
                        from_zone = GameZone.Hand,
                        to_zone = GameZone.Vanguard
                    }
                },
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }

        private static GameState CreateCommandFixture(string gameId)
        {
            return new GameState
            {
                game_id = gameId,
                format = "D",
                random_seed = 2607,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p0",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-deck-1", "CARD-DECK-1", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-hand-1", "CARD-HAND-1", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p1",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-deck-1", "CARD-DECK-1", 1)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-hand-1", "CARD-HAND-1", 1)
                        }
                    }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }

        private static LegalGameAction FirstLegalAction(GameState state, GameActionType actionType)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, 0);
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == actionType)
                {
                    return actions[i];
                }
            }

            throw new InvalidOperationException("Missing legal action " + actionType + ".");
        }

        private static LegalGameAction FirstMove(GameState state, GameZone from, GameZone to)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, 0);
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == GameActionType.MoveCard &&
                    actions[i].from_zone == from &&
                    actions[i].to_zone == to)
                {
                    return actions[i];
                }
            }

            throw new InvalidOperationException("Missing legal move " + from + " to " + to + ".");
        }

        private static GameState CloneState(GameState state)
        {
            return GameState.FromJson(state.ToJson(false));
        }
    }
}
