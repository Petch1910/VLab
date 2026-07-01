using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionDraftResultFormatterTests
    {
        [Test]
        public void AcceptedResultFormatsStableMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.SuccessMessage,
                PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.Format(
                    new PendingAutoAbilityManualResolutionDecisionDraftResult
                    {
                        accepted = true
                    }));
        }

        [Test]
        public void RejectedResultFormatsRejectionReason()
        {
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING",
                PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.Format(
                    new PendingAutoAbilityManualResolutionDecisionDraftResult
                    {
                        accepted = false,
                        rejection_reason = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING"
                    }));
        }

        [Test]
        public void NullResultFormatsFallback()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.NullResultMessage,
                PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.Format(null));
        }
    }
}
