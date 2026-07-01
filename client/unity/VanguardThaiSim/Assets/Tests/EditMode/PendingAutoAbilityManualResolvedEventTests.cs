using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolvedEventTests
    {
        [Test]
        public void ResolveCommitBuildsManualResolvedEvent()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateDecision(
                "pending-1",
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "manual unsupported effect");
            PendingAutoAbilityQueueCommitResult commitResult =
                PendingAutoAbilityQueueCommitHelper.Commit(CreateQueue("pending-1"), decision);

            PendingAutoAbilityManualResolvedEventBuildResult result =
                PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, decision);

            Assert.IsTrue(commitResult.accepted);
            Assert.IsTrue(result.accepted);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.NotNull(result.resolved_event);
            Assert.AreEqual(PendingAutoAbilityManualResolvedEvent.EventType, result.resolved_event.event_type);
            Assert.AreEqual("queue-1", result.resolved_event.queue_id);
            Assert.AreEqual("pending-1", result.resolved_event.pending_id);
            Assert.AreEqual(commitResult.decision_id, result.resolved_event.decision_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, result.resolved_event.decision_type);
            Assert.AreEqual(0, result.resolved_event.player_index);
            Assert.AreEqual("OnDraw", result.resolved_event.timing_event);
            Assert.AreEqual(commitResult.before_queue_hash, result.resolved_event.before_queue_hash);
            Assert.AreEqual(commitResult.after_queue_hash, result.resolved_event.after_queue_hash);
            Assert.AreEqual("manual unsupported effect", result.resolved_event.manual_resolution_reason);
            Assert.AreNotEqual(string.Empty, result.resolved_event.manual_resolution_reason_hash);
            Assert.IsFalse(result.resolved_event.hides_source_card_identity);
            Assert.IsTrue(result.resolved_event.summary.Contains("pending-1"));
        }

        [Test]
        public void ManualResolvedEventRoundTripsJson()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateDecision(
                "pending-1",
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "manual unsupported effect");
            PendingAutoAbilityQueueCommitResult commitResult =
                PendingAutoAbilityQueueCommitHelper.Commit(CreateQueue("pending-1"), decision);

            PendingAutoAbilityManualResolvedEventBuildResult result =
                PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, decision);

            PendingAutoAbilityManualResolvedEvent roundTrip =
                PendingAutoAbilityManualResolvedEvent.FromJson(result.resolved_event.ToJson(false));

            Assert.AreEqual(result.resolved_event.event_id, roundTrip.event_id);
            Assert.AreEqual(PendingAutoAbilityManualResolvedEvent.EventType, roundTrip.event_type);
            Assert.AreEqual(result.resolved_event.pending_id, roundTrip.pending_id);
            Assert.AreEqual(result.resolved_event.manual_resolution_reason_hash, roundTrip.manual_resolution_reason_hash);
        }

        [Test]
        public void NonResolveOrRejectedCommitDoesNotBuildEvent()
        {
            PendingAutoAbilityManualResolutionDecision skipDecision = CreateDecision(
                "pending-1",
                PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                "skip unsupported effect");
            PendingAutoAbilityQueueCommitResult skipCommit =
                PendingAutoAbilityQueueCommitHelper.Commit(CreateQueue("pending-1"), skipDecision);
            PendingAutoAbilityQueueCommitResult rejectedCommit =
                PendingAutoAbilityQueueCommitHelper.Commit(CreateQueue("pending-1"), CreateDecision(
                    "other-pending",
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "wrong pending"));

            PendingAutoAbilityManualResolvedEventBuildResult skipResult =
                PendingAutoAbilityManualResolvedEventBuilder.Build(skipCommit, skipDecision);
            PendingAutoAbilityManualResolvedEventBuildResult rejectedResult =
                PendingAutoAbilityManualResolvedEventBuilder.Build(rejectedCommit, skipDecision);

            Assert.IsFalse(skipResult.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolvedEventBuildRejectionReasons.ManualResolutionNotRecorded,
                skipResult.rejection_reason);
            Assert.IsNull(skipResult.resolved_event);
            Assert.IsFalse(rejectedResult.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolvedEventBuildRejectionReasons.CommitResultRejected,
                rejectedResult.rejection_reason);
            Assert.IsNull(rejectedResult.resolved_event);
        }

        [Test]
        public void MissingOrMismatchedInputsReject()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateDecision(
                "pending-1",
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "manual unsupported effect");
            PendingAutoAbilityQueueCommitResult commitResult =
                PendingAutoAbilityQueueCommitHelper.Commit(CreateQueue("pending-1"), decision);
            PendingAutoAbilityManualResolutionDecision mismatchDecision = CreateDecision(
                "other-pending",
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "manual unsupported effect");
            PendingAutoAbilityManualResolutionDecision invalidTypeDecision = CreateDecision(
                "pending-1",
                PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                "manual unsupported effect");

            PendingAutoAbilityManualResolvedEventBuildResult missingCommit =
                PendingAutoAbilityManualResolvedEventBuilder.Build(null, decision);
            PendingAutoAbilityManualResolvedEventBuildResult missingDecision =
                PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, null);
            PendingAutoAbilityManualResolvedEventBuildResult mismatch =
                PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, mismatchDecision);
            PendingAutoAbilityManualResolvedEventBuildResult invalidType =
                PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, invalidTypeDecision);

            Assert.AreEqual(
                PendingAutoAbilityManualResolvedEventBuildRejectionReasons.CommitResultMissing,
                missingCommit.rejection_reason);
            Assert.AreEqual(
                PendingAutoAbilityManualResolvedEventBuildRejectionReasons.DecisionMissing,
                missingDecision.rejection_reason);
            Assert.AreEqual(
                PendingAutoAbilityManualResolvedEventBuildRejectionReasons.PendingIdMismatch,
                mismatch.rejection_reason);
            Assert.AreEqual(
                PendingAutoAbilityManualResolvedEventBuildRejectionReasons.DecisionTypeInvalid,
                invalidType.rejection_reason);
        }

        [Test]
        public void HiddenSourceEventDoesNotLeakSourceIdsOrReasonText()
        {
            const string rawSourceInstance = "raw-source-inst";
            const string rawCardId = "BT01-001TH";
            string rawPendingId = "pending-auto|" + rawSourceInstance + "|" + rawCardId;
            PendingAutoAbilityManualResolutionDecision decision = CreateDecision(
                rawPendingId,
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "manual reason mentions " + rawSourceInstance + " and " + rawCardId,
                true,
                rawSourceInstance,
                rawCardId);
            PendingAutoAbilityQueueCommitResult commitResult =
                PendingAutoAbilityQueueCommitHelper.Commit(CreateQueue(rawPendingId), decision);

            PendingAutoAbilityManualResolvedEventBuildResult result =
                PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, decision);
            string json = result.resolved_event.ToJson(false);

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.resolved_event.hides_source_card_identity);
            Assert.AreEqual("<hidden>", result.resolved_event.manual_resolution_reason);
            Assert.IsTrue(result.resolved_event.pending_id.StartsWith("pending-auto-hidden|0|OnDraw|"));
            Assert.IsTrue(result.resolved_event.decision_id.StartsWith("pending-auto-manual-decision|Resolve|"));
            Assert.IsFalse(json.Contains(rawSourceInstance));
            Assert.IsFalse(json.Contains(rawCardId));
            Assert.IsFalse(json.Contains("manual reason mentions"));
        }

        [Test]
        public void BuilderDoesNotMutateSourceInputs()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1");
            PendingAutoAbilityManualResolutionDecision decision = CreateDecision(
                "pending-1",
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "manual unsupported effect");
            string decisionJsonBefore = decision.ToJson(false);
            string queueJsonBefore = queue.ToJson(false);
            PendingAutoAbilityQueueCommitResult commitResult =
                PendingAutoAbilityQueueCommitHelper.Commit(queue, decision);
            string commitPendingBefore = commitResult.pending_id;
            string commitDecisionBefore = commitResult.decision_id;
            string commitQueueJsonBefore = commitResult.queue.ToJson(false);

            PendingAutoAbilityManualResolvedEventBuilder.Build(commitResult, decision);

            Assert.AreEqual(decisionJsonBefore, decision.ToJson(false));
            Assert.AreEqual(queueJsonBefore, queue.ToJson(false));
            Assert.AreEqual(commitPendingBefore, commitResult.pending_id);
            Assert.AreEqual(commitDecisionBefore, commitResult.decision_id);
            Assert.AreEqual(commitQueueJsonBefore, commitResult.queue.ToJson(false));
        }

        private static PendingAutoAbilityQueue CreateQueue(string pendingId)
        {
            return new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new System.Collections.Generic.List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = pendingId,
                        source_card_instance_id = "source-inst-1",
                        source_card_id = "CARD-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "Pending ability"
                    }
                }
            };
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            string pendingId,
            string decisionType,
            string reason,
            bool hidesSource = false,
            string sourceCardInstanceId = "source-inst-1",
            string sourceCardId = "CARD-1")
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-1",
                decision_type = decisionType,
                selected_index = 0,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = hidesSource ? string.Empty : sourceCardInstanceId,
                source_card_id = hidesSource ? GameStateViewFactory.HiddenCardId : sourceCardId,
                hides_source_card_identity = hidesSource,
                reason = reason,
                summary = "Pending ability"
            };
        }
    }
}
