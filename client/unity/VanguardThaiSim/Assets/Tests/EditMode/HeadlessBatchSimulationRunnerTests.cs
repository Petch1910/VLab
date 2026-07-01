using NUnit.Framework;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class HeadlessBatchSimulationRunnerTests
    {
        [Test]
        public void DefaultBatchRunsThreeAcceptedSequentialSeeds()
        {
            HeadlessBatchSimulationResult result = HeadlessBatchSimulationRunner.RunDefault();

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual(HeadlessBatchSimulationRequest.DefaultRunCount, result.run_count);
            Assert.AreEqual(3, result.accepted_count);
            Assert.AreEqual(0, result.blocked_count);
            Assert.AreEqual(3, result.results.Count);
            Assert.AreEqual(HeadlessSimulationRunner.DefaultSeed, result.results[0].seed);
            Assert.AreEqual(HeadlessSimulationRunner.DefaultSeed + 1, result.results[1].seed);
            Assert.AreEqual(HeadlessSimulationRunner.DefaultSeed + 2, result.results[2].seed);
        }

        [Test]
        public void CustomBatchCountAndStartSeedAreApplied()
        {
            HeadlessBatchSimulationResult result = HeadlessBatchSimulationRunner.Run(new HeadlessBatchSimulationRequest
            {
                start_seed = 200,
                run_count = 2,
                ruleset = "Premium"
            });

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual(200, result.start_seed);
            Assert.AreEqual(2, result.run_count);
            Assert.AreEqual("Premium", result.ruleset);
            Assert.AreEqual(200, result.results[0].seed);
            Assert.AreEqual(201, result.results[1].seed);
        }

        [Test]
        public void BatchRejectsOutOfRangeRunCountBeforeRunning()
        {
            HeadlessBatchSimulationResult result = HeadlessBatchSimulationRunner.Run(new HeadlessBatchSimulationRequest
            {
                run_count = HeadlessBatchSimulationRequest.MaxRunCount + 1
            });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(0, result.results.Count);
            StringAssert.Contains("run_count", result.failure_reason);
        }

        [Test]
        public void BatchCliArgumentsParseCountSeedRulesetDeckCodeAndResultPath()
        {
            HeadlessBatchSimulationCliInput input = HeadlessBatchSimulationCliArguments.Parse(new[]
            {
                HeadlessBatchSimulationCliArguments.BatchCountArgument,
                "5",
                HeadlessBatchSimulationCliArguments.StartSeedArgument,
                "900",
                HeadlessBatchSimulationCliArguments.RulesetArgument,
                "v-premium",
                HeadlessBatchSimulationCliArguments.DeckCodeArgument,
                "VGTH1.example",
                HeadlessBatchSimulationCliArguments.ResultPathArgument,
                "batch.json"
            });

            Assert.IsTrue(input.IsValid);
            Assert.AreEqual(5, input.request.run_count);
            Assert.AreEqual(900, input.request.start_seed);
            Assert.AreEqual("V", input.request.ruleset);
            Assert.AreEqual("VGTH1.example", input.request.deck_code);
            Assert.AreEqual("batch.json", input.result_path);
        }

        [Test]
        public void BatchCliArgumentsRejectInvalidCount()
        {
            HeadlessBatchSimulationCliInput input = HeadlessBatchSimulationCliArguments.Parse(new[]
            {
                HeadlessBatchSimulationCliArguments.BatchCountArgument,
                "abc"
            });

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual(1, input.errors.Count);
        }
    }
}
