using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class EventSourcingCoverageEntry
    {
        public string path_id;
        public string mutation_target;
        public bool mutates_game_state;
        public bool uses_rules_core;
        public bool creates_game_event;
        public bool appends_to_game_state_event_log;
        public bool replayable_with_game_event_reducer;
        public bool explicit_exception;
        public string exception_reason;
        public string next_action;
    }

    [Serializable]
    public sealed class EventSourcingCoverageReport
    {
        public List<EventSourcingCoverageEntry> entries = new List<EventSourcingCoverageEntry>();

        public void EnsureLists()
        {
            if (entries == null)
            {
                entries = new List<EventSourcingCoverageEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static EventSourcingCoverageReport FromJson(string json)
        {
            EventSourcingCoverageReport report =
                JsonUtility.FromJson<EventSourcingCoverageReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Event sourcing coverage report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class EventSourcingCoverageCatalog
    {
        public const string DrawPathId = "GameActionService.Draw";
        public const string MoveCardPathId = "GameActionService.MoveCard";
        public const string SetPhasePathId = "GameActionService.SetPhase";
        public const string AddGiftMarkerPathId = "GameActionService.AddGiftMarker";
        public const string ResourceFlipPathId = "GameActionService.ResourceFlip";
        public const string DeclareAttackPathId = "GameActionService.DeclareAttack";
        public const string GuardPathId = "GameActionService.Guard";
        public const string TriggerCheckPathId = "GameActionService.TriggerCheck";
        public const string MulliganCardsPathId = "GameActionService.MulliganCards";
        public const string AbilityCoreStructuredEffectsPathId = "AbilityCore.StructuredEffects";
        public const string UndoLastPathId = "GameActionService.UndoLast";
        public const string PendingAutoTimingCommitPathId =
            "AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent";

        public static EventSourcingCoverageReport CreateCurrentReport()
        {
            var report = new EventSourcingCoverageReport();

            AddCovered(
                report,
                DrawPathId,
                "player.deck/player.hand/GameState.event_log",
                true);
            AddCovered(
                report,
                MoveCardPathId,
                "player zone lists/GameState.event_log",
                true);
            AddCovered(
                report,
                SetPhasePathId,
                "GameState.phase/GameState.event_log",
                true);
            AddCovered(
                report,
                AddGiftMarkerPathId,
                "player.gift_markers/GameState.event_log",
                true);
            AddCovered(
                report,
                ResourceFlipPathId,
                "player.damage.face_up/GameState.event_log",
                true);
            AddCovered(
                report,
                DeclareAttackPathId,
                "attacker/target circles/GameState.event_log",
                true);
            AddCovered(
                report,
                GuardPathId,
                "guard zone list/GameState.event_log",
                true);
            AddCovered(
                report,
                TriggerCheckPathId,
                "trigger/damage zones/GameState.event_log",
                true);
            AddCovered(
                report,
                MulliganCardsPathId,
                "player.hand/player.deck/GameState.event_log",
                true);
            AddCovered(
                report,
                AbilityCoreStructuredEffectsPathId,
                "delegates structured effects through RulesCore/GameEvent",
                true);

            AddException(
                report,
                UndoLastPathId,
                "reverse previous GameEvent mutation and remove last event log entry",
                false,
                "Undo is a manual/replay utility exception until M11-07 decides explicit undo replay representation.",
                "Keep isolated from bot/network command paths.");

            AddException(
                report,
                PendingAutoTimingCommitPathId,
                "GameState.pending_auto_abilities",
                false,
                "Pending AUTO timing commit mutates committed queue state but currently records metadata in specialized pending AUTO event models, not GameEventReducer.",
                "M11/M12 follow-up must decide whether pending AUTO queue commits become GameEvent variants or a parallel replay stream.");

            return report;
        }

        public static EventSourcingCoverageEntry Find(
            EventSourcingCoverageReport report,
            string pathId)
        {
            if (report == null || report.entries == null)
            {
                return null;
            }

            for (int i = 0; i < report.entries.Count; i++)
            {
                EventSourcingCoverageEntry entry = report.entries[i];
                if (entry != null && string.Equals(entry.path_id, pathId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        public static int CountExceptions(EventSourcingCoverageReport report)
        {
            if (report == null || report.entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < report.entries.Count; i++)
            {
                EventSourcingCoverageEntry entry = report.entries[i];
                if (entry != null && entry.explicit_exception)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsGameEventSourced(EventSourcingCoverageReport report, string pathId)
        {
            EventSourcingCoverageEntry entry = Find(report, pathId);
            return entry != null &&
                   entry.mutates_game_state &&
                   entry.creates_game_event &&
                   entry.appends_to_game_state_event_log &&
                   entry.replayable_with_game_event_reducer &&
                   !entry.explicit_exception;
        }

        private static void AddCovered(
            EventSourcingCoverageReport report,
            string pathId,
            string mutationTarget,
            bool usesRulesCore)
        {
            Add(
                report,
                pathId,
                mutationTarget,
                true,
                usesRulesCore,
                true,
                true,
                true,
                false,
                string.Empty,
                string.Empty);
        }

        private static void AddException(
            EventSourcingCoverageReport report,
            string pathId,
            string mutationTarget,
            bool usesRulesCore,
            string exceptionReason,
            string nextAction)
        {
            Add(
                report,
                pathId,
                mutationTarget,
                true,
                usesRulesCore,
                false,
                false,
                false,
                true,
                exceptionReason,
                nextAction);
        }

        private static void Add(
            EventSourcingCoverageReport report,
            string pathId,
            string mutationTarget,
            bool mutatesGameState,
            bool usesRulesCore,
            bool createsGameEvent,
            bool appendsToEventLog,
            bool replayableWithReducer,
            bool explicitException,
            string exceptionReason,
            string nextAction)
        {
            report.EnsureLists();
            report.entries.Add(new EventSourcingCoverageEntry
            {
                path_id = pathId ?? string.Empty,
                mutation_target = mutationTarget ?? string.Empty,
                mutates_game_state = mutatesGameState,
                uses_rules_core = usesRulesCore,
                creates_game_event = createsGameEvent,
                appends_to_game_state_event_log = appendsToEventLog,
                replayable_with_game_event_reducer = replayableWithReducer,
                explicit_exception = explicitException,
                exception_reason = exceptionReason ?? string.Empty,
                next_action = nextAction ?? string.Empty
            });
        }
    }
}
