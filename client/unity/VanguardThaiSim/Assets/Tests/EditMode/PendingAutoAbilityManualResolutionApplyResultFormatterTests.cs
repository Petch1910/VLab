using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionApplyResultFormatterTests
    {
        [Test]
        public void AcceptedResultFormatsDecisionTypeAndPendingId()
        {
            string formatted = PendingAutoAbilityManualResolutionApplyResultFormatter.Format(
                PendingAutoAbilityManualResolutionApplyResult.Accepted(
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "Validated."));

            Assert.AreEqual(
                "Pending manual decision apply: accepted type=Resolve id=pending-1",
                formatted);
        }

        [Test]
        public void RejectedResultFormatsRejectionReason()
        {
            string formatted = PendingAutoAbilityManualResolutionApplyResultFormatter.Format(
                PendingAutoAbilityManualResolutionApplyResult.Rejected(
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch));

            Assert.AreEqual(
                "Pending manual decision apply: rejected " +
                PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch,
                formatted);
        }

        [Test]
        public void NullResultFormatsFallback()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyResultFormatter.NullResultMessage,
                PendingAutoAbilityManualResolutionApplyResultFormatter.Format(null));
        }
    }
}
