using System;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class DistributedWorkerPrototypeRequest
    {
        public string worker_id = "local-worker-0";
        public HeadlessBatchSimulationRequest batch_request = new HeadlessBatchSimulationRequest();
        public bool export_dataset = true;
        public bool include_profile;

        public DistributedWorkerPrototypeRequest CloneNormalized()
        {
            return new DistributedWorkerPrototypeRequest
            {
                worker_id = string.IsNullOrWhiteSpace(worker_id) ? "local-worker-0" : worker_id.Trim(),
                batch_request = (batch_request ?? new HeadlessBatchSimulationRequest()).CloneNormalized(),
                export_dataset = export_dataset,
                include_profile = include_profile
            };
        }
    }

    [Serializable]
    public sealed class DistributedWorkerPrototypeResult
    {
        public int schema_version = 1;
        public bool accepted;
        public string rejection_reason;
        public string worker_id;
        public string worker_policy = "local_bounded_no_network_no_cluster_no_RL";
        public DistributedWorkerPrototypeSpecValidationResult spec_validation;
        public HeadlessBatchSimulationResult batch_result;
        public HeadlessDatasetExport dataset_export;
        public HeadlessPerformanceProfileResult performance_profile;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class DistributedWorkerPrototype
    {
        public static DistributedWorkerPrototypeResult Run(
            DistributedWorkerPrototypeRequest request,
            DistributedWorkerPrototypeSpecDocument spec = null)
        {
            DistributedWorkerPrototypeRequest safeRequest =
                (request ?? new DistributedWorkerPrototypeRequest()).CloneNormalized();
            DistributedWorkerPrototypeSpecDocument safeSpec =
                spec ?? DistributedWorkerPrototypeSpec.CreateDefault();
            DistributedWorkerPrototypeSpecValidationResult specValidation =
                DistributedWorkerPrototypeSpec.Validate(safeSpec);

            DistributedWorkerPrototypeResult result = new DistributedWorkerPrototypeResult
            {
                accepted = false,
                worker_id = safeRequest.worker_id,
                spec_validation = specValidation
            };

            if (!specValidation.accepted)
            {
                result.rejection_reason = "worker_spec_rejected";
                return result;
            }

            if (safeRequest.batch_request.run_count < 1 ||
                safeRequest.batch_request.run_count > safeSpec.max_worker_run_count)
            {
                result.rejection_reason = "worker_run_count_out_of_bounds";
                return result;
            }

            HeadlessBatchSimulationResult batch = HeadlessBatchSimulationRunner.Run(safeRequest.batch_request);
            result.batch_result = batch;
            result.accepted = batch.accepted;
            result.rejection_reason = batch.accepted ? string.Empty : batch.failure_reason;

            if (safeRequest.export_dataset)
            {
                result.dataset_export = HeadlessDatasetExporter.FromBatch(
                    batch,
                    safeRequest.worker_id + "-dataset");
            }

            if (safeRequest.include_profile)
            {
                result.performance_profile = HeadlessPerformanceProfiler.Run(new HeadlessPerformanceProfileRequest
                {
                    start_seed = safeRequest.batch_request.start_seed,
                    run_count = safeRequest.batch_request.run_count,
                    ruleset = safeRequest.batch_request.ruleset,
                    deck_code = safeRequest.batch_request.deck_code
                });
            }

            return result;
        }
    }
}
