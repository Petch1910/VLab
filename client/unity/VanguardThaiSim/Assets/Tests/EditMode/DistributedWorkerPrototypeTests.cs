using NUnit.Framework;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class DistributedWorkerPrototypeTests
    {
        [Test]
        public void WorkerRunsBoundedBatchAndDatasetExport()
        {
            DistributedWorkerPrototypeResult result = DistributedWorkerPrototype.Run(new DistributedWorkerPrototypeRequest
            {
                worker_id = "worker-a",
                batch_request = new HeadlessBatchSimulationRequest
                {
                    start_seed = 900,
                    run_count = 2,
                    ruleset = "D"
                },
                export_dataset = true
            });

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual("worker-a", result.worker_id);
            Assert.AreEqual("local_bounded_no_network_no_cluster_no_RL", result.worker_policy);
            Assert.IsTrue(result.spec_validation.accepted);
            Assert.NotNull(result.batch_result);
            Assert.AreEqual(2, result.batch_result.run_count);
            Assert.AreEqual(2, result.batch_result.accepted_count);
            Assert.NotNull(result.dataset_export);
            Assert.AreEqual("worker-a-dataset", result.dataset_export.dataset_id);
            Assert.AreEqual(2, result.dataset_export.run_count);

            string json = result.ToJson(false);
            StringAssert.DoesNotContain("\"card_instance_id\":", json);
            StringAssert.DoesNotContain("\"card_id\":", json);
            StringAssert.DoesNotContain("Photon", json);
        }

        [Test]
        public void WorkerRejectsInvalidSpecBeforeRunning()
        {
            DistributedWorkerPrototypeSpecDocument spec = DistributedWorkerPrototypeSpec.CreateDefault();
            spec.non_goals.Remove("no_RL_training");

            DistributedWorkerPrototypeResult result = DistributedWorkerPrototype.Run(
                new DistributedWorkerPrototypeRequest(),
                spec);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("worker_spec_rejected", result.rejection_reason);
            Assert.IsFalse(result.spec_validation.accepted);
            Assert.IsNull(result.batch_result);
            CollectionAssert.Contains(result.spec_validation.errors, "missing_no_RL_training_non_goal");
        }

        [Test]
        public void WorkerRejectsRunCountOutsideSpecLimit()
        {
            DistributedWorkerPrototypeSpecDocument spec = DistributedWorkerPrototypeSpec.CreateDefault();
            spec.max_worker_run_count = 1;

            DistributedWorkerPrototypeResult result = DistributedWorkerPrototype.Run(
                new DistributedWorkerPrototypeRequest
                {
                    batch_request = new HeadlessBatchSimulationRequest
                    {
                        run_count = 2
                    }
                },
                spec);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("worker_run_count_out_of_bounds", result.rejection_reason);
            Assert.IsNull(result.batch_result);
        }

        [Test]
        public void WorkerCanIncludeOptionalTimingProfile()
        {
            DistributedWorkerPrototypeResult result = DistributedWorkerPrototype.Run(new DistributedWorkerPrototypeRequest
            {
                worker_id = "worker-profile",
                batch_request = new HeadlessBatchSimulationRequest
                {
                    start_seed = 910,
                    run_count = 1,
                    ruleset = "standard"
                },
                include_profile = true
            });

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.NotNull(result.performance_profile);
            Assert.IsTrue(result.performance_profile.accepted);
            Assert.AreEqual(1, result.performance_profile.run_count);
            Assert.AreEqual("D", result.performance_profile.ruleset);
        }
    }
}
