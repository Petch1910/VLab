using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionApplyExecutorTests
    {
        [Test]
        public void SkipRemovesFirstPendingItemFromReturnedQueue()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2");
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1");

            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                PendingAutoAbilityManualResolutionApplyExecutor.Apply(queue, decision);

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.apply_result.accepted);
            Assert.AreEqual(1, result.queue.pending.Count);
            Assert.AreEqual("pending-2", result.queue.pending[0].pending_id);
        }

        [Test]
        public void DeferMovesFirstPendingItemToEndOfReturnedQueue()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2", "pending-3");
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Defer, "pending-1");

            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                PendingAutoAbilityManualResolutionApplyExecutor.Apply(queue, decision);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(3, result.queue.pending.Count);
            Assert.AreEqual("pending-2", result.queue.pending[0].pending_id);
            Assert.AreEqual("pending-3", result.queue.pending[1].pending_id);
            Assert.AreEqual("pending-1", result.queue.pending[2].pending_id);
        }

        [Test]
        public void ResolveIsAcceptedButDoesNotExecuteOrMutateReturnedQueue()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2");
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1");

            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                PendingAutoAbilityManualResolutionApplyExecutor.Apply(queue, decision);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(2, result.queue.pending.Count);
            Assert.AreEqual("pending-1", result.queue.pending[0].pending_id);
            Assert.AreEqual("pending-2", result.queue.pending[1].pending_id);
            Assert.IsTrue(result.apply_result.summary.Contains("Structured ability execution remains manual"));
        }

        [Test]
        public void InvalidInputsAreRejectedThroughValidator()
        {
            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                PendingAutoAbilityManualResolutionApplyExecutor.Apply(
                    CreateQueue("pending-1"),
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-2"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch,
                result.rejection_reason);
            Assert.IsFalse(result.apply_result.accepted);
            Assert.AreEqual(1, result.queue.pending.Count);
            Assert.AreEqual("pending-1", result.queue.pending[0].pending_id);
        }

        [Test]
        public void ApplyDoesNotMutateSourceQueueOrDecision()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2");
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1");
            string queueBefore = queue.ToJson(false);
            string decisionBefore = decision.ToJson(false);

            PendingAutoAbilityManualResolutionApplyExecutor.Apply(queue, decision);

            Assert.AreEqual(queueBefore, queue.ToJson(false));
            Assert.AreEqual(decisionBefore, decision.ToJson(false));
        }

        private static PendingAutoAbilityQueue CreateQueue(params string[] pendingIds)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-apply"
            };

            for (int i = 0; i < pendingIds.Length; i++)
            {
                queue.pending.Add(new PendingAutoAbility
                {
                    pending_id = pendingIds[i],
                    player_index = 0,
                    timing_event = "OnBattle",
                    source_card_instance_id = "source-" + i,
                    source_card_id = "CARD-" + i,
                    summary = "Pending AUTO " + i
                });
            }

            return queue;
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            string decisionType,
            string pendingId)
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-" + pendingId,
                decision_type = decisionType,
                selected_index = 0,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "source-0",
                source_card_id = "CARD-0",
                hides_source_card_identity = false,
                reason = "test",
                summary = "Manual decision"
            };
        }
    }
}
