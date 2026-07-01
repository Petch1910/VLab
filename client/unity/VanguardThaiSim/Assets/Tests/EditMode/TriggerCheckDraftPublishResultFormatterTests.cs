using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftPublishResultFormatterTests
    {
        [Test]
        public void SuccessfulPublishFormatsSuccessMessage()
        {
            ManualTriggerCheckDraftResult result = new ManualTriggerCheckDraftResult
            {
                accepted = true,
                sent = true,
                rejection_reason = "ignored",
                transport_error_code = "TRANSPORT_IGNORED",
                transport_message = "ignored message"
            };

            Assert.AreEqual(
                TriggerCheckDraftPublishResultFormatter.SuccessMessage,
                TriggerCheckDraftPublishResultFormatter.Format(result));
        }

        [Test]
        public void RejectionReasonHasPriority()
        {
            ManualTriggerCheckDraftResult result = new ManualTriggerCheckDraftResult
            {
                accepted = false,
                sent = false,
                rejection_reason = "CHECKED_CARD_ID_MISSING",
                transport_error_code = "TRANSPORT_FAILED",
                transport_message = "network failed"
            };

            Assert.AreEqual(
                "CHECKED_CARD_ID_MISSING",
                TriggerCheckDraftPublishResultFormatter.Format(result));
        }

        [Test]
        public void TransportErrorCodeIsUsedWhenRejectionIsEmpty()
        {
            ManualTriggerCheckDraftResult result = new ManualTriggerCheckDraftResult
            {
                accepted = true,
                sent = false,
                rejection_reason = string.Empty,
                transport_error_code = "TRANSPORT_FAILED",
                transport_message = "network failed"
            };

            Assert.AreEqual(
                "TRANSPORT_FAILED",
                TriggerCheckDraftPublishResultFormatter.Format(result));
        }

        [Test]
        public void TransportMessageIsUsedWhenEarlierMessagesAreEmpty()
        {
            ManualTriggerCheckDraftResult result = new ManualTriggerCheckDraftResult
            {
                accepted = true,
                sent = false,
                rejection_reason = string.Empty,
                transport_error_code = string.Empty,
                transport_message = "network failed"
            };

            Assert.AreEqual(
                "network failed",
                TriggerCheckDraftPublishResultFormatter.Format(result));
        }

        [Test]
        public void NullOrEmptyResultFormatsEmptyString()
        {
            Assert.AreEqual(string.Empty, TriggerCheckDraftPublishResultFormatter.Format(null));
            Assert.AreEqual(
                string.Empty,
                TriggerCheckDraftPublishResultFormatter.Format(new ManualTriggerCheckDraftResult()));
        }
    }
}
