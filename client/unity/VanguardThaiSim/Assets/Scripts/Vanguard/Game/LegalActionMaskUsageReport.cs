using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class LegalActionMaskUsageEntry
    {
        public string path_id;
        public string layer;
        public bool uses_legal_action_generator;
        public bool validates_with_rules_core;
        public bool mutates_directly;
        public bool hardened;
        public string exception_reason;
    }

    [Serializable]
    public sealed class LegalActionMaskUsageReport
    {
        public List<LegalActionMaskUsageEntry> entries = new List<LegalActionMaskUsageEntry>();

        public void EnsureLists()
        {
            if (entries == null)
            {
                entries = new List<LegalActionMaskUsageEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static LegalActionMaskUsageReport FromJson(string json)
        {
            LegalActionMaskUsageReport report = JsonUtility.FromJson<LegalActionMaskUsageReport>(json);
            if (report == null)
            {
                throw new ArgumentException("Legal action mask usage report JSON could not be parsed.", "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class LegalActionMaskUsagePathIds
    {
        public const string PlayTableDraw = "PlayTable.Draw";
        public const string PlayTableMoveSelected = "PlayTable.MoveSelected";
        public const string PlayTableSetPhase = "PlayTable.SetPhase";
        public const string PlayTableAddGiftMarker = "PlayTable.AddGiftMarker";
        public const string PlayTableUndo = "PlayTable.Undo";
        public const string EasyBotDecision = "EasyBot.Decision";
        public const string ProfileBotDecision = "ProfileBot.Decision";
        public const string BotDecisionContext = "Bot.DecisionContext";
        public const string HeuristicBotV2Decision = "HeuristicBotV2.Decision";
        public const string PlaybookBotDecision = "PlaybookBot.Decision";
        public const string MultiplayerSessionLocalAction = "MultiplayerSession.ExecuteLocalAction";
        public const string AbilityCoreCommandExecution = "AbilityCore.ExecuteCommand";
    }

    public static class LegalActionMaskUsageCatalog
    {
        public static LegalActionMaskUsageReport CreateCurrentReport()
        {
            var report = new LegalActionMaskUsageReport();

            Add(
                report,
                LegalActionMaskUsagePathIds.PlayTableDraw,
                "UI",
                true,
                true,
                false,
                true,
                string.Empty);
            Add(
                report,
                LegalActionMaskUsagePathIds.PlayTableMoveSelected,
                "UI",
                false,
                true,
                false,
                true,
                "Builds selected-card move intent, then RulesCore.TryExecute validates before mutation.");
            Add(
                report,
                LegalActionMaskUsagePathIds.PlayTableSetPhase,
                "UI",
                true,
                true,
                false,
                true,
                string.Empty);
            Add(
                report,
                LegalActionMaskUsagePathIds.PlayTableAddGiftMarker,
                "UI",
                true,
                true,
                false,
                true,
                string.Empty);
            Add(
                report,
                LegalActionMaskUsagePathIds.PlayTableUndo,
                "UI",
                false,
                false,
                true,
                false,
                "Undo uses GameActionService.UndoLast directly and remains the explicit manual/replay exception.");
            Add(
                report,
                LegalActionMaskUsagePathIds.EasyBotDecision,
                "Bot",
                true,
                true,
                false,
                true,
                string.Empty);
            Add(
                report,
                LegalActionMaskUsagePathIds.ProfileBotDecision,
                "Bot",
                true,
                true,
                false,
                true,
                string.Empty);
            Add(
                report,
                LegalActionMaskUsagePathIds.BotDecisionContext,
                "Bot",
                true,
                true,
                false,
                true,
                "Creates masked player view and legal actions before bot selection.");
            Add(
                report,
                LegalActionMaskUsagePathIds.HeuristicBotV2Decision,
                "Bot",
                true,
                true,
                false,
                true,
                "GameState overload enters BotDecisionContext before evaluating actions.");
            Add(
                report,
                LegalActionMaskUsagePathIds.PlaybookBotDecision,
                "Bot",
                true,
                true,
                false,
                true,
                "Playbook bot evaluates through BotDecisionContext and HeuristicBotV2.");
            Add(
                report,
                LegalActionMaskUsagePathIds.MultiplayerSessionLocalAction,
                "Multiplayer",
                false,
                true,
                false,
                true,
                "Receives an action from caller, then RulesCore.TryExecute validates before local publish.");
            Add(
                report,
                LegalActionMaskUsagePathIds.AbilityCoreCommandExecution,
                "AbilityCore",
                false,
                true,
                false,
                true,
                "Structured ability scaffold creates commands from legal action lookup and validates through RulesCore.TryExecute.");

            return report;
        }

        public static LegalActionMaskUsageEntry Find(LegalActionMaskUsageReport report, string pathId)
        {
            if (report == null || report.entries == null)
            {
                return null;
            }

            for (int i = 0; i < report.entries.Count; i++)
            {
                LegalActionMaskUsageEntry entry = report.entries[i];
                if (entry != null && string.Equals(entry.path_id, pathId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        public static int CountDirectMutationExceptions(LegalActionMaskUsageReport report)
        {
            if (report == null || report.entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < report.entries.Count; i++)
            {
                LegalActionMaskUsageEntry entry = report.entries[i];
                if (entry != null && entry.mutates_directly)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsHardened(LegalActionMaskUsageReport report, string pathId)
        {
            LegalActionMaskUsageEntry entry = Find(report, pathId);
            return entry != null && entry.hardened && !entry.mutates_directly;
        }

        private static void Add(
            LegalActionMaskUsageReport report,
            string pathId,
            string layer,
            bool usesLegalActionGenerator,
            bool validatesWithRulesCore,
            bool mutatesDirectly,
            bool hardened,
            string exceptionReason)
        {
            report.EnsureLists();
            report.entries.Add(new LegalActionMaskUsageEntry
            {
                path_id = pathId ?? string.Empty,
                layer = layer ?? string.Empty,
                uses_legal_action_generator = usesLegalActionGenerator,
                validates_with_rules_core = validatesWithRulesCore,
                mutates_directly = mutatesDirectly,
                hardened = hardened,
                exception_reason = exceptionReason ?? string.Empty
            });
        }
    }
}
