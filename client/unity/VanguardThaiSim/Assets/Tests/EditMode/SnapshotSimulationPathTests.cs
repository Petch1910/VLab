using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class SnapshotSimulationPathTests
    {
        [Test]
        public void SimulateSingleAppliesActionOnlyToBranch()
        {
            GameState live = CreateGameState();
            string liveBefore = live.ToJson();
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw);

            SnapshotSimulationPathResult result = SnapshotSimulationPath.SimulateSingle(live, draw);
            GameState branch = result.RestoreBranchState();

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.live_unchanged);
            Assert.AreEqual(liveBefore, live.ToJson());
            Assert.AreEqual(0, live.event_log.Count);
            Assert.AreEqual(1, live.GetPlayer(0).deck.Count);
            Assert.AreEqual(1, live.GetPlayer(0).hand.Count);
            Assert.AreEqual(1, branch.event_log.Count);
            Assert.AreEqual(0, branch.GetPlayer(0).deck.Count);
            Assert.AreEqual(2, branch.GetPlayer(0).hand.Count);
        }

        [Test]
        public void SequentialActionsApplyToBranchInOrder()
        {
            GameState live = CreateGameState();
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw);
            LegalGameAction setMain = FirstPhase(RulesCore.GetLegalActions(live, 0), GamePhase.Main);

            SnapshotSimulationPathResult result = SnapshotSimulationPath.Simulate(
                live,
                new List<LegalGameAction> { draw, setMain });
            GameState branch = result.RestoreBranchState();

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.accepted_action_count);
            Assert.AreEqual(2, result.branch_event_count);
            Assert.AreEqual(GamePhase.Main, branch.phase);
            Assert.AreEqual(0, live.event_log.Count);
            Assert.AreEqual(GamePhase.Mulligan, live.phase);
        }

        [Test]
        public void RejectedActionReturnsBranchResultWithoutMutatingLive()
        {
            GameState live = CreateGameState();
            string liveBefore = live.ToJson();
            var illegal = new LegalGameAction
            {
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                card_instance_id = "missing",
                from_zone = GameZone.Hand,
                to_zone = GameZone.Vanguard
            };

            SnapshotSimulationPathResult result = SnapshotSimulationPath.SimulateSingle(live, illegal);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.rejection_reason.StartsWith(SnapshotSimulationRejectionReasons.ActionRejected));
            Assert.IsTrue(result.live_unchanged);
            Assert.AreEqual(liveBefore, live.ToJson());
            Assert.AreEqual(0, result.accepted_action_count);
            Assert.AreEqual(0, result.branch_event_count);
            Assert.AreEqual(1, result.action_results.Count);
            Assert.IsFalse(result.action_results[0].accepted);
        }

        [Test]
        public void MissingInputsRejectCleanly()
        {
            GameState live = CreateGameState();

            SnapshotSimulationPathResult missingLive =
                SnapshotSimulationPath.Simulate(null, new List<LegalGameAction>());
            SnapshotSimulationPathResult missingActions =
                SnapshotSimulationPath.Simulate(live, null);

            Assert.IsFalse(missingLive.accepted);
            Assert.AreEqual(SnapshotSimulationRejectionReasons.LiveStateMissing, missingLive.rejection_reason);
            Assert.IsFalse(missingActions.accepted);
            Assert.AreEqual(SnapshotSimulationRejectionReasons.ActionsMissing, missingActions.rejection_reason);
        }

        [Test]
        public void ResultRoundTripsJson()
        {
            GameState live = CreateGameState();
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw);

            SnapshotSimulationPathResult result = SnapshotSimulationPath.SimulateSingle(live, draw);
            SnapshotSimulationPathResult roundTrip =
                SnapshotSimulationPathResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.snapshot_id, roundTrip.snapshot_id);
            Assert.AreEqual(result.accepted_action_count, roundTrip.accepted_action_count);
            Assert.AreEqual(result.branch_event_count, roundTrip.branch_event_count);
            Assert.AreEqual(result.action_results.Count, roundTrip.action_results.Count);
            Assert.AreEqual(result.action_results[0].action_type, roundTrip.action_results[0].action_type);
            Assert.AreEqual(2, roundTrip.RestoreBranchState().GetPlayer(0).hand.Count);
        }

        private static LegalGameAction FirstAction(
            IReadOnlyList<LegalGameAction> actions,
            GameActionType actionType)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == actionType)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing action " + actionType);
            return null;
        }

        private static LegalGameAction FirstPhase(
            IReadOnlyList<LegalGameAction> actions,
            GamePhase phase)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == GameActionType.SetPhase &&
                    actions[i].phase == phase)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing phase " + phase);
            return null;
        }

        private static GameState CreateGameState()
        {
            return new GameState
            {
                game_id = "snapshot-sim-game",
                format = "D",
                random_seed = 51,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-deck-1", "CARD-DECK-1", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-hand-1", "CARD-HAND-1", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }
    }
}
