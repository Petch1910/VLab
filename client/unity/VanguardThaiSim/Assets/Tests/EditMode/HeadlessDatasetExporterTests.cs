using NUnit.Framework;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class HeadlessDatasetExporterTests
    {
        [Test]
        public void ExportFromBatchCreatesDeterministicSummaryRecords()
        {
            HeadlessBatchSimulationResult batch = HeadlessBatchSimulationRunner.Run(new HeadlessBatchSimulationRequest
            {
                start_seed = 500,
                run_count = 2,
                ruleset = "D"
            });

            HeadlessDatasetExport export = HeadlessDatasetExporter.FromBatch(batch, "m17-05-test");

            Assert.AreEqual(1, export.schema_version);
            Assert.AreEqual("m17-05-test", export.dataset_id);
            Assert.AreEqual("headless_batch", export.source);
            Assert.AreEqual("summary_metrics_no_card_instance_ids", export.source_policy);
            Assert.AreEqual(2, export.run_count);
            Assert.AreEqual(2, export.accepted_count);
            Assert.AreEqual(0, export.blocked_count);
            Assert.AreEqual(2, export.runs.Count);
            Assert.AreEqual(0, export.runs[0].index);
            Assert.AreEqual(500, export.runs[0].seed);
            Assert.AreEqual(501, export.runs[1].seed);
            Assert.AreEqual("Main", export.runs[0].final_phase);
            Assert.AreEqual(4, export.runs[0].event_count);
            string json = export.ToJson(false);
            StringAssert.DoesNotContain("\"card_instance_id\":", json);
            StringAssert.DoesNotContain("\"card_instance_ids\":", json);
            StringAssert.DoesNotContain("\"card_id\":", json);
        }

        [Test]
        public void ExportHandlesNullBatchAsEmptyDataset()
        {
            HeadlessDatasetExport export = HeadlessDatasetExporter.FromBatch(null, "empty");

            Assert.AreEqual("empty", export.dataset_id);
            Assert.AreEqual(0, export.run_count);
            Assert.AreEqual(0, export.runs.Count);
        }
    }
}
