using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftSelectionValidatorTests
    {
        [Test]
        public void LocalModeRejectsWithOnlineOnlyMessage()
        {
            TriggerCheckDraftSelectionValidation result = TriggerCheckDraftSelectionValidator.Validate(
                false,
                "instance-1",
                "BT01-001TH");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(TriggerCheckDraftSelectionValidator.OnlineOnlyMessage, result.rejection_reason);
        }

        [Test]
        public void NoSelectionRejectsWithSelectCardMessage()
        {
            TriggerCheckDraftSelectionValidation result = TriggerCheckDraftSelectionValidator.Validate(
                true,
                null,
                null);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(TriggerCheckDraftSelectionValidator.SelectCardMessage, result.rejection_reason);
        }

        [Test]
        public void HiddenCardRejectsWithHiddenIdentityMessage()
        {
            TriggerCheckDraftSelectionValidation result = TriggerCheckDraftSelectionValidator.Validate(
                true,
                "instance-1",
                GameStateViewFactory.HiddenCardId);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(TriggerCheckDraftSelectionValidator.HiddenCardMessage, result.rejection_reason);
        }

        [Test]
        public void VisibleSelectionIsAccepted()
        {
            TriggerCheckDraftSelectionValidation result = TriggerCheckDraftSelectionValidator.Validate(
                true,
                "instance-1",
                "BT01-001TH");

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(string.Empty, result.rejection_reason);
        }
    }
}
