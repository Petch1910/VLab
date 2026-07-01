using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionValidationResultFormatterTests
    {
        [Test]
        public void AcceptedResultFormatsDecisionTypeAndPendingId()
        {
            string formatted = PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.Format(
                new PendingAutoAbilityManualResolutionDecisionValidationResult
                {
                    accepted = true,
                    decision = CreateDecision(
                        PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                        "pending-1")
                });

            Assert.AreEqual(
                "Pending manual decision validation: valid type=Resolve id=pending-1 source=CARD-1@source-1",
                formatted);
        }

        [Test]
        public void RejectedResultFormatsRejectionReason()
        {
            string formatted = PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.Format(
                new PendingAutoAbilityManualResolutionDecisionValidationResult
                {
                    accepted = false,
                    rejection_reason = PendingAutoAbilityManualResolutionDecisionValidator.PendingIdMissingReason
                });

            Assert.AreEqual(
                "Pending manual decision validation: rejected " +
                PendingAutoAbilityManualResolutionDecisionValidator.PendingIdMissingReason,
                formatted);
        }

        [Test]
        public void NullResultFormatsFallback()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.NullResultMessage,
                PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.Format(null));
        }

        [Test]
        public void HiddenSourceResultDoesNotLeakSourceIdentity()
        {
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Defer, "pending-hidden");
            decision.hides_source_card_identity = true;
            decision.source_card_id = GameStateViewFactory.HiddenCardId;
            decision.source_card_instance_id = "private-source";

            string formatted = PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.Format(
                new PendingAutoAbilityManualResolutionDecisionValidationResult
                {
                    accepted = true,
                    decision = decision
                });

            Assert.AreEqual(
                "Pending manual decision validation: valid type=Defer id=pending-hidden source=hidden",
                formatted);
            Assert.IsFalse(formatted.Contains("private-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            string decisionType,
            string pendingId)
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-" + pendingId,
                decision_type = decisionType,
                selected_index = 1,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "source-1",
                source_card_id = "CARD-1",
                hides_source_card_identity = false
            };
        }
    }
}
