using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionApplyCommandContractTests
    {
        [Test]
        public void CommandJsonRoundTrips()
        {
            var command = new PendingAutoAbilityManualResolutionApplyCommand
            {
                command_id = "apply|pending-1|decision-1",
                queue_id = "queue-1",
                pending_id = "pending-1",
                decision_id = "decision-1",
                decision_type = PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                player_index = 0
            };

            PendingAutoAbilityManualResolutionApplyCommand roundTrip =
                PendingAutoAbilityManualResolutionApplyCommand.FromJson(command.ToJson(false));

            Assert.AreEqual(command.command_id, roundTrip.command_id);
            Assert.AreEqual(command.queue_id, roundTrip.queue_id);
            Assert.AreEqual(command.pending_id, roundTrip.pending_id);
            Assert.AreEqual(command.decision_id, roundTrip.decision_id);
            Assert.AreEqual(command.decision_type, roundTrip.decision_type);
            Assert.AreEqual(command.player_index, roundTrip.player_index);
        }

        [Test]
        public void ResultJsonRoundTrips()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyResult.Accepted(
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                    "Skipped pending AUTO.");

            PendingAutoAbilityManualResolutionApplyResult roundTrip =
                PendingAutoAbilityManualResolutionApplyResult.FromJson(result.ToJson(false));

            Assert.IsTrue(roundTrip.accepted);
            Assert.AreEqual(string.Empty, roundTrip.rejection_reason);
            Assert.AreEqual("queue-1", roundTrip.queue_id);
            Assert.AreEqual("pending-1", roundTrip.pending_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Skip, roundTrip.decision_type);
            Assert.AreEqual("Skipped pending AUTO.", roundTrip.summary);
        }

        [Test]
        public void RejectionReasonConstantsAreStable()
        {
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_QUEUE_MISSING",
                PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing);
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING",
                PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing);
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_PENDING_ID_MISMATCH",
                PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch);
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID",
                PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionTypeInvalid);
        }

        [Test]
        public void RejectedResultCarriesReasonWithoutQueueMutationContract()
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing,
                result.rejection_reason);
            Assert.AreEqual(string.Empty, result.queue_id);
            Assert.AreEqual(string.Empty, result.pending_id);
            Assert.AreEqual(string.Empty, result.decision_type);
        }
    }
}
