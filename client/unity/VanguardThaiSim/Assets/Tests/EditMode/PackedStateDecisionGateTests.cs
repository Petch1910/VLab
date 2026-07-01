using NUnit.Framework;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class PackedStateDecisionGateTests
    {
        [Test]
        public void MissingInputDefersWithReadableStatePolicy()
        {
            PackedStateDecisionReport report = PackedStateDecisionGate.Evaluate(null);

            Assert.AreEqual(PackedStateDecisionGate.DecisionDefer, report.decision);
            Assert.AreEqual("keep_readable_game_state", report.live_game_state_policy);
            Assert.AreEqual("no_migration_without_allow_prototype", report.packed_state_policy);
            CollectionAssert.Contains(report.blockers, "input_missing");
        }

        [Test]
        public void SafetyGateFailureDefersEvenWithSlowProfile()
        {
            PackedStateDecisionReport report = PackedStateDecisionGate.Evaluate(new PackedStateDecisionInput
            {
                correctness_gates_stable = false,
                hidden_state_gates_stable = true,
                observation_api_stable = true,
                profiler_baseline_exists = true,
                target_average_elapsed_ms = 1f,
                profiler_result = AcceptedProfile(10f)
            });

            Assert.AreEqual(PackedStateDecisionGate.DecisionDefer, report.decision);
            CollectionAssert.Contains(report.blockers, "correctness_gates_not_stable");
            Assert.AreEqual(0, report.allowed_next_steps.Count);
        }

        [Test]
        public void ProfileWithinTargetDefersPackedStateMigration()
        {
            PackedStateDecisionReport report = PackedStateDecisionGate.Evaluate(new PackedStateDecisionInput
            {
                correctness_gates_stable = true,
                hidden_state_gates_stable = true,
                observation_api_stable = true,
                profiler_baseline_exists = true,
                target_average_elapsed_ms = 20f,
                profiler_result = AcceptedProfile(5f)
            });

            Assert.AreEqual(PackedStateDecisionGate.DecisionDefer, report.decision);
            CollectionAssert.Contains(report.blockers, "current_profile_within_target_keep_readable_state");
            Assert.AreEqual(5f, report.observed_average_elapsed_ms);
        }

        [Test]
        public void StableSlowProfileAllowsPrototypeOnly()
        {
            PackedStateDecisionReport report = PackedStateDecisionGate.Evaluate(new PackedStateDecisionInput
            {
                correctness_gates_stable = true,
                hidden_state_gates_stable = true,
                observation_api_stable = true,
                profiler_baseline_exists = true,
                target_average_elapsed_ms = 2f,
                profiler_result = AcceptedProfile(10f)
            });

            Assert.AreEqual(PackedStateDecisionGate.DecisionAllowPrototype, report.decision);
            Assert.AreEqual(0, report.blockers.Count);
            CollectionAssert.Contains(report.allowed_next_steps, "prototype_converter_outside_live_GameState");
            CollectionAssert.Contains(report.allowed_next_steps, "keep_readable_GameState_as_source_of_truth");
            StringAssert.Contains("no_migration_without_allow_prototype", report.ToJson(false));
        }

        private static HeadlessPerformanceProfileResult AcceptedProfile(float averageMs)
        {
            return new HeadlessPerformanceProfileResult
            {
                accepted = true,
                run_count = 2,
                accepted_count = 2,
                average_elapsed_ms = averageMs
            };
        }
    }
}
