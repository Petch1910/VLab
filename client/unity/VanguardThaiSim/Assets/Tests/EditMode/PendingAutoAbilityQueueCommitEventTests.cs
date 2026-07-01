using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueueCommitEventTests
    {
        [Test]
        public void BuildAcceptedCommitEventRoundTripsReplayMetadata()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1", "pending-2");
            PendingAutoAbilityQueueCommitResult commit =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1"));

            PendingAutoAbilityQueueCommitEventBuildResult result =
                PendingAutoAbilityQueueCommitEventBuilder.Build(commit);
            PendingAutoAbilityQueueCommitEvent roundTrip =
                PendingAutoAbilityQueueCommitEvent.FromJson(result.commit_event.ToJson(false));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.NotNull(result.commit_event);
            Assert.AreEqual(PendingAutoAbilityQueueCommitEvent.EventType, roundTrip.event_type);
            Assert.AreEqual(commit.queue_id, roundTrip.queue_id);
            Assert.AreEqual(commit.pending_id, roundTrip.pending_id);
            Assert.AreEqual(commit.decision_id, roundTrip.decision_id);
            Assert.AreEqual(commit.decision_type, roundTrip.decision_type);
            Assert.AreEqual(commit.player_index, roundTrip.player_index);
            Assert.AreEqual(commit.before_queue_hash, roundTrip.before_queue_hash);
            Assert.AreEqual(commit.after_queue_hash, roundTrip.after_queue_hash);
            Assert.AreEqual(commit.manual_resolution_recorded, roundTrip.manual_resolution_recorded);
            Assert.IsTrue(roundTrip.event_id.Contains(commit.pending_id));
        }

        [Test]
        public void BuildManualResolveEventRecordsManualResolutionFlag()
        {
            PendingAutoAbilityQueueCommitResult commit =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    CreateQueue("pending-1"),
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1"));

            PendingAutoAbilityQueueCommitEventBuildResult result =
                PendingAutoAbilityQueueCommitEventBuilder.Build(commit);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.commit_event.manual_resolution_recorded);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, result.commit_event.decision_type);
            Assert.IsTrue(result.commit_event.summary.Contains(PendingAutoAbilityManualResolutionDecisionTypes.Resolve));
        }

        [Test]
        public void BuildRejectsMissingOrRejectedCommitResult()
        {
            PendingAutoAbilityQueueCommitEventBuildResult missing =
                PendingAutoAbilityQueueCommitEventBuilder.Build(null);
            PendingAutoAbilityQueueCommitResult rejectedCommit =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    CreateQueue("pending-1"),
                    CreateDecision("Unsupported", "pending-1"));
            PendingAutoAbilityQueueCommitEventBuildResult rejected =
                PendingAutoAbilityQueueCommitEventBuilder.Build(rejectedCommit);

            Assert.IsFalse(missing.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitEventBuildRejectionReasons.CommitResultMissing, missing.rejection_reason);
            Assert.IsNull(missing.commit_event);
            Assert.IsFalse(rejected.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitEventBuildRejectionReasons.CommitResultRejected, rejected.rejection_reason);
            Assert.IsNull(rejected.commit_event);
        }

        [Test]
        public void BuildDoesNotLeakSourceCardIdentityOrMutateCommitResult()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1");
            string beforeQueue = queue.ToJson(false);
            PendingAutoAbilityQueueCommitResult commit =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1"));
            string beforeCommitQueue = commit.queue.ToJson(false);

            PendingAutoAbilityQueueCommitEventBuildResult result =
                PendingAutoAbilityQueueCommitEventBuilder.Build(commit);
            string json = result.commit_event.ToJson(false);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsFalse(json.Contains("source_card_id"));
            Assert.IsFalse(json.Contains("source_card_instance_id"));
            Assert.IsFalse(json.Contains("CARD-SECRET"));
            Assert.IsFalse(json.Contains("src-secret"));
            Assert.AreEqual(beforeQueue, queue.ToJson(false));
            Assert.AreEqual(beforeCommitQueue, commit.queue.ToJson(false));
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
                        source_card_instance_id = "src-secret",
                        source_card_id = "CARD-SECRET",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "Secret source pending"
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
                decision_id = "decision|CARD-SECRET|src-secret|" + decisionType + "|" + pendingId,
                decision_type = decisionType,
                selected_index = 0,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "src-secret",
                source_card_id = "CARD-SECRET",
                reason = "test " + decisionType,
                summary = "Decision " + decisionType
            };
        }
    }
}
