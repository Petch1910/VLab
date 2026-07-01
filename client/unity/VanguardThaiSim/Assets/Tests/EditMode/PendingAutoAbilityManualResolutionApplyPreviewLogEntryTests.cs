using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionApplyPreviewLogEntryTests
    {
        [Test]
        public void AcceptedLogEntryRoundTripsJson()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Accepted(
                    "apply-preview-log-1",
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                    "Applied Skip to pending AUTO pending-1.");

            PendingAutoAbilityManualResolutionApplyPreviewLogEntry roundTrip =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.FromJson(entry.ToJson(false));

            Assert.AreEqual("apply-preview-log-1", roundTrip.log_entry_id);
            Assert.IsTrue(roundTrip.accepted);
            Assert.AreEqual(string.Empty, roundTrip.rejection_reason);
            Assert.AreEqual("queue-1", roundTrip.queue_id);
            Assert.AreEqual("pending-1", roundTrip.pending_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Skip, roundTrip.decision_type);
            Assert.AreEqual("Applied Skip to pending AUTO pending-1.", roundTrip.summary);
        }

        [Test]
        public void RejectedLogEntryRoundTripsJson()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Rejected(
                    "apply-preview-log-2",
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing);

            PendingAutoAbilityManualResolutionApplyPreviewLogEntry roundTrip =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.FromJson(entry.ToJson(false));

            Assert.AreEqual("apply-preview-log-2", roundTrip.log_entry_id);
            Assert.IsFalse(roundTrip.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing,
                roundTrip.rejection_reason);
            Assert.AreEqual(string.Empty, roundTrip.queue_id);
            Assert.AreEqual(string.Empty, roundTrip.pending_id);
            Assert.AreEqual(string.Empty, roundTrip.decision_type);
            Assert.AreEqual(string.Empty, roundTrip.summary);
        }

        [Test]
        public void FromApplyResultCreatesAcceptedHiddenSourceSafeEntry()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyResult.Accepted(
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "Validated Resolve for pending AUTO. Structured ability execution remains manual.");

            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.FromApplyResult(
                    result,
                    "apply-preview-log-3");

            string json = entry.ToJson(false);
            Assert.IsTrue(entry.accepted);
            Assert.AreEqual("pending-1", entry.pending_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, entry.decision_type);
            Assert.IsFalse(json.Contains("source_card_id"));
            Assert.IsFalse(json.Contains("source_card_instance_id"));
        }

        [Test]
        public void FromApplyResultCreatesRejectedEntry()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing);

            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.FromApplyResult(
                    result,
                    "apply-preview-log-4");

            Assert.IsFalse(entry.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing,
                entry.rejection_reason);
            Assert.AreEqual(string.Empty, entry.pending_id);
            Assert.AreEqual(string.Empty, entry.decision_type);
        }

        [Test]
        public void FromApplyResultDoesNotMutateSourceResult()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyResult.Accepted(
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                    "Applied Defer to pending AUTO pending-1.");
            string before = result.ToJson(false);

            PendingAutoAbilityManualResolutionApplyPreviewLogEntry.FromApplyResult(
                result,
                "apply-preview-log-5");

            Assert.AreEqual(before, result.ToJson(false));
        }
    }
}
