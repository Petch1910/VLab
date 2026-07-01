using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class SnapshotRollbackVerifierTests
    {
        [Test]
        public void VerifierAcceptsBranchActionWithoutMutatingLiveState()
        {
            GameState live = CreateGameState();
            GameStateNoMutationSnapshot liveSnapshot = NoMutationSnapshot.Capture(live);
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw);

            SnapshotRollbackVerificationResult result =
                SnapshotRollbackVerifier.VerifyBranchIsolation(live, draw);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.live_unchanged);
            Assert.IsTrue(result.restore_matches_live_before);
            Assert.IsTrue(result.branch_changed);
            Assert.AreEqual(result.live_before_hash, result.live_after_hash);
            Assert.AreEqual(result.live_before_hash, result.restored_hash);
            Assert.AreNotEqual(result.live_before_hash, result.branch_after_hash);
            Assert.IsTrue(liveSnapshot.Matches(live));
            Assert.AreEqual(0, live.event_log.Count);
            Assert.AreEqual(1, live.GetPlayer(0).deck.Count);
            Assert.AreEqual(1, live.GetPlayer(0).hand.Count);
        }

        [Test]
        public void VerifierRejectsIllegalBranchActionWithoutMutatingLiveState()
        {
            GameState live = CreateGameState();
            GameStateNoMutationSnapshot liveSnapshot = NoMutationSnapshot.Capture(live);
            var illegal = new LegalGameAction
            {
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                card_instance_id = "missing",
                from_zone = GameZone.Hand,
                to_zone = GameZone.Vanguard
            };

            SnapshotRollbackVerificationResult result =
                SnapshotRollbackVerifier.VerifyBranchIsolation(live, illegal);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.rejection_reason.StartsWith(SnapshotRollbackRejectionReasons.BranchActionRejected));
            Assert.IsTrue(result.live_unchanged);
            Assert.IsFalse(result.branch_changed);
            Assert.IsTrue(liveSnapshot.Matches(live));
            Assert.AreEqual(0, live.event_log.Count);
        }

        [Test]
        public void VerifierRejectsMissingInputs()
        {
            GameState live = CreateGameState();
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw);

            SnapshotRollbackVerificationResult missingLive =
                SnapshotRollbackVerifier.VerifyBranchIsolation(null, draw);
            SnapshotRollbackVerificationResult missingAction =
                SnapshotRollbackVerifier.VerifyBranchIsolation(live, null);

            Assert.IsFalse(missingLive.accepted);
            Assert.AreEqual(SnapshotRollbackRejectionReasons.LiveStateMissing, missingLive.rejection_reason);
            Assert.IsFalse(missingAction.accepted);
            Assert.AreEqual(SnapshotRollbackRejectionReasons.BranchActionMissing, missingAction.rejection_reason);
        }

        [Test]
        public void SnapshotRestoreIsIndependentAfterLiveMutation()
        {
            GameState live = CreateGameState();
            GameStateSnapshot snapshot = GameStateSnapshot.Capture(live);
            RulesCore.ExecuteOrThrow(live, FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw));

            GameState restored = snapshot.Restore();

            Assert.AreEqual(1, restored.GetPlayer(0).deck.Count);
            Assert.AreEqual(1, restored.GetPlayer(0).hand.Count);
            Assert.AreEqual(0, restored.event_log.Count);
            Assert.AreEqual(0, live.GetPlayer(0).deck.Count);
            Assert.AreEqual(2, live.GetPlayer(0).hand.Count);
            Assert.AreEqual(1, live.event_log.Count);
        }

        [Test]
        public void VerificationResultRoundTripsJson()
        {
            GameState live = CreateGameState();
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw);

            SnapshotRollbackVerificationResult result =
                SnapshotRollbackVerifier.VerifyBranchIsolation(live, draw);
            SnapshotRollbackVerificationResult roundTrip =
                SnapshotRollbackVerificationResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.snapshot_id, roundTrip.snapshot_id);
            Assert.AreEqual(result.live_before_hash, roundTrip.live_before_hash);
            Assert.AreEqual(result.branch_changed, roundTrip.branch_changed);
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

        private static GameState CreateGameState()
        {
            return new GameState
            {
                game_id = "snapshot-rollback-game",
                format = "D",
                random_seed = 31,
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
    }
}
