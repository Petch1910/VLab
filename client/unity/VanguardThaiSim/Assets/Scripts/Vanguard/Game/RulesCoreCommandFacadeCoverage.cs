using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class RulesCoreCommandFacadeCoverageEntry
    {
        public string command_id;
        public string action_type;
        public bool legal_action_generator_covered;
        public bool legal_action_executor_covered;
        public bool rules_core_matcher_covered;
        public string mutation_service_method;
        public bool facade_covered;
        public string exception_reason;
    }

    [Serializable]
    public sealed class RulesCoreCommandFacadeCoverageReport
    {
        public List<RulesCoreCommandFacadeCoverageEntry> entries =
            new List<RulesCoreCommandFacadeCoverageEntry>();

        public void EnsureLists()
        {
            if (entries == null)
            {
                entries = new List<RulesCoreCommandFacadeCoverageEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static RulesCoreCommandFacadeCoverageReport FromJson(string json)
        {
            RulesCoreCommandFacadeCoverageReport report =
                JsonUtility.FromJson<RulesCoreCommandFacadeCoverageReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "RulesCore command facade coverage report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class RulesCoreCommandFacadeCoverage
    {
        public const string UndoLastCommandId = "GameAction.UndoLast";

        public static RulesCoreCommandFacadeCoverageReport CreateCurrentReport()
        {
            var report = new RulesCoreCommandFacadeCoverageReport();

            AddCovered(
                report,
                PhaseTimingMatrixCommandIds.Draw,
                GameActionType.Draw,
                "GameActionService.Draw");
            AddCovered(
                report,
                PhaseTimingMatrixCommandIds.MoveCard,
                GameActionType.MoveCard,
                "GameActionService.MoveCard");
            AddCovered(
                report,
                PhaseTimingMatrixCommandIds.SetPhase,
                GameActionType.SetPhase,
                "GameActionService.SetPhase");
            AddCovered(
                report,
                PhaseTimingMatrixCommandIds.AddGiftMarker,
                GameActionType.AddGiftMarker,
                "GameActionService.AddGiftMarker");
            AddCovered(
                report,
                PhaseTimingMatrixCommandIds.ResourceFlip,
                GameActionType.ResourceFlip,
                "GameActionService.ResourceFlip");

            AddException(
                report,
                UndoLastCommandId,
                "UndoLast",
                "GameActionService.UndoLast",
                "Undo remains a manual/replay utility outside RulesCore command facade until event-sourcing expansion.");

            return report;
        }

        public static RulesCoreCommandFacadeCoverageEntry Find(
            RulesCoreCommandFacadeCoverageReport report,
            string commandId)
        {
            if (report == null || report.entries == null)
            {
                return null;
            }

            for (int i = 0; i < report.entries.Count; i++)
            {
                RulesCoreCommandFacadeCoverageEntry entry = report.entries[i];
                if (entry != null && string.Equals(entry.command_id, commandId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        public static bool IsFacadeCovered(RulesCoreCommandFacadeCoverageReport report, string commandId)
        {
            RulesCoreCommandFacadeCoverageEntry entry = Find(report, commandId);
            return entry != null && entry.facade_covered;
        }

        public static int CountExceptions(RulesCoreCommandFacadeCoverageReport report)
        {
            if (report == null || report.entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < report.entries.Count; i++)
            {
                RulesCoreCommandFacadeCoverageEntry entry = report.entries[i];
                if (entry != null && !entry.facade_covered)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AddCovered(
            RulesCoreCommandFacadeCoverageReport report,
            string commandId,
            GameActionType actionType,
            string serviceMethod)
        {
            Add(
                report,
                commandId,
                actionType.ToString(),
                true,
                true,
                true,
                serviceMethod,
                true,
                string.Empty);
        }

        private static void AddException(
            RulesCoreCommandFacadeCoverageReport report,
            string commandId,
            string actionType,
            string serviceMethod,
            string exceptionReason)
        {
            Add(
                report,
                commandId,
                actionType,
                false,
                false,
                false,
                serviceMethod,
                false,
                exceptionReason);
        }

        private static void Add(
            RulesCoreCommandFacadeCoverageReport report,
            string commandId,
            string actionType,
            bool generatorCovered,
            bool executorCovered,
            bool matcherCovered,
            string serviceMethod,
            bool facadeCovered,
            string exceptionReason)
        {
            report.EnsureLists();
            report.entries.Add(new RulesCoreCommandFacadeCoverageEntry
            {
                command_id = commandId ?? string.Empty,
                action_type = actionType ?? string.Empty,
                legal_action_generator_covered = generatorCovered,
                legal_action_executor_covered = executorCovered,
                rules_core_matcher_covered = matcherCovered,
                mutation_service_method = serviceMethod ?? string.Empty,
                facade_covered = facadeCovered,
                exception_reason = exceptionReason ?? string.Empty
            });
        }
    }
}
