using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckLogPublishValidatorTests
    {
        [Test]
        public void OfflineModeIsRejectedWithExistingMessage()
        {
            TriggerCheckLogPublishValidation validation =
                TriggerCheckLogPublishValidator.Validate(false);

            Assert.IsFalse(validation.accepted);
            Assert.AreEqual(
                TriggerCheckLogPublishValidator.OnlineOnlyMessage,
                validation.rejection_reason);
        }

        [Test]
        public void OnlineModeIsAccepted()
        {
            TriggerCheckLogPublishValidation validation =
                TriggerCheckLogPublishValidator.Validate(true);

            Assert.IsTrue(validation.accepted);
            Assert.AreEqual(string.Empty, validation.rejection_reason);
        }
    }
}
