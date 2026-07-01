using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class TimingWindowAuditEntry
    {
        public string category;
        public string identifier;
        public string source;
        public string notes;
        public bool gap;
    }

    [Serializable]
    public sealed class TimingWindowAuditReport
    {
        public List<TimingWindowAuditEntry> entries = new List<TimingWindowAuditEntry>();

        public void EnsureLists()
        {
            if (entries == null)
            {
                entries = new List<TimingWindowAuditEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TimingWindowAuditReport FromJson(string json)
        {
            TimingWindowAuditReport report = JsonUtility.FromJson<TimingWindowAuditReport>(json);
            if (report == null)
            {
                throw new ArgumentException("Timing window audit report JSON could not be parsed.", "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class TimingWindowAuditCategories
    {
        public const string GamePhase = "GamePhase";
        public const string GameActionType = "GameActionType";
        public const string AbilityTiming = "AbilityTiming";
        public const string PendingAutoTiming = "PendingAutoTiming";
        public const string TriggerCheckSource = "TriggerCheckSource";
        public const string CombatModifierExpiration = "CombatModifierExpiration";
        public const string Gap = "Gap";
    }

    public static class TimingWindowAuditCatalog
    {
        public static TimingWindowAuditReport CreateCurrentReport()
        {
            var report = new TimingWindowAuditReport();

            AddEnum<GamePhase>(
                report,
                TimingWindowAuditCategories.GamePhase,
                "GamePhase.cs",
                "Current coarse phase enum.");
            AddEnum<GameActionType>(
                report,
                TimingWindowAuditCategories.GameActionType,
                "GameActionType.cs",
                "Current command event action enum.");
            AddEnum<AbilityTiming>(
                report,
                TimingWindowAuditCategories.AbilityTiming,
                "AbilityTiming.cs",
                "Early ability timing enum; not yet the authoritative rules window model.");
            AddPendingAutoTiming(report, GameActionType.Draw);
            AddPendingAutoTiming(report, GameActionType.MoveCard);
            AddPendingAutoTiming(report, GameActionType.SetPhase);
            AddPendingAutoTiming(report, GameActionType.AddGiftMarker);
            AddPendingAutoTiming(report, GameActionType.ResourceFlip);
            AddEnum<TriggerCheckSource>(
                report,
                TimingWindowAuditCategories.TriggerCheckSource,
                "TriggerCheckResolutionBundle.cs",
                "Manual draft/check source context, not a full timing window.");
            AddEnum<CombatModifierExpiration>(
                report,
                TimingWindowAuditCategories.CombatModifierExpiration,
                "CombatModifierLedger.cs",
                "Modifier duration/cleanup checkpoint enum.");

            AddGap(
                report,
                "TypedTimingWindowEnumMissing",
                "No authoritative TimingWindow enum exists yet; pending AUTO timing currently uses strings.");
            AddGap(
                report,
                "PhaseTimingMatrixMissing",
                "No matrix maps commands/resolver windows to phases yet.");
            AddGap(
                report,
                "BattleStepWindowsMissing",
                "Attack, guard, drive, damage, battle resolution, and close-step windows are not modeled yet.");
            AddGap(
                report,
                "CleanupNotPhaseIntegrated",
                "EndOfBattle and EndOfTurn cleanup previews are not wired to a phase/window controller.");
            AddGap(
                report,
                "TriggerCheckSourceNotWindow",
                "Drive/Damage/Manual trigger check sources are context labels, not phase-legal timing windows.");

            return report;
        }

        public static int CountCategory(TimingWindowAuditReport report, string category)
        {
            if (report == null || report.entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < report.entries.Count; i++)
            {
                TimingWindowAuditEntry entry = report.entries[i];
                if (entry != null && string.Equals(entry.category, category, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool Contains(TimingWindowAuditReport report, string category, string identifier)
        {
            if (report == null || report.entries == null)
            {
                return false;
            }

            for (int i = 0; i < report.entries.Count; i++)
            {
                TimingWindowAuditEntry entry = report.entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (string.Equals(entry.category, category, StringComparison.Ordinal) &&
                    string.Equals(entry.identifier, identifier, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddEnum<TEnum>(
            TimingWindowAuditReport report,
            string category,
            string source,
            string notes)
        {
            string[] names = Enum.GetNames(typeof(TEnum));
            for (int i = 0; i < names.Length; i++)
            {
                Add(report, category, names[i], source, notes, false);
            }
        }

        private static void AddPendingAutoTiming(TimingWindowAuditReport report, GameActionType actionType)
        {
            string timing = AbilityTriggerEventCollector.GetTimingEvent(new GameEvent { action_type = actionType });
            Add(
                report,
                TimingWindowAuditCategories.PendingAutoTiming,
                timing,
                "AbilityTriggerEventCollector.cs",
                "String timing key derived from " + actionType + ".",
                false);
        }

        private static void AddGap(TimingWindowAuditReport report, string identifier, string notes)
        {
            Add(report, TimingWindowAuditCategories.Gap, identifier, "M11-01 audit", notes, true);
        }

        private static void Add(
            TimingWindowAuditReport report,
            string category,
            string identifier,
            string source,
            string notes,
            bool gap)
        {
            report.EnsureLists();
            report.entries.Add(new TimingWindowAuditEntry
            {
                category = category ?? string.Empty,
                identifier = identifier ?? string.Empty,
                source = source ?? string.Empty,
                notes = notes ?? string.Empty,
                gap = gap
            });
        }
    }
}
