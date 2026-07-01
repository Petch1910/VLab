using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueueMaskerTests
    {
        [Test]
        public void TrueStateViewPreservesSourceIdentity()
        {
            PendingAutoAbilityQueue queue = CreateTwoPlayerQueue();

            PendingAutoAbilityQueue view = PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.TrueState);

            Assert.AreEqual("CARD-P0", view.pending[0].source_card_id);
            Assert.AreEqual("CARD-P1", view.pending[1].source_card_id);
            Assert.IsFalse(view.pending[0].hides_source_card_identity);
        }

        [Test]
        public void OwnerPlayerViewPreservesOwnAbilityAndMasksOpponentAbility()
        {
            PendingAutoAbilityQueue queue = CreateTwoPlayerQueue();

            PendingAutoAbilityQueue view = PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.Player,
                0);

            Assert.AreEqual("CARD-P0", view.pending[0].source_card_id);
            Assert.AreEqual("src-p0", view.pending[0].source_card_instance_id);
            Assert.IsFalse(view.pending[0].hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.pending[1].source_card_id);
            Assert.AreEqual("hidden-pending-auto-source-0001", view.pending[1].source_card_instance_id);
            Assert.IsTrue(view.pending[1].hides_source_card_identity);
            Assert.IsFalse(view.pending[1].pending_id.Contains("CARD-P1"));
            Assert.IsFalse(view.pending[1].summary.Contains("CARD-P1"));
        }

        [Test]
        public void SpectatorViewMasksAllSourceIdentity()
        {
            PendingAutoAbilityQueue queue = CreateTwoPlayerQueue();

            PendingAutoAbilityQueue view = PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.pending[0].source_card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.pending[1].source_card_id);
            Assert.AreEqual("hidden-pending-auto-source-0000", view.pending[0].source_card_instance_id);
            Assert.AreEqual("pending-auto-hidden|0|OnDraw|0000", view.pending[0].pending_id);
            Assert.IsTrue(view.pending[0].hides_source_card_identity);
            Assert.IsFalse(view.pending[0].summary.Contains("CARD-P0"));
            Assert.IsFalse(view.pending[1].summary.Contains("CARD-P1"));
        }

        [Test]
        public void MaskingIsDeterministicAndDoesNotMutateSource()
        {
            PendingAutoAbilityQueue queue = CreateTwoPlayerQueue();
            string before = queue.ToJson();

            PendingAutoAbilityQueue first = PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.Spectator);
            PendingAutoAbilityQueue second = PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(first.ToJson(), second.ToJson());
            Assert.AreEqual(before, queue.ToJson());
        }

        [Test]
        public void NullSourceCreatesEmptyDefaultQueue()
        {
            PendingAutoAbilityQueue view = PendingAutoAbilityQueueMasker.CreateView(
                null,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual("pending-auto-ability-queue", view.queue_id);
            Assert.AreEqual(0, view.pending.Count);
        }

        private static PendingAutoAbilityQueue CreateTwoPlayerQueue()
        {
            return new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    CreateAbility("p0", 0, "OnDraw", "CARD-P0"),
                    CreateAbility("p1", 1, "OnMoveCard", "CARD-P1")
                }
            };
        }

        private static PendingAutoAbility CreateAbility(
            string id,
            int playerIndex,
            string timingEvent,
            string cardId)
        {
            return new PendingAutoAbility
            {
                pending_id = "pending-" + cardId,
                source_card_instance_id = "src-" + id,
                source_card_id = cardId,
                player_index = playerIndex,
                timing_event = timingEvent,
                summary = cardId + " ability " + timingEvent
            };
        }
    }
}
