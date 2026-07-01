using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerResolverTests
    {
        [Test]
        public void CriticalTriggerReturnsPowerAndCriticalBonus()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.Critical);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(10000, result.power_bonus);
            Assert.AreEqual(1, result.critical_bonus);
            Assert.AreEqual(0, result.draw_cards);
        }

        [Test]
        public void DrawTriggerReturnsPowerAndDrawCount()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.Draw);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(10000, result.power_bonus);
            Assert.AreEqual(1, result.draw_cards);
            Assert.AreEqual(0, result.critical_bonus);
        }

        [Test]
        public void FrontTriggerReturnsFrontRowPowerBonus()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.Front);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(0, result.power_bonus);
            Assert.AreEqual(10000, result.front_row_power_bonus);
        }

        [Test]
        public void HealTriggerReturnsHealAttemptFlag()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.Heal);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(10000, result.power_bonus);
            Assert.IsTrue(result.heal_attempt);
        }

        [Test]
        public void OverTriggerReturnsOverTriggerFlagAndLargePower()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.Over);

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.over_trigger);
            Assert.AreEqual(100000000, result.power_bonus);
        }

        [Test]
        public void NoneTriggerReturnsAcceptedZeroBonuses()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.None);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(0, result.power_bonus);
            Assert.AreEqual(0, result.critical_bonus);
            Assert.AreEqual(0, result.draw_cards);
            Assert.AreEqual(0, result.front_row_power_bonus);
            Assert.IsFalse(result.heal_attempt);
            Assert.IsFalse(result.over_trigger);
        }

        [Test]
        public void UnknownTriggerNeedsManualResolution()
        {
            TriggerResolveResult result = TriggerResolver.Resolve(TriggerType.Unknown);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.needs_manual_resolution);
            Assert.AreEqual(TriggerType.Unknown, result.trigger_type);
        }

        [Test]
        public void TriggerResolutionIsDeterministic()
        {
            TriggerResolveResult first = TriggerResolver.Resolve(TriggerType.Critical);
            TriggerResolveResult second = TriggerResolver.Resolve(TriggerType.Critical);

            Assert.AreEqual(first.accepted, second.accepted);
            Assert.AreEqual(first.power_bonus, second.power_bonus);
            Assert.AreEqual(first.critical_bonus, second.critical_bonus);
            Assert.AreEqual(first.explanation, second.explanation);
        }
    }
}
