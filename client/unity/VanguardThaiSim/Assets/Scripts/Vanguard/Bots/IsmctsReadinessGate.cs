using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Bots
{
    [Serializable]
    public sealed class IsmctsReadinessItem
    {
        public string requirement_id;
        public string title;
        public bool ready;
        public string evidence;
    }

    [Serializable]
    public sealed class IsmctsReadinessReport
    {
        public string gate_id;
        public bool advanced_search_allowed;
        public int ready_count;
        public int blocked_count;
        public string summary;
        public List<IsmctsReadinessItem> items = new List<IsmctsReadinessItem>();

        public void EnsureLists()
        {
            if (items == null)
            {
                items = new List<IsmctsReadinessItem>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static IsmctsReadinessReport FromJson(string json)
        {
            IsmctsReadinessReport report = JsonUtility.FromJson<IsmctsReadinessReport>(json);
            if (report == null)
            {
                throw new ArgumentException("ISMCTS readiness report JSON could not be parsed.", "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class IsmctsReadinessGate
    {
        public static IsmctsReadinessReport EvaluateDefault()
        {
            return Evaluate(new List<IsmctsReadinessItem>
            {
                Ready("legal-actions", "Bot uses RulesCore legal actions", "M14-01 HeuristicBotV2"),
                Ready("hidden-state", "Bot/search helpers preserve hidden-state boundaries", "M11-09, M14-01, M14-02, M14-08"),
                Ready("snapshot-simulation", "Branch simulation uses cloned snapshots only", "M14-05 SnapshotSimulationPath"),
                Ready("probability-planning-only", "Trigger probability is planning-only and never RNG outcome", "M14-03 TriggerRiskAttackChoice"),
                Ready("guard-and-battle-advisors", "Guard and battle sequence advisors are deterministic", "M14-02, M14-04"),
                Ready("debug-trace", "Decision traces are sanitized and deterministic", "M14-08 BotDebugTrace"),
                Ready("no-mutation-tests", "Current bot/search helpers have no-mutation tests", "Unity EditMode M14 baseline")
            });
        }

        public static IsmctsReadinessReport Evaluate(IReadOnlyList<IsmctsReadinessItem> items)
        {
            var safeItems = new List<IsmctsReadinessItem>();
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    IsmctsReadinessItem item = items[i];
                    if (item == null)
                    {
                        continue;
                    }

                    safeItems.Add(new IsmctsReadinessItem
                    {
                        requirement_id = item.requirement_id ?? string.Empty,
                        title = item.title ?? string.Empty,
                        ready = item.ready,
                        evidence = item.evidence ?? string.Empty
                    });
                }
            }

            int readyCount = 0;
            int blockedCount = 0;
            for (int i = 0; i < safeItems.Count; i++)
            {
                if (safeItems[i].ready)
                {
                    readyCount++;
                }
                else
                {
                    blockedCount++;
                }
            }

            bool allowed = safeItems.Count > 0 && blockedCount == 0;
            return new IsmctsReadinessReport
            {
                gate_id = "M14-09-ISMCTS-readiness",
                advanced_search_allowed = allowed,
                ready_count = readyCount,
                blocked_count = blockedCount,
                summary = allowed
                    ? "Advanced search prototype may proceed."
                    : "Advanced search prototype remains blocked.",
                items = safeItems
            };
        }

        private static IsmctsReadinessItem Ready(string requirementId, string title, string evidence)
        {
            return new IsmctsReadinessItem
            {
                requirement_id = requirementId,
                title = title,
                ready = true,
                evidence = evidence
            };
        }
    }
}
