using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class AbilityTriggerGameStateAdapterTests
    {
        [Test]
        public void MatchingRegistrationAppendsToCopiedGameStateQueue()
        {
            GameState state = CreateStateWithExistingQueue();

            PendingAutoAbilityQueue result =
                AbilityTriggerGameStateAdapter.CollectPendingQueue(
                    state,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnDraw")
                    });

            Assert.AreEqual("state-queue", result.queue_id);
            Assert.AreEqual(2, result.pending.Count);
            Assert.AreEqual("existing", result.pending[0].pending_id);
            Assert.AreEqual("CARD-reg-1", result.pending[1].source_card_id);
            Assert.AreEqual("OnDraw", result.pending[1].timing_event);
        }

        [Test]
        public void SourceGameStateQueueIsNotMutated()
        {
            GameState state = CreateStateWithExistingQueue();

            PendingAutoAbilityQueue result =
                AbilityTriggerGameStateAdapter.CollectPendingQueue(
                    state,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnDraw")
                    });

            Assert.AreEqual(1, state.pending_auto_abilities.pending.Count);
            Assert.AreEqual("existing", state.pending_auto_abilities.pending[0].pending_id);
            Assert.AreEqual(2, result.pending.Count);
        }

        [Test]
        public void NullStateUsesDefaultQueue()
        {
            PendingAutoAbilityQueue result =
                AbilityTriggerGameStateAdapter.CollectPendingQueue(
                    null,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnDraw")
                    });

            Assert.AreEqual("pending-auto-ability-queue", result.queue_id);
            Assert.AreEqual(1, result.pending.Count);
            Assert.AreEqual("CARD-reg-1", result.pending[0].source_card_id);
        }

        [Test]
        public void NullSourceQueueListRemainsUnchanged()
        {
            var state = new GameState
            {
                pending_auto_abilities = new PendingAutoAbilityQueue
                {
                    queue_id = "broken-queue",
                    pending = null
                }
            };

            PendingAutoAbilityQueue result =
                AbilityTriggerGameStateAdapter.CollectPendingQueue(
                    state,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnDraw")
                    });

            Assert.IsNull(state.pending_auto_abilities.pending);
            Assert.AreEqual("broken-queue", result.queue_id);
            Assert.AreEqual(1, result.pending.Count);
        }

        [Test]
        public void NonMatchingRegistrationReturnsCopiedQueueOnly()
        {
            GameState state = CreateStateWithExistingQueue();

            PendingAutoAbilityQueue result =
                AbilityTriggerGameStateAdapter.CollectPendingQueue(
                    state,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnMoveCard")
                    });

            Assert.AreEqual(1, result.pending.Count);
            Assert.AreEqual("existing", result.pending[0].pending_id);
            Assert.AreEqual(1, state.pending_auto_abilities.pending.Count);
        }

        [Test]
        public void CommitFromRulesCoreTimingEventAddsMatchingPendingAbilityToStateQueue()
        {
            GameState state = CreatePlayableState();
            RulesCommandResult commandResult =
                RulesCore.TryExecute(state, FirstAction(state, 0, GameActionType.Draw));

            AbilityTriggerGameStateAdapterResult result =
                AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent(
                    state,
                    commandResult.game_event,
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnDraw")
                    });

            Assert.IsTrue(commandResult.accepted);
            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.state_was_updated);
            Assert.AreEqual("event-000001", result.source_event_id);
            Assert.AreEqual("OnDraw", result.timing_event);
            Assert.AreEqual(0, result.before_count);
            Assert.AreEqual(1, result.after_count);
            Assert.AreEqual(1, result.added_count);
            Assert.AreEqual(1, state.pending_auto_abilities.pending.Count);
            Assert.AreEqual("OnDraw", state.pending_auto_abilities.pending[0].timing_event);
            Assert.AreEqual("CARD-reg-1", state.pending_auto_abilities.pending[0].source_card_id);
            Assert.AreEqual(1, state.event_log.Count);
        }

        [Test]
        public void CommitFromUnmatchedTimingEventLeavesStateQueueReferenceUnchanged()
        {
            GameState state = CreateStateWithExistingQueue();
            PendingAutoAbilityQueue originalQueue = state.pending_auto_abilities;

            AbilityTriggerGameStateAdapterResult result =
                AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent(
                    state,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnMoveCard")
                    });

            Assert.IsTrue(result.accepted);
            Assert.IsFalse(result.state_was_updated);
            Assert.AreEqual(1, result.before_count);
            Assert.AreEqual(1, result.after_count);
            Assert.AreEqual(0, result.added_count);
            Assert.AreSame(originalQueue, state.pending_auto_abilities);
            Assert.AreEqual(1, state.pending_auto_abilities.pending.Count);
            Assert.AreEqual("existing", state.pending_auto_abilities.pending[0].pending_id);
        }

        [Test]
        public void CommitFromTimingEventCopiesSourceQueueBeforeStateAssignment()
        {
            GameState state = CreateStateWithExistingQueue();
            PendingAutoAbilityQueue originalQueue = state.pending_auto_abilities;

            AbilityTriggerGameStateAdapterResult result =
                AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent(
                    state,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>
                    {
                        CreateRegistration("reg-1", "OnDraw")
                    });

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.state_was_updated);
            Assert.AreNotSame(originalQueue, state.pending_auto_abilities);
            Assert.AreEqual(1, originalQueue.pending.Count);
            Assert.AreEqual("existing", originalQueue.pending[0].pending_id);
            Assert.AreEqual(2, state.pending_auto_abilities.pending.Count);
            Assert.AreEqual("existing", state.pending_auto_abilities.pending[0].pending_id);
            Assert.AreEqual("OnDraw", state.pending_auto_abilities.pending[1].timing_event);
        }

        [Test]
        public void CommitFromTimingEventRejectsMissingInputsWithoutMutation()
        {
            GameState state = CreateStateWithExistingQueue();
            PendingAutoAbilityQueue originalQueue = state.pending_auto_abilities;

            AbilityTriggerGameStateAdapterResult missingState =
                AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent(
                    null,
                    CreateDrawEvent(),
                    new List<AbilityTriggerRegistration>());
            AbilityTriggerGameStateAdapterResult missingEvent =
                AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent(
                    state,
                    null,
                    new List<AbilityTriggerRegistration>());

            Assert.IsFalse(missingState.accepted);
            Assert.AreEqual(AbilityTriggerGameStateAdapterRejectionReasons.MissingState, missingState.rejection_reason);
            Assert.IsFalse(missingEvent.accepted);
            Assert.AreEqual(AbilityTriggerGameStateAdapterRejectionReasons.MissingGameEvent, missingEvent.rejection_reason);
            Assert.AreSame(originalQueue, state.pending_auto_abilities);
            Assert.AreEqual(1, state.pending_auto_abilities.pending.Count);
        }

        private static GameState CreateStateWithExistingQueue()
        {
            return new GameState
            {
                game_id = "game-1",
                pending_auto_abilities = new PendingAutoAbilityQueue
                {
                    queue_id = "state-queue",
                    pending = new List<PendingAutoAbility>
                    {
                        new PendingAutoAbility
                        {
                            pending_id = "existing",
                            source_card_instance_id = "existing-instance",
                            source_card_id = "EXISTING",
                            player_index = 0,
                            timing_event = "OnRide",
                            summary = "Existing ability"
                        }
                    }
                }
            };
        }

        private static GameEvent CreateDrawEvent()
        {
            return new GameEvent
            {
                event_id = "event-draw-1",
                action_type = GameActionType.Draw,
                actor_index = 0
            };
        }

        private static AbilityTriggerRegistration CreateRegistration(string id, string timingEvent)
        {
            return new AbilityTriggerRegistration
            {
                registration_id = id,
                source_card_instance_id = "instance-" + id,
                source_card_id = "CARD-" + id,
                player_index = 0,
                timing_event = timingEvent,
                summary = "Ability " + id
            };
        }

        private static GameState CreatePlayableState()
        {
            var state = new GameState
            {
                game_id = "game-real-event",
                phase = GamePhase.Main
            };
            state.players.Add(new PlayerGameState
            {
                player_id = "p1",
                deck = new List<GameCardInstance>
                {
                    new GameCardInstance("p1-deck-card-1", "DRAW-CARD-1", 0)
                }
            });
            state.players.Add(new PlayerGameState { player_id = "p2" });
            return state;
        }

        private static LegalGameAction FirstAction(GameState state, int playerIndex, GameActionType actionType)
        {
            foreach (LegalGameAction action in RulesCore.GetLegalActions(state, playerIndex))
            {
                if (action.action_type == actionType)
                {
                    return action;
                }
            }

            return null;
        }
    }
}
