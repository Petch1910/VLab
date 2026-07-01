using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class PackedStateDecisionInput
    {
        public bool correctness_gates_stable;
        public bool hidden_state_gates_stable;
        public bool observation_api_stable;
        public bool profiler_baseline_exists;
        public float target_average_elapsed_ms;
        public HeadlessPerformanceProfileResult profiler_result;
    }

    [Serializable]
    public sealed class PackedStateDecisionReport
    {
        public int schema_version = 1;
        public string decision;
        public string live_game_state_policy = "keep_readable_game_state";
        public string packed_state_policy = "no_migration_without_allow_prototype";
        public float target_average_elapsed_ms;
        public float observed_average_elapsed_ms;
        public List<string> blockers = new List<string>();
        public List<string> allowed_next_steps = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            if (blockers == null)
            {
                blockers = new List<string>();
            }

            if (allowed_next_steps == null)
            {
                allowed_next_steps = new List<string>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class PackedStateDecisionGate
    {
        public const string DecisionDefer = "defer";
        public const string DecisionAllowPrototype = "allow_prototype";

        public static PackedStateDecisionReport Evaluate(PackedStateDecisionInput input)
        {
            PackedStateDecisionReport report = new PackedStateDecisionReport
            {
                decision = DecisionDefer
            };

            if (input == null)
            {
                report.blockers.Add("input_missing");
                return report;
            }

            report.target_average_elapsed_ms = input.target_average_elapsed_ms;
            if (input.profiler_result != null)
            {
                report.observed_average_elapsed_ms = input.profiler_result.average_elapsed_ms;
            }

            AddSafetyBlockers(report.blockers, input);
            AddProfilerBlockers(report.blockers, input);

            if (report.blockers.Count > 0)
            {
                return report;
            }

            if (input.profiler_result.average_elapsed_ms <= input.target_average_elapsed_ms)
            {
                report.blockers.Add("current_profile_within_target_keep_readable_state");
                return report;
            }

            report.decision = DecisionAllowPrototype;
            report.allowed_next_steps.Add("prototype_converter_outside_live_GameState");
            report.allowed_next_steps.Add("add_golden_tests_readable_to_packed_round_trip");
            report.allowed_next_steps.Add("keep_readable_GameState_as_source_of_truth");
            return report;
        }

        private static void AddSafetyBlockers(List<string> blockers, PackedStateDecisionInput input)
        {
            if (!input.correctness_gates_stable)
            {
                blockers.Add("correctness_gates_not_stable");
            }

            if (!input.hidden_state_gates_stable)
            {
                blockers.Add("hidden_state_gates_not_stable");
            }

            if (!input.observation_api_stable)
            {
                blockers.Add("observation_api_not_stable");
            }
        }

        private static void AddProfilerBlockers(List<string> blockers, PackedStateDecisionInput input)
        {
            if (!input.profiler_baseline_exists)
            {
                blockers.Add("profiler_baseline_missing");
            }

            if (input.profiler_result == null)
            {
                blockers.Add("profiler_result_missing");
                return;
            }

            if (!input.profiler_result.accepted)
            {
                blockers.Add("profiler_result_not_accepted");
            }

            if (input.profiler_result.run_count <= 0)
            {
                blockers.Add("profiler_result_empty");
            }

            if (input.target_average_elapsed_ms <= 0f)
            {
                blockers.Add("target_average_elapsed_ms_missing");
            }
        }
    }
}
