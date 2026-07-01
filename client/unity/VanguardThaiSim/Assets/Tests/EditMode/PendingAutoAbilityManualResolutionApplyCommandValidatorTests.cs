using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionApplyCommandValidatorTests
    {
        [Test]
        public void ValidQueueAndDecisionCreateAcceptedApplyResult()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1");
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1");

            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(queue, decision);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.AreEqual("queue-1", result.queue_id);
            Assert.AreEqual("pending-1", result.pending_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, result.decision_type);
            Assert.IsTrue(result.summary.Contains("pending-1"));
        }

        [Test]
        public void MissingQueueIsRejected()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(
                    null,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing,
                result.rejection_reason);
        }

        [Test]
        public void MissingDecisionIsRejected()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(CreateQueue("pending-1"), null);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing,
                result.rejection_reason);
        }

        [Test]
        public void PendingIdMismatchIsRejected()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(
                    CreateQueue("pending-1"),
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-2"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch,
                result.rejection_reason);
        }

        [Test]
        public void UnsupportedDecisionTypeIsRejected()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(
                    CreateQueue("pending-1"),
                    CreateDecision("AutoResolve", "pending-1"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionTypeInvalid,
                result.rejection_reason);
        }

        [Test]
        public void ValidationDoesNotMutateQueueOrDecision()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1");
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Defer, "pending-1");
            string queueBefore = queue.ToJson(false);
            string decisionBefore = decision.ToJson(false);

            PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(queue, decision);

            Assert.AreEqual(queueBefore, queue.ToJson(false));
            Assert.AreEqual(decisionBefore, decision.ToJson(false));
        }

        private static PendingAutoAbilityQueue CreateQueue(string pendingId)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1"
            };
            queue.pending.Add(new PendingAutoAbility
            {
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "source-1",
                source_card_id = "CARD-1",
                summary = "Pending AUTO"
            });
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
                source_card_instance_id = "source-1",
                source_card_id = "CARD-1",
                hides_source_card_identity = false,
                reason = "test",
                summary = "Manual decision"
            };
        }
    }
}
