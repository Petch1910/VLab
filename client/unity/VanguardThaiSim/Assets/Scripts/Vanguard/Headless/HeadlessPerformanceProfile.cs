using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessPerformanceProfileRequest
    {
        public const int DefaultRunCount = 3;
        public const int MaxRunCount = 50;

        public int start_seed = HeadlessSimulationRunner.DefaultSeed;
        public int run_count = DefaultRunCount;
        public string ruleset = HeadlessSimulationRunner.DefaultRuleset;
        public string deck_code;

        public HeadlessPerformanceProfileRequest CloneNormalized()
        {
            return new HeadlessPerformanceProfileRequest
            {
                start_seed = start_seed,
                run_count = run_count,
                ruleset = HeadlessSimulationRequest.NormalizeRuleset(ruleset),
                deck_code = deck_code
            };
        }
    }

    [Serializable]
    public sealed class HeadlessPerformanceProfileResult
    {
        public int schema_version = 1;
        public bool accepted;
        public string rejection_reason;
        public string profiler_id;
        public int start_seed;
        public int run_count;
        public int accepted_count;
        public int blocked_count;
        public string ruleset;
        public float total_elapsed_ms;
        public float average_elapsed_ms;
        public float min_elapsed_ms;
        public float max_elapsed_ms;
        public string behavior_policy = "timing_summary_only_no_simulation_changes";
        public List<HeadlessPerformanceRunRecord> runs = new List<HeadlessPerformanceRunRecord>();

        public string ToJson(bool prettyPrint = false)
        {
            if (runs == null)
            {
                runs = new List<HeadlessPerformanceRunRecord>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    [Serializable]
    public sealed class HeadlessPerformanceRunRecord
    {
        public int index;
        public int seed;
        public bool accepted;
        public string failure_reason;
        public float elapsed_ms;
        public int actions_executed;
        public int event_count;
        public string final_phase;
    }

    public static class HeadlessPerformanceProfiler
    {
        public static HeadlessPerformanceProfileResult Run(HeadlessPerformanceProfileRequest request)
        {
            HeadlessPerformanceProfileRequest safeRequest =
                (request ?? new HeadlessPerformanceProfileRequest()).CloneNormalized();

            HeadlessPerformanceProfileResult result = new HeadlessPerformanceProfileResult
            {
                accepted = true,
                profiler_id = BuildProfilerId(safeRequest.start_seed, safeRequest.run_count),
                start_seed = safeRequest.start_seed,
                run_count = safeRequest.run_count,
                ruleset = safeRequest.ruleset
            };

            if (safeRequest.run_count < 1 || safeRequest.run_count > HeadlessPerformanceProfileRequest.MaxRunCount)
            {
                result.accepted = false;
                result.rejection_reason = "run_count must be between 1 and " +
                                          HeadlessPerformanceProfileRequest.MaxRunCount + ".";
                result.run_count = 0;
                return result;
            }

            float min = float.MaxValue;
            float max = 0f;

            for (int i = 0; i < safeRequest.run_count; i++)
            {
                int seed = safeRequest.start_seed + i;
                Stopwatch stopwatch = Stopwatch.StartNew();
                HeadlessSimulationResult simulation = HeadlessSimulationRunner.Run(new HeadlessSimulationRequest
                {
                    seed = seed,
                    ruleset = safeRequest.ruleset,
                    deck_code = safeRequest.deck_code
                });
                stopwatch.Stop();

                float elapsedMs = (float)Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                min = Math.Min(min, elapsedMs);
                max = Math.Max(max, elapsedMs);
                result.total_elapsed_ms = (float)Math.Round(result.total_elapsed_ms + elapsedMs, 3);

                if (simulation.accepted)
                {
                    result.accepted_count++;
                }
                else
                {
                    result.blocked_count++;
                }

                result.runs.Add(new HeadlessPerformanceRunRecord
                {
                    index = i,
                    seed = seed,
                    accepted = simulation.accepted,
                    failure_reason = simulation.failure_reason,
                    elapsed_ms = elapsedMs,
                    actions_executed = simulation.actions_executed,
                    event_count = simulation.event_count,
                    final_phase = simulation.final_phase
                });
            }

            result.min_elapsed_ms = result.runs.Count == 0 ? 0f : min;
            result.max_elapsed_ms = max;
            result.average_elapsed_ms = result.runs.Count == 0
                ? 0f
                : (float)Math.Round(result.total_elapsed_ms / result.runs.Count, 3);
            return result;
        }

        private static string BuildProfilerId(int startSeed, int runCount)
        {
            return "profile-s" + startSeed.ToString() + "-n" + runCount.ToString();
        }
    }
}
