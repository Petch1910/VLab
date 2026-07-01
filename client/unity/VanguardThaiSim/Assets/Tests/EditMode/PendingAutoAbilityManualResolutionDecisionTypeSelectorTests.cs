using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionTypeSelectorTests
    {
        [Test]
        public void NextCyclesThroughSupportedDecisionTypes()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                PendingAutoAbilityManualResolutionDecisionTypeSelector.Next(
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve));
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                PendingAutoAbilityManualResolutionDecisionTypeSelector.Next(
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip));
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                PendingAutoAbilityManualResolutionDecisionTypeSelector.Next(
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer));
        }

        [Test]
        public void InvalidStateFallsBackToResolve()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                PendingAutoAbilityManualResolutionDecisionTypeSelector.Normalize("AutoResolve"));
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                PendingAutoAbilityManualResolutionDecisionTypeSelector.Next("AutoResolve"));
        }

        [Test]
        public void ButtonLabelUsesNormalizedDecisionType()
        {
            Assert.AreEqual(
                "Dec:Resolve",
                PendingAutoAbilityManualResolutionDecisionTypeSelector.FormatButtonLabel(""));
            Assert.AreEqual(
                "Dec:Defer",
                PendingAutoAbilityManualResolutionDecisionTypeSelector.FormatButtonLabel(
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer));
        }
    }
}
