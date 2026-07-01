using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableActionStatusFormatterTests
    {
        [Test]
        public void CannotDrawFormatsExistingText()
        {
            Assert.AreEqual(
                PlayTableActionStatusFormatter.CannotDrawMessage,
                PlayTableActionStatusFormatter.FormatCannotDraw());
        }

        [Test]
        public void CannotSetPhaseFormatsPhaseText()
        {
            Assert.AreEqual(
                "Cannot set phase to Battle.",
                PlayTableActionStatusFormatter.FormatCannotSetPhase(GamePhase.Battle));
        }

        [Test]
        public void CannotAddGiftMarkerFormatsMarkerText()
        {
            Assert.AreEqual(
                "Cannot add Force marker.",
                PlayTableActionStatusFormatter.FormatCannotAddGiftMarker(GiftMarkerType.Force));
        }

        [Test]
        public void CannotTriggerCheckUsesPlayerFacingText()
        {
            Assert.AreEqual(
                PlayTableActionStatusFormatter.CannotTriggerCheckMessage,
                PlayTableActionStatusFormatter.FormatCannotTriggerCheck());
        }

        [Test]
        public void CannotGuardUsesPlayerFacingText()
        {
            Assert.AreEqual(
                PlayTableActionStatusFormatter.CannotGuardMessage,
                PlayTableActionStatusFormatter.FormatCannotGuard());
        }

        [Test]
        public void CannotAttackUsesPlayerFacingText()
        {
            Assert.AreEqual(
                PlayTableActionStatusFormatter.CannotAttackMessage,
                PlayTableActionStatusFormatter.FormatCannotAttack());
        }

        [Test]
        public void CannotAttackTargetUsesPlayerFacingText()
        {
            Assert.AreEqual(
                PlayTableActionStatusFormatter.CannotAttackTargetMessage,
                PlayTableActionStatusFormatter.FormatCannotAttackTarget());
        }

        [Test]
        public void UndoDisabledOnlineFormatsExistingText()
        {
            Assert.AreEqual(
                PlayTableActionStatusFormatter.UndoDisabledOnlineMessage,
                PlayTableActionStatusFormatter.FormatUndoDisabledOnline());
        }
    }
}
