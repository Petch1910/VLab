using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessDatasetExport
    {
        public int schema_version = 1;
        public string dataset_id;
        public string source = "headless_batch";
        public string source_policy = "summary_metrics_no_card_instance_ids";
        public int run_count;
        public int accepted_count;
        public int blocked_count;
        public List<HeadlessDatasetRunRecord> runs = new List<HeadlessDatasetRunRecord>();

        public string ToJson(bool prettyPrint = false)
        {
            if (runs == null)
            {
                runs = new List<HeadlessDatasetRunRecord>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    [Serializable]
    public sealed class HeadlessDatasetRunRecord
    {
        public int index;
        public bool accepted;
        public string failure_reason;
        public int seed;
        public string ruleset;
        public string deck_source;
        public int actions_executed;
        public int event_count;
        public string final_phase;
        public int player0_deck_count;
        public int player0_hand_count;
        public int player0_rear_guard_count;
        public int player0_protect_markers;
    }

    public static class HeadlessDatasetExporter
    {
        public static HeadlessDatasetExport FromBatch(HeadlessBatchSimulationResult batch, string datasetId)
        {
            HeadlessDatasetExport export = new HeadlessDatasetExport
            {
                dataset_id = string.IsNullOrWhiteSpace(datasetId) ? "headless-dataset" : datasetId.Trim()
            };

            if (batch == null)
            {
                return export;
            }

            export.run_count = batch.run_count;
            export.accepted_count = batch.accepted_count;
            export.blocked_count = batch.blocked_count;

            if (batch.results != null)
            {
                for (int i = 0; i < batch.results.Count; i++)
                {
                    export.runs.Add(FromResult(i, batch.results[i]));
                }
            }

            return export;
        }

        private static HeadlessDatasetRunRecord FromResult(int index, HeadlessSimulationResult result)
        {
            if (result == null)
            {
                return new HeadlessDatasetRunRecord
                {
                    index = index,
                    accepted = false,
                    failure_reason = "missing_result"
                };
            }

            return new HeadlessDatasetRunRecord
            {
                index = index,
                accepted = result.accepted,
                failure_reason = result.failure_reason,
                seed = result.seed,
                ruleset = result.ruleset,
                deck_source = result.deck_source,
                actions_executed = result.actions_executed,
                event_count = result.event_count,
                final_phase = result.final_phase,
                player0_deck_count = result.player0_deck_count,
                player0_hand_count = result.player0_hand_count,
                player0_rear_guard_count = result.player0_rear_guard_count,
                player0_protect_markers = result.player0_protect_markers
            };
        }
    }
}
