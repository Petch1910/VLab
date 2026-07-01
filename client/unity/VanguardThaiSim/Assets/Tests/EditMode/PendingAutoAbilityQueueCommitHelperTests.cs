using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueueCommitHelperTests
    {
        [Test]
        public void CommitSkipRemovesActivePendingWithoutMutatingInput()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2");
            string before = queue.ToJson(false);

            PendingAutoAbilityQueueCommitResult result =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1"));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual("queue-1", result.queue_id);
            Assert.AreEqual("pending-1", result.pending_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Skip, result.decision_type);
            Assert.IsFalse(result.manual_resolution_recorded);
            Assert.AreEqual(1, result.queue.pending.Count);
            Assert.AreEqual("pending-2", result.queue.pending[0].pending_id);
            Assert.AreNotEqual(result.before_queue_hash, result.after_queue_hash);
            Assert.AreEqual(before, queue.ToJson(false));
        }

        [Test]
        public void CommitDeferMovesActivePendingToBackWithoutMutatingInput()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2", "pending-3");
            string before = queue.ToJson(false);

            PendingAutoAbilityQueueCommitResult result =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Defer, "pending-1"));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsFalse(result.manual_resolution_recorded);
            Assert.AreEqual(3, result.queue.pending.Count);
            Assert.AreEqual("pending-2", result.queue.pending[0].pending_id);
            Assert.AreEqual("pending-3", result.queue.pending[1].pending_id);
            Assert.AreEqual("pending-1", result.queue.pending[2].pending_id);
            Assert.AreNotEqual(result.before_queue_hash, result.after_queue_hash);
            Assert.AreEqual(before, queue.ToJson(false));
        }

        [Test]
        public void CommitManualResolveRecordsManualResolutionWithoutExecutingCardText()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2");
            string before = queue.ToJson(false);

            PendingAutoAbilityQueueCommitResult result =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1"));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.manual_resolution_recorded);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, result.decision_type);
            Assert.AreEqual(1, result.queue.pending.Count);
            Assert.AreEqual("pending-2", result.queue.pending[0].pending_id);
            Assert.AreEqual(before, queue.ToJson(false));
        }

        [Test]
        public void CommitRejectsInvalidInputsWithoutMutatingInput()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1");
            string before = queue.ToJson(false);

            PendingAutoAbilityQueueCommitResult missingDecision =
                PendingAutoAbilityQueueCommitHelper.Commit(queue, null);
            PendingAutoAbilityQueueCommitResult invalidType =
                PendingAutoAbilityQueueCommitHelper.Commit(queue, CreateDecision("BadDecision", "pending-1"));
            PendingAutoAbilityQueueCommitResult mismatch =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "other-pending"));
            PendingAutoAbilityQueueCommitResult missingQueue =
                PendingAutoAbilityQueueCommitHelper.Commit(null, CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1"));

            Assert.IsFalse(missingDecision.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.DecisionMissing, missingDecision.rejection_reason);
            Assert.IsFalse(invalidType.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.DecisionTypeInvalid, invalidType.rejection_reason);
            Assert.IsFalse(mismatch.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.PendingIdMismatch, mismatch.rejection_reason);
            Assert.IsFalse(missingQueue.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.QueueMissing, missingQueue.rejection_reason);
            Assert.AreEqual(before, queue.ToJson(false));
        }

        [Test]
        public void CommitRejectsNullPendingListWithoutNormalizingInput()
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-null",
                pending = null
            };

            PendingAutoAbilityQueueCommitResult result =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.QueueMissing, result.rejection_reason);
            Assert.IsNull(queue.pending);
        }

        private static PendingAutoAbilityQueue CreateQueue(params string[] pendingIds)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>()
            };

            for (int i = 0; i < pendingIds.Length; i++)
            {
                queue.pending.Add(
                    new PendingAutoAbility
                    {
                        pending_id = pendingIds[i],
                        source_card_instance_id = "src-" + i,
                        source_card_id = "CARD-" + i,
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "Pending " + pendingIds[i]
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
                decision_id = "decision|" + decisionType + "|" + pendingId,
                decision_type = decisionType,
                selected_index = 0,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnDraw",
                reason = "test " + decisionType,
                summary = "Decision " + decisionType
            };
        }
    }
}
