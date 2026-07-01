using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Smoke
{
    [Serializable]
    public sealed class ClientSmokeFlowReport
    {
        public List<string> steps = new List<string>();
        public List<string> blockers = new List<string>();

        public bool IsPass
        {
            get { return blockers == null || blockers.Count == 0; }
        }

        public void AddStep(string step)
        {
            if (steps == null)
            {
                steps = new List<string>();
            }

            steps.Add(string.IsNullOrWhiteSpace(step) ? "Unnamed smoke step passed." : step.Trim());
        }

        public void AddBlocker(string blocker)
        {
            if (blockers == null)
            {
                blockers = new List<string>();
            }

            blockers.Add(string.IsNullOrWhiteSpace(blocker) ? "Unknown smoke blocker." : blocker.Trim());
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        private void EnsureLists()
        {
            if (steps == null)
            {
                steps = new List<string>();
            }

            if (blockers == null)
            {
                blockers = new List<string>();
            }
        }
    }
}
