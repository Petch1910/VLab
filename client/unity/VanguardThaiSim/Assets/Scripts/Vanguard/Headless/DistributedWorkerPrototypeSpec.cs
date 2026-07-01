using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class DistributedWorkerPrototypeSpecDocument
    {
        public int schema_version = 1;
        public string milestone = "M17-09";
        public string prototype_status = "spec_only";
        public int max_worker_run_count = HeadlessPerformanceProfileRequest.MaxRunCount;
        public string transport_policy = "local_process_or_container_later_no_live_photon";
        public string state_policy = "readable_GameState_source_of_truth";
        public List<string> input_contract = new List<string>();
        public List<string> output_contract = new List<string>();
        public List<string> artifact_policy = new List<string>();
        public List<string> safety_limits = new List<string>();
        public List<string> non_goals = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public void EnsureLists()
        {
            if (input_contract == null) input_contract = new List<string>();
            if (output_contract == null) output_contract = new List<string>();
            if (artifact_policy == null) artifact_policy = new List<string>();
            if (safety_limits == null) safety_limits = new List<string>();
            if (non_goals == null) non_goals = new List<string>();
        }
    }

    [Serializable]
    public sealed class DistributedWorkerPrototypeSpecValidationResult
    {
        public bool accepted;
        public List<string> errors = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class DistributedWorkerPrototypeSpec
    {
        public static DistributedWorkerPrototypeSpecDocument CreateDefault()
        {
            return new DistributedWorkerPrototypeSpecDocument
            {
                input_contract = new List<string>
                {
                    "HeadlessBatchSimulationRequest",
                    "HeadlessSimulationRequest",
                    "seed_range",
                    "ruleset",
                    "deck_code_optional"
                },
                output_contract = new List<string>
                {
                    "HeadlessBatchSimulationResult",
                    "HeadlessDatasetExport",
                    "HeadlessPerformanceProfileResult",
                    "worker_status_summary"
                },
                artifact_policy = new List<string>
                {
                    "json_artifacts_only",
                    "no_card_instance_ids",
                    "no_hidden_state",
                    "no_full_replay_by_default"
                },
                safety_limits = new List<string>
                {
                    "max_worker_run_count_50",
                    "no_live_network_transport",
                    "no_GameState_mutation_outside_headless_runner",
                    "no_packed_state_migration"
                },
                non_goals = new List<string>
                {
                    "no_RL_training",
                    "no_distributed_cluster",
                    "no_Photon_room_worker",
                    "no_live_UI",
                    "no_ranked_secure_server"
                }
            };
        }

        public static DistributedWorkerPrototypeSpecValidationResult Validate(
            DistributedWorkerPrototypeSpecDocument spec)
        {
            DistributedWorkerPrototypeSpecValidationResult result =
                new DistributedWorkerPrototypeSpecValidationResult();
            if (spec == null)
            {
                result.errors.Add("spec_missing");
                return result;
            }

            spec.EnsureLists();
            Require(result, spec.schema_version == 1, "schema_version_must_be_1");
            Require(result, spec.milestone == "M17-09", "milestone_must_be_M17-09");
            Require(result, spec.prototype_status == "spec_only", "prototype_status_must_be_spec_only");
            Require(result, spec.max_worker_run_count > 0, "max_worker_run_count_missing");
            Require(result, spec.max_worker_run_count <= HeadlessPerformanceProfileRequest.MaxRunCount,
                "max_worker_run_count_exceeds_headless_limit");
            RequireContains(result, spec.input_contract, "HeadlessBatchSimulationRequest", "missing_batch_input_contract");
            RequireContains(result, spec.output_contract, "HeadlessDatasetExport", "missing_dataset_output_contract");
            RequireContains(result, spec.artifact_policy, "no_card_instance_ids", "missing_no_card_instance_ids_policy");
            RequireContains(result, spec.artifact_policy, "no_hidden_state", "missing_no_hidden_state_policy");
            RequireContains(result, spec.safety_limits, "no_live_network_transport", "missing_no_live_network_limit");
            RequireContains(result, spec.safety_limits, "no_packed_state_migration", "missing_no_packed_state_migration_limit");
            RequireContains(result, spec.non_goals, "no_RL_training", "missing_no_RL_training_non_goal");
            RequireContains(result, spec.non_goals, "no_distributed_cluster", "missing_no_cluster_non_goal");

            result.accepted = result.errors.Count == 0;
            return result;
        }

        private static void Require(
            DistributedWorkerPrototypeSpecValidationResult result,
            bool condition,
            string error)
        {
            if (!condition)
            {
                result.errors.Add(error);
            }
        }

        private static void RequireContains(
            DistributedWorkerPrototypeSpecValidationResult result,
            List<string> values,
            string expected,
            string error)
        {
            if (values == null || !values.Contains(expected))
            {
                result.errors.Add(error);
            }
        }
    }
}
