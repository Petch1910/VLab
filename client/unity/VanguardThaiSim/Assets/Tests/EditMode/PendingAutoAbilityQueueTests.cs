using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueueTests
    {
        [Test]
        public void EnqueuePreservesOrderingAndDoesNotMutateSource()
        {
            PendingAutoAbilityQueue source = CreateQueue();

            PendingAutoAbilityQueue withFirst =
                PendingAutoAbilityQueueBuilder.Enqueue(source, CreateAbility("a1"));
            PendingAutoAbilityQueue withSecond =
                PendingAutoAbilityQueueBuilder.Enqueue(withFirst, CreateAbility("a2"));

            Assert.AreEqual(0, source.pending.Count);
            Assert.AreEqual(1, withFirst.pending.Count);
            Assert.AreEqual(2, withSecond.pending.Count);
            Assert.AreEqual("a1", withSecond.pending[0].pending_id);
            Assert.AreEqual("a2", withSecond.pending[1].pending_id);
        }

        [Test]
        public void PeekReturnsFirstPendingAbilityWithoutRemovingIt()
        {
            PendingAutoAbilityQueue queue = PendingAutoAbilityQueueBuilder.Enqueue(
                PendingAutoAbilityQueueBuilder.Enqueue(CreateQueue(), CreateAbility("a1")),
                CreateAbility("a2"));

            PendingAutoAbility ability = PendingAutoAbilityQueueBuilder.Peek(queue);

            Assert.NotNull(ability);
            Assert.AreEqual("a1", ability.pending_id);
            Assert.AreEqual(2, queue.pending.Count);
        }

        [Test]
        public void DequeueReturnsFirstPendingAbilityAndRemainder()
        {
            PendingAutoAbilityQueue queue = PendingAutoAbilityQueueBuilder.Enqueue(
                PendingAutoAbilityQueueBuilder.Enqueue(CreateQueue(), CreateAbility("a1")),
                CreateAbility("a2"));

            PendingAutoAbility ability;
            PendingAutoAbilityQueue remainder = PendingAutoAbilityQueueBuilder.Dequeue(queue, out ability);

            Assert.NotNull(ability);
            Assert.AreEqual("a1", ability.pending_id);
            Assert.AreEqual(2, queue.pending.Count);
            Assert.AreEqual(1, remainder.pending.Count);
            Assert.AreEqual("a2", remainder.pending[0].pending_id);
        }

        [Test]
        public void ClearReturnsEmptyQueueWithSameId()
        {
            PendingAutoAbilityQueue queue = PendingAutoAbilityQueueBuilder.Enqueue(
                CreateQueue("queue-1"),
                CreateAbility("a1"));

            PendingAutoAbilityQueue cleared = PendingAutoAbilityQueueBuilder.Clear(queue);

            Assert.AreEqual("queue-1", cleared.queue_id);
            Assert.AreEqual(0, cleared.pending.Count);
            Assert.AreEqual(1, queue.pending.Count);
        }

        [Test]
        public void EnsureListsInitializesNullPendingList()
        {
            var queue = new PendingAutoAbilityQueue
            {
                pending = null
            };

            queue.EnsureLists();

            Assert.NotNull(queue.pending);
            Assert.AreEqual(0, queue.pending.Count);
        }

        [Test]
        public void JsonRoundTripPreservesFields()
        {
            PendingAutoAbilityQueue queue = PendingAutoAbilityQueueBuilder.Enqueue(
                CreateQueue("queue-2"),
                CreateAbility("a1"));

            PendingAutoAbilityQueue roundTrip = PendingAutoAbilityQueue.FromJson(queue.ToJson());

            Assert.AreEqual("queue-2", roundTrip.queue_id);
            Assert.AreEqual(1, roundTrip.pending.Count);
            Assert.AreEqual("a1", roundTrip.pending[0].pending_id);
            Assert.AreEqual("instance-a1", roundTrip.pending[0].source_card_instance_id);
            Assert.AreEqual("CARD-a1", roundTrip.pending[0].source_card_id);
            Assert.AreEqual(0, roundTrip.pending[0].player_index);
            Assert.AreEqual("OnAttack", roundTrip.pending[0].timing_event);
            Assert.AreEqual("Ability a1", roundTrip.pending[0].summary);
        }

        private static PendingAutoAbilityQueue CreateQueue(string queueId = "queue")
        {
            return new PendingAutoAbilityQueue
            {
                queue_id = queueId
            };
        }

        private static PendingAutoAbility CreateAbility(string id)
        {
            return new PendingAutoAbility
            {
                pending_id = id,
                source_card_instance_id = "instance-" + id,
                source_card_id = "CARD-" + id,
                player_index = 0,
                timing_event = "OnAttack",
                summary = "Ability " + id
            };
        }
    }
}
