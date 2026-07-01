using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftSelectorCycleHelperTests
    {
        [Test]
        public void TriggerTypeCompletesExistingCycle()
        {
            Assert.AreEqual(
                TriggerType.Critical,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.Unknown));
            Assert.AreEqual(
                TriggerType.Draw,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.Critical));
            Assert.AreEqual(
                TriggerType.Front,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.Draw));
            Assert.AreEqual(
                TriggerType.Heal,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.Front));
            Assert.AreEqual(
                TriggerType.Over,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.Heal));
            Assert.AreEqual(
                TriggerType.None,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.Over));
            Assert.AreEqual(
                TriggerType.Unknown,
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(TriggerType.None));
        }

        [Test]
        public void CheckSourceCompletesExistingCycle()
        {
            Assert.AreEqual(
                TriggerCheckSource.Drive,
                TriggerCheckDraftSelectorCycleHelper.NextCheckSource(TriggerCheckSource.Manual));
            Assert.AreEqual(
                TriggerCheckSource.Damage,
                TriggerCheckDraftSelectorCycleHelper.NextCheckSource(TriggerCheckSource.Drive));
            Assert.AreEqual(
                TriggerCheckSource.Manual,
                TriggerCheckDraftSelectorCycleHelper.NextCheckSource(TriggerCheckSource.Damage));
        }

        [Test]
        public void CheckIndexWrapsFromThreeToZero()
        {
            Assert.AreEqual(1, TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(0));
            Assert.AreEqual(2, TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(1));
            Assert.AreEqual(3, TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(2));
            Assert.AreEqual(0, TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(3));
        }

        [Test]
        public void NegativeCheckIndexNormalizesIntoRange()
        {
            Assert.AreEqual(0, TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(-1));
            Assert.AreEqual(3, TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(-2));
        }
    }
}
