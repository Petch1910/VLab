using NUnit.Framework;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class HeadlessPerformanceProfilerTests
    {
        [Test]
        public void ProfilerRunsBoundedHeadlessSimulationsAndSummaries()
        {
            HeadlessPerformanceProfileResult result = HeadlessPerformanceProfiler.Run(new HeadlessPerformanceProfileRequest
            {
                start_seed = 700,
                run_count = 2,
                ruleset = "D"
            });

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual(1, result.schema_version);
            Assert.AreEqual("profile-s700-n2", result.profiler_id);
            Assert.AreEqual(2, result.run_count);
            Assert.AreEqual(2, result.accepted_count);
            Assert.AreEqual(0, result.blocked_count);
            Assert.AreEqual(2, result.runs.Count);
            Assert.AreEqual(700, result.runs[0].seed);
            Assert.AreEqual(701, result.runs[1].seed);
            Assert.AreEqual(4, result.runs[0].actions_executed);
            Assert.AreEqual(4, result.runs[0].event_count);
            Assert.AreEqual("Main", result.runs[0].final_phase);
            Assert.GreaterOrEqual(result.total_elapsed_ms, 0f);
            Assert.GreaterOrEqual(result.average_elapsed_ms, 0f);
            Assert.GreaterOrEqual(result.max_elapsed_ms, result.min_elapsed_ms);
            Assert.AreEqual("timing_summary_only_no_simulation_changes", result.behavior_policy);
        }

        [Test]
        public void ProfilerRejectsOutOfRangeRunCountBeforeSimulation()
        {
            HeadlessPerformanceProfileResult result = HeadlessPerformanceProfiler.Run(new HeadlessPerformanceProfileRequest
            {
                run_count = HeadlessPerformanceProfileRequest.MaxRunCount + 1
            });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(0, result.run_count);
            Assert.AreEqual(0, result.runs.Count);
            StringAssert.Contains("run_count must be between 1", result.rejection_reason);
        }

        [Test]
        public void ProfilerKeepsSimulationSummaryStableAcrossRuns()
        {
            HeadlessPerformanceProfileResult first = HeadlessPerformanceProfiler.Run(new HeadlessPerformanceProfileRequest
            {
                start_seed = 710,
                run_count = 1,
                ruleset = "standard"
            });
            HeadlessPerformanceProfileResult second = HeadlessPerformanceProfiler.Run(new HeadlessPerformanceProfileRequest
            {
                start_seed = 710,
                run_count = 1,
                ruleset = "standard"
            });

            Assert.IsTrue(first.accepted, first.ToJson(true));
            Assert.IsTrue(second.accepted, second.ToJson(true));
            Assert.AreEqual("D", first.ruleset);
            Assert.AreEqual(first.runs[0].seed, second.runs[0].seed);
            Assert.AreEqual(first.runs[0].accepted, second.runs[0].accepted);
            Assert.AreEqual(first.runs[0].actions_executed, second.runs[0].actions_executed);
            Assert.AreEqual(first.runs[0].event_count, second.runs[0].event_count);
            Assert.AreEqual(first.runs[0].final_phase, second.runs[0].final_phase);
        }
    }
}
