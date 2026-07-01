using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class AbilityTriggerEventCollectorTests
    {
        [Test]
        public void MatchingTimingEventCreatesPendingAbility()
        {
            GameEvent gameEvent = CreateMoveEvent();
            var registrations = new List<AbilityTriggerRegistration>
            {
                CreateRegistration("reg-1", "OnMoveCard")
            };

            PendingAutoAbilityQueue queue =
                AbilityTriggerEventCollector.Collect(gameEvent, registrations);

            Assert.AreEqual(1, queue.pending.Count);
            Assert.AreEqual("instance-reg-1", queue.pending[0].source_card_instance_id);
            Assert.AreEqual("CARD-reg-1", queue.pending[0].source_card_id);
            Assert.AreEqual(0, queue.pending[0].player_index);
            Assert.AreEqual("OnMoveCard", queue.pending[0].timing_event);
            Assert.AreEqual("Ability reg-1", queue.pending[0].summary);
        }

        [Test]
        public void NonMatchingTimingEventCreatesNoPendingAbility()
        {
            GameEvent gameEvent = CreateMoveEvent();
            var registrations = new List<AbilityTriggerRegistration>
            {
                CreateRegistration("reg-1", "OnDraw")
            };

            PendingAutoAbilityQueue queue =
                AbilityTriggerEventCollector.Collect(gameEvent, registrations);

            Assert.AreEqual(0, queue.pending.Count);
        }

        [Test]
        public void PendingIdIsDeterministicForSameEventAndRegistration()
        {
            GameEvent gameEvent = CreateMoveEvent();
            var registrations = new List<AbilityTriggerRegistration>
            {
                CreateRegistration("reg-1", "OnMoveCard")
            };

            PendingAutoAbilityQueue first =
                AbilityTriggerEventCollector.Collect(gameEvent, registrations);
            PendingAutoAbilityQueue second =
                AbilityTriggerEventCollector.Collect(gameEvent, registrations);

            const string expectedId =
                "pending-auto|reg-1|OnMoveCard|event-1|MoveCard|card-inst-1|0|0";

            Assert.AreEqual(expectedId, first.pending[0].pending_id);
            Assert.AreEqual(expectedId, second.pending[0].pending_id);
        }

        [Test]
        public void SourceQueueIsCopiedBeforeAppending()
        {
            var source = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "existing",
                        source_card_instance_id = "existing-instance",
                        source_card_id = "EXISTING",
                        player_index = 1,
                        timing_event = "OnDraw",
                        summary = "Existing"
                    }
                }
            };

            PendingAutoAbilityQueue result = AbilityTriggerEventCollector.Collect(
                CreateMoveEvent(),
                new List<AbilityTriggerRegistration>
                {
                    CreateRegistration("reg-1", "OnMoveCard")
                },
                source);

            Assert.AreEqual(1, source.pending.Count);
            Assert.AreEqual("queue-1", result.queue_id);
            Assert.AreEqual(2, result.pending.Count);
            Assert.AreEqual("existing", result.pending[0].pending_id);
            Assert.AreEqual("reg-1", result.pending[1].pending_id.Split('|')[1]);
        }

        [Test]
        public void NullEventOrRegistrationsReturnCopiedQueue()
        {
            var source = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = null
            };

            PendingAutoAbilityQueue nullEvent =
                AbilityTriggerEventCollector.Collect(null, new List<AbilityTriggerRegistration>(), source);
            PendingAutoAbilityQueue nullRegistrations =
                AbilityTriggerEventCollector.Collect(CreateMoveEvent(), null, source);

            Assert.AreEqual("queue-1", nullEvent.queue_id);
            Assert.AreEqual(0, nullEvent.pending.Count);
            Assert.AreEqual("queue-1", nullRegistrations.queue_id);
            Assert.AreEqual(0, nullRegistrations.pending.Count);
            Assert.IsNull(source.pending);
        }

        [Test]
        public void TimingEventMappingCoversKnownGameActions()
        {
            Assert.AreEqual("OnDraw", AbilityTriggerEventCollector.GetTimingEvent(
                new GameEvent { action_type = GameActionType.Draw }));
            Assert.AreEqual("OnMoveCard", AbilityTriggerEventCollector.GetTimingEvent(
                new GameEvent { action_type = GameActionType.MoveCard }));
            Assert.AreEqual("OnSetPhase", AbilityTriggerEventCollector.GetTimingEvent(
                new GameEvent { action_type = GameActionType.SetPhase }));
            Assert.AreEqual("OnAddGiftMarker", AbilityTriggerEventCollector.GetTimingEvent(
                new GameEvent { action_type = GameActionType.AddGiftMarker }));
            Assert.AreEqual("OnResourceFlip", AbilityTriggerEventCollector.GetTimingEvent(
                new GameEvent { action_type = GameActionType.ResourceFlip }));
        }

        private static GameEvent CreateMoveEvent()
        {
            return new GameEvent
            {
                event_id = "event-1",
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                card_instance_id = "card-inst-1",
                from_zone = GameZone.Hand,
                to_zone = GameZone.RearGuard,
                from_index = 2,
                to_index = 1
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
    }
}
