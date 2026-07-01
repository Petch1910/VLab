using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessSimulationResult
    {
        public bool accepted;
        public string failure_reason;
        public int seed;
        public string ruleset;
        public string deck_source;
        public string result_path;
        public string replay_path;
        public int actions_executed;
        public int event_count;
        public string final_phase;
        public int player_count;
        public int player0_deck_count;
        public int player0_hand_count;
        public int player0_rear_guard_count;
        public int player0_protect_markers;
        public List<string> action_types = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static HeadlessSimulationResult Failed(string reason, int seed)
        {
            return new HeadlessSimulationResult
            {
                accepted = false,
                failure_reason = string.IsNullOrWhiteSpace(reason) ? "Headless simulation failed." : reason.Trim(),
                seed = seed,
                action_types = new List<string>()
            };
        }

        public void EnsureLists()
        {
            if (action_types == null)
            {
                action_types = new List<string>();
            }
        }
    }
}
