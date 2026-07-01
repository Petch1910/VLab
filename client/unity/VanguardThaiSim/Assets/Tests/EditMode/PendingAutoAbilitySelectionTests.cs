using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilitySelectionTests
    {
        [Test]
        public void EmptyQueueSelectionIsRejected()
        {
            PendingAutoAbilitySelectionState state =
                PendingAutoAbilitySelection.Select(new PendingAutoAbilityQueue(), 0);

            Assert.IsFalse(state.accepted);
            Assert.IsFalse(state.has_selection);
            Assert.AreEqual(-1, state.selected_index);
            Assert.IsNull(state.selected_ability);
            Assert.AreEqual(PendingAutoAbilitySelection.EmptyQueueReason, state.rejection_reason);
        }

        [Test]
        public void NullQueueSelectionIsRejectedAsEmpty()
        {
            PendingAutoAbilitySelectionState state = PendingAutoAbilitySelection.Select(null, 0);

            Assert.IsFalse(state.accepted);
            Assert.AreEqual(PendingAutoAbilitySelection.EmptyQueueReason, state.rejection_reason);
        }

        [Test]
        public void ValidIndexReturnsCopiedSelectedAbility()
        {
            PendingAutoAbilityQueue queue = CreateQueue();

            PendingAutoAbilitySelectionState state = PendingAutoAbilitySelection.Select(queue, 1);

            Assert.IsTrue(state.accepted);
            Assert.IsTrue(state.has_selection);
            Assert.AreEqual(1, state.selected_index);
            Assert.NotNull(state.selected_ability);
            Assert.AreEqual("pending-2", state.selected_ability.pending_id);
            Assert.AreEqual("CARD-2", state.selected_ability.source_card_id);
            Assert.AreNotSame(queue.pending[1], state.selected_ability);
        }

        [Test]
        public void OutOfRangeIndexIsRejected()
        {
            PendingAutoAbilitySelectionState negative =
                PendingAutoAbilitySelection.Select(CreateQueue(), -1);
            PendingAutoAbilitySelectionState tooHigh =
                PendingAutoAbilitySelection.Select(CreateQueue(), 2);

            Assert.IsFalse(negative.accepted);
            Assert.AreEqual(PendingAutoAbilitySelection.IndexOutOfRangeReason, negative.rejection_reason);
            Assert.IsFalse(tooHigh.accepted);
            Assert.AreEqual(PendingAutoAbilitySelection.IndexOutOfRangeReason, tooHigh.rejection_reason);
        }

        [Test]
        public void NullItemIsRejected()
        {
            var queue = new PendingAutoAbilityQueue
            {
                pending = new List<PendingAutoAbility> { null }
            };

            PendingAutoAbilitySelectionState state = PendingAutoAbilitySelection.Select(queue, 0);

            Assert.IsFalse(state.accepted);
            Assert.AreEqual(PendingAutoAbilitySelection.IndexOutOfRangeReason, state.rejection_reason);
        }

        [Test]
        public void ClearReturnsAcceptedNoSelectionState()
        {
            PendingAutoAbilitySelectionState state = PendingAutoAbilitySelection.Clear();

            Assert.IsTrue(state.accepted);
            Assert.IsFalse(state.has_selection);
            Assert.AreEqual(-1, state.selected_index);
            Assert.IsNull(state.selected_ability);
            Assert.AreEqual(string.Empty, state.rejection_reason);
        }

        [Test]
        public void SelectingDoesNotMutateQueue()
        {
            PendingAutoAbilityQueue queue = CreateQueue();
            string before = queue.ToJson();

            PendingAutoAbilitySelection.Select(queue, 0);

            Assert.AreEqual(before, queue.ToJson());
        }

        private static PendingAutoAbilityQueue CreateQueue()
        {
            return new PendingAutoAbilityQueue
            {
                queue_id = "queue-select",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "src-1",
                        source_card_id = "CARD-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "First"
                    },
                    new PendingAutoAbility
                    {
                        pending_id = "pending-2",
                        source_card_instance_id = "src-2",
                        source_card_id = "CARD-2",
                        player_index = 1,
                        timing_event = "OnBattle",
                        summary = "Second"
                    }
                }
            };
        }
    }
}
