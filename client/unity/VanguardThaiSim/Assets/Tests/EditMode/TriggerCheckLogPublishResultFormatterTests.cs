using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckLogPublishResultFormatterTests
    {
        [Test]
        public void SuccessfulPublishFormatsExistingSuccessMessage()
        {
            Assert.AreEqual(
                TriggerCheckLogPublishResultFormatter.SuccessMessage,
                TriggerCheckLogPublishResultFormatter.Format(MultiplayerTransportResult.Ok()));
        }

        [Test]
        public void FailedPublishPreservesExistingTransportMessageText()
        {
            MultiplayerTransportResult result = MultiplayerTransportResult.Fail(
                "TRIGGER_CHECK_REPLAY_LOG_MISSING",
                "No trigger check replay log payload is available to publish.");

            Assert.AreEqual(
                "TRIGGER_CHECK_REPLAY_LOG_MISSING: No trigger check replay log payload is available to publish.",
                TriggerCheckLogPublishResultFormatter.Format(result));
        }

        [Test]
        public void NullResultFormatsDeterministicTransportError()
        {
            Assert.AreEqual(
                TriggerCheckLogPublishResultFormatter.NullResultMessage,
                TriggerCheckLogPublishResultFormatter.Format(null));
        }

        [Test]
        public void EmptyNonNullResultPreservesCurrentConcatenation()
        {
            Assert.AreEqual(
                ": ",
                TriggerCheckLogPublishResultFormatter.Format(new MultiplayerTransportResult()));
        }
    }
}
