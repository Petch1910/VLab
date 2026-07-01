using NUnit.Framework;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class DistributedWorkerPrototypeSpecTests
    {
        [Test]
        public void DefaultSpecValidatesRequiredWorkerBoundaries()
        {
            DistributedWorkerPrototypeSpecDocument spec = DistributedWorkerPrototypeSpec.CreateDefault();
            DistributedWorkerPrototypeSpecValidationResult result = DistributedWorkerPrototypeSpec.Validate(spec);

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual("M17-09", spec.milestone);
            Assert.AreEqual("spec_only", spec.prototype_status);
            Assert.AreEqual("local_process_or_container_later_no_live_photon", spec.transport_policy);
            CollectionAssert.Contains(spec.input_contract, "HeadlessBatchSimulationRequest");
            CollectionAssert.Contains(spec.output_contract, "HeadlessDatasetExport");
            CollectionAssert.Contains(spec.artifact_policy, "no_card_instance_ids");
            CollectionAssert.Contains(spec.safety_limits, "no_live_network_transport");
            CollectionAssert.Contains(spec.non_goals, "no_RL_training");
        }

        [Test]
        public void MissingHiddenStatePolicyRejectsSpec()
        {
            DistributedWorkerPrototypeSpecDocument spec = DistributedWorkerPrototypeSpec.CreateDefault();
            spec.artifact_policy.Remove("no_hidden_state");

            DistributedWorkerPrototypeSpecValidationResult result = DistributedWorkerPrototypeSpec.Validate(spec);

            Assert.IsFalse(result.accepted);
            CollectionAssert.Contains(result.errors, "missing_no_hidden_state_policy");
        }

        [Test]
        public void ExcessiveRunCountRejectsSpec()
        {
            DistributedWorkerPrototypeSpecDocument spec = DistributedWorkerPrototypeSpec.CreateDefault();
            spec.max_worker_run_count = HeadlessPerformanceProfileRequest.MaxRunCount + 1;

            DistributedWorkerPrototypeSpecValidationResult result = DistributedWorkerPrototypeSpec.Validate(spec);

            Assert.IsFalse(result.accepted);
            CollectionAssert.Contains(result.errors, "max_worker_run_count_exceeds_headless_limit");
        }

        [Test]
        public void SpecJsonKeepsPrototypeNonGoalsVisible()
        {
            DistributedWorkerPrototypeSpecDocument spec = DistributedWorkerPrototypeSpec.CreateDefault();
            string json = spec.ToJson(false);

            StringAssert.Contains("spec_only", json);
            StringAssert.Contains("no_distributed_cluster", json);
            StringAssert.Contains("no_Photon_room_worker", json);
            StringAssert.Contains("no_card_instance_ids", json);
        }
    }
}
