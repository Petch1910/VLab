using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerProbabilityEngineTests
    {
        [Test]
        public void OneCheckProbabilityMatchesTriggerRatio()
        {
            bool ok = TriggerProbabilityEngine.TryCalculate(
                50,
                16,
                1,
                out TriggerProbabilityResult result);

            Assert.IsTrue(ok);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(16d / 50d, result.ProbabilityAtLeastOneTrigger, 0.0000001d);
            Assert.AreEqual(34d / 50d, result.GetProbabilityForHits(0), 0.0000001d);
            Assert.AreEqual(16d / 50d, result.GetProbabilityForHits(1), 0.0000001d);
        }

        [Test]
        public void TwoCheckDistributionMatchesHypergeometricExpectation()
        {
            bool ok = TriggerProbabilityEngine.TryCalculate(
                50,
                16,
                2,
                out TriggerProbabilityResult result);

            Assert.IsTrue(ok);
            Assert.AreEqual(3, result.HitDistribution.Count);
            Assert.AreEqual(561d / 1225d, result.GetProbabilityForHits(0), 0.0000001d);
            Assert.AreEqual(544d / 1225d, result.GetProbabilityForHits(1), 0.0000001d);
            Assert.AreEqual(120d / 1225d, result.GetProbabilityForHits(2), 0.0000001d);
            Assert.AreEqual(664d / 1225d, result.ProbabilityAtLeastOneTrigger, 0.0000001d);
        }

        [Test]
        public void ZeroTriggerPoolAlwaysMisses()
        {
            bool ok = TriggerProbabilityEngine.TryCalculate(
                12,
                0,
                2,
                out TriggerProbabilityResult result);

            Assert.IsTrue(ok);
            Assert.AreEqual(1d, result.GetProbabilityForHits(0), 0.0000001d);
            Assert.AreEqual(0d, result.GetProbabilityForHits(1), 0.0000001d);
            Assert.AreEqual(0d, result.GetProbabilityForHits(2), 0.0000001d);
            Assert.AreEqual(0d, result.ProbabilityAtLeastOneTrigger, 0.0000001d);
        }

        [Test]
        public void InvalidInputsReturnClearFailure()
        {
            bool tooManyTriggers = TriggerProbabilityEngine.TryCalculate(
                10,
                11,
                1,
                out TriggerProbabilityResult triggerResult);

            Assert.IsFalse(tooManyTriggers);
            Assert.IsFalse(triggerResult.IsValid);
            Assert.AreEqual("TRIGGER_CARDS_EXCEED_TOTAL", triggerResult.ErrorCode);

            bool tooManyChecks = TriggerProbabilityEngine.TryCalculate(
                10,
                4,
                11,
                out TriggerProbabilityResult checkResult);

            Assert.IsFalse(tooManyChecks);
            Assert.IsFalse(checkResult.IsValid);
            Assert.AreEqual("CHECK_COUNT_EXCEEDS_TOTAL", checkResult.ErrorCode);
        }

        [Test]
        public void CardListCalculationIgnoresOrder()
        {
            var firstOrder = new List<GameCardInstance>
            {
                new GameCardInstance("a", "TRIGGER-1", 0),
                new GameCardInstance("b", "NORMAL-1", 0),
                new GameCardInstance("c", "TRIGGER-2", 0),
                new GameCardInstance("d", "NORMAL-2", 0)
            };
            var secondOrder = new List<GameCardInstance>
            {
                firstOrder[3],
                firstOrder[2],
                firstOrder[1],
                firstOrder[0]
            };

            bool firstOk = TriggerProbabilityEngine.TryCalculateFromCards(
                firstOrder,
                IsTestTrigger,
                2,
                out TriggerProbabilityResult first);
            bool secondOk = TriggerProbabilityEngine.TryCalculateFromCards(
                secondOrder,
                IsTestTrigger,
                2,
                out TriggerProbabilityResult second);

            Assert.IsTrue(firstOk);
            Assert.IsTrue(secondOk);
            Assert.AreEqual(first.GetProbabilityForHits(0), second.GetProbabilityForHits(0), 0.0000001d);
            Assert.AreEqual(first.GetProbabilityForHits(1), second.GetProbabilityForHits(1), 0.0000001d);
            Assert.AreEqual(first.GetProbabilityForHits(2), second.GetProbabilityForHits(2), 0.0000001d);
        }

        private static bool IsTestTrigger(GameCardInstance card)
        {
            return card != null && card.card_id.StartsWith("TRIGGER-");
        }
    }
}
