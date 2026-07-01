using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionPublishResultFormatterTests
    {
        [Test]
        public void SuccessFormatsStableMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionPublishResultFormatter.SuccessMessage,
                PendingAutoAbilityManualResolutionDecisionPublishResultFormatter.Format(
                    MultiplayerTransportResult.Ok()));
        }

        [Test]
        public void FailureFormatsErrorCodeAndMessage()
        {
            Assert.AreEqual(
                "SEND_FAILED: network unavailable",
                PendingAutoAbilityManualResolutionDecisionPublishResultFormatter.Format(
                    MultiplayerTransportResult.Fail("SEND_FAILED", "network unavailable")));
        }

        [Test]
        public void NullResultFormatsFallback()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionPublishResultFormatter.NullResultMessage,
                PendingAutoAbilityManualResolutionDecisionPublishResultFormatter.Format(null));
        }
    }
}
