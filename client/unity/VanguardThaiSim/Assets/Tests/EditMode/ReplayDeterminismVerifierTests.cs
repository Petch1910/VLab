using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class ReplayDeterminismVerifierTests
    {
        [Test]
        public void VerifierAcceptsSupportedRulesCoreCommandScript()
        {
            GameState initial = CreateGameState();
            GameState live = CloneState(initial);
            RunSupportedCommandScript(live);

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, live, live.event_log);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(4, result.event_count);
            Assert.AreEqual(result.expected_final_state_hash, result.replayed_final_state_hash);
            Assert.IsTrue(result.summary.Contains("4 event"));
        }

        [Test]
        public void VerifierAcceptsResourceFlipCommandScript()
        {
            GameState initial = CreateGameState();
            initial.GetPlayer(0).damage.Add(new GameCardInstance("p1-damage-1", "DAMAGE-1", 0, true));
            GameState live = CloneState(initial);

            RulesCore.ExecuteOrThrow(
                live,
                FirstResource(RulesCore.GetLegalActions(live, 0), GameResourceOperationType.CounterBlast));

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, live, live.event_log);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.event_count);
            Assert.IsFalse(live.GetPlayer(0).damage[0].face_up);
        }

        [Test]
        public void VerifierAcceptsRideToSoulCommandScript()
        {
            GameState initial = CreateGameState();
            initial.GetPlayer(0).hand.Add(new GameCardInstance("p1-hand-2", "CARD-HAND-2", 0));
            GameState live = CloneState(initial);

            RulesCore.ExecuteOrThrow(
                live,
                FirstMove(RulesCore.GetLegalActions(live, 0), GameZone.Hand, GameZone.Vanguard));
            RulesCore.ExecuteOrThrow(
                live,
                FirstMove(RulesCore.GetLegalActions(live, 0), GameZone.Hand, GameZone.Vanguard));

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, live, live.event_log);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.event_count);
            Assert.AreEqual(1, live.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(1, live.GetPlayer(0).CountZone(GameZone.Soul));
        }

        [Test]
        public void VerifierRejectsDivergentFinalState()
        {
            GameState initial = CreateGameState();
            GameState live = CloneState(initial);
            RunSupportedCommandScript(live);
            GameState wrongFinal = CloneState(live);
            wrongFinal.phase = GamePhase.End;

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, wrongFinal, live.event_log);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(ReplayDeterminismRejectionReasons.FinalStateMismatch, result.rejection_reason);
            Assert.AreNotEqual(result.expected_final_state_hash, result.replayed_final_state_hash);
        }

        [Test]
        public void VerifierRejectsUnsupportedReplayEvent()
        {
            GameState initial = CreateGameState();
            GameState expected = CloneState(initial);
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    event_id = "event-bad",
                    action_type = (GameActionType)999,
                    actor_index = 0
                }
            };

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, expected, events);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.rejection_reason.StartsWith(ReplayDeterminismRejectionReasons.ReplayApplyFailed));
            Assert.AreEqual(1, result.event_count);
        }

        [Test]
        public void VerifierDoesNotMutateSourceStatesOrEvents()
        {
            GameState initial = CreateGameState();
            GameState live = CloneState(initial);
            RunSupportedCommandScript(live);
            GameStateNoMutationSnapshot initialSnapshot = NoMutationSnapshot.Capture(initial);
            GameStateNoMutationSnapshot liveSnapshot = NoMutationSnapshot.Capture(live);
            string firstEventBefore = UnityEngine.JsonUtility.ToJson(live.event_log[0], false);

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, live, live.event_log);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(initialSnapshot.Matches(initial));
            Assert.IsTrue(liveSnapshot.Matches(live));
            Assert.AreEqual(firstEventBefore, UnityEngine.JsonUtility.ToJson(live.event_log[0], false));
        }

        [Test]
        public void VerificationResultRoundTripsJson()
        {
            GameState initial = CreateGameState();
            GameState live = CloneState(initial);
            RunSupportedCommandScript(live);

            ReplayDeterminismVerificationResult result =
                ReplayDeterminismVerifier.Verify(initial, live, live.event_log);
            ReplayDeterminismVerificationResult roundTrip =
                ReplayDeterminismVerificationResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.replay_id, roundTrip.replay_id);
            Assert.AreEqual(result.event_count, roundTrip.event_count);
            Assert.AreEqual(result.expected_final_state_hash, roundTrip.expected_final_state_hash);
        }

        [Test]
        public void VerifierRejectsMissingStates()
        {
            ReplayDeterminismVerificationResult missingInitial =
                ReplayDeterminismVerifier.Verify(null, CreateGameState(), null);
            ReplayDeterminismVerificationResult missingFinal =
                ReplayDeterminismVerifier.Verify(CreateGameState(), null, null);

            Assert.IsFalse(missingInitial.accepted);
            Assert.AreEqual(ReplayDeterminismRejectionReasons.InitialStateMissing, missingInitial.rejection_reason);
            Assert.IsFalse(missingFinal.accepted);
            Assert.AreEqual(ReplayDeterminismRejectionReasons.FinalStateMissing, missingFinal.rejection_reason);
        }

        private static void RunSupportedCommandScript(GameState state)
        {
            RulesCore.ExecuteOrThrow(state, FirstAction(RulesCore.GetLegalActions(state, 0), GameActionType.Draw));
            RulesCore.ExecuteOrThrow(state, FirstMove(RulesCore.GetLegalActions(state, 0), GameZone.Hand, GameZone.Vanguard));
            RulesCore.ExecuteOrThrow(state, FirstPhase(RulesCore.GetLegalActions(state, 0), GamePhase.Main));
            RulesCore.ExecuteOrThrow(state, FirstGift(RulesCore.GetLegalActions(state, 0), GiftMarkerType.Force));
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

        private static LegalGameAction FirstMove(
            IReadOnlyList<LegalGameAction> actions,
            GameZone from,
            GameZone to)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == GameActionType.MoveCard &&
                    actions[i].from_zone == from &&
                    actions[i].to_zone == to)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing move " + from + " to " + to);
            return null;
        }

        private static LegalGameAction FirstPhase(
            IReadOnlyList<LegalGameAction> actions,
            GamePhase phase)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == GameActionType.SetPhase && actions[i].phase == phase)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing phase " + phase);
            return null;
        }

        private static LegalGameAction FirstGift(
            IReadOnlyList<LegalGameAction> actions,
            GiftMarkerType markerType)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == GameActionType.AddGiftMarker &&
                    actions[i].gift_marker_type == markerType)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing gift marker " + markerType);
            return null;
        }

        private static LegalGameAction FirstResource(
            IReadOnlyList<LegalGameAction> actions,
            GameResourceOperationType operationType)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == GameActionType.ResourceFlip &&
                    actions[i].resource_operation_type == operationType)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing resource operation " + operationType);
            return null;
        }

        private static GameState CreateGameState()
        {
            return new GameState
            {
                game_id = "replay-determinism-game",
                format = "D",
                random_seed = 21,
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
                        player_id = "p2",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p2-deck-1", "CARD-DECK-1", 1)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p2-hand-1", "CARD-HAND-1", 1)
                        }
                    }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }

        private static GameState CloneState(GameState state)
        {
            return GameState.FromJson(state.ToJson(false));
        }
    }
}
