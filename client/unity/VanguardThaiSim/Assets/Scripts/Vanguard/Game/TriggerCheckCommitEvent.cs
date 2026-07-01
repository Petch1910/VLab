using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class TriggerCheckCommitEventBuildRejectionReasons
    {
        public const string LogEntryMissing = "TRIGGER_CHECK_LOG_ENTRY_MISSING";
        public const string PlayerIndexInvalid = "TRIGGER_CHECK_PLAYER_INDEX_INVALID";
        public const string CheckedCardMissing = "TRIGGER_CHECK_CHECKED_CARD_MISSING";
    }

    [Serializable]
    public sealed class TriggerCheckCommitEvent
    {
        public const string EventType = "TriggerCheckCommitted";

        public string event_id;
        public string event_type = EventType;
        public string source_log_entry_id;
        public TriggerCheckSource check_source;
        public int player_index;
        public int check_index;
        public string checked_card_instance_id;
        public string checked_card_id;
        public TriggerType trigger_type;
        public bool hides_checked_card_identity;
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public int modifier_count;
        public List<string> modifier_ids = new List<string>();
        public string rng_outcome_id;
        public string summary;

        public void EnsureLists()
        {
            if (modifier_ids == null) modifier_ids = new List<string>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TriggerCheckCommitEvent FromJson(string json)
        {
            TriggerCheckCommitEvent commitEvent = JsonUtility.FromJson<TriggerCheckCommitEvent>(json);
            if (commitEvent == null)
            {
                throw new ArgumentException("Trigger check commit event JSON could not be parsed.", "json");
            }

            if (string.IsNullOrWhiteSpace(commitEvent.event_type))
            {
                commitEvent.event_type = EventType;
            }

            commitEvent.EnsureLists();
            return commitEvent;
        }
    }

    public sealed class TriggerCheckCommitEventBuildResult
    {
        public bool accepted;
        public string rejection_reason;
        public TriggerCheckCommitEvent commit_event;
    }

    public static class TriggerCheckCommitEventBuilder
    {
        public static TriggerCheckCommitEventBuildResult Build(TriggerCheckLogEntry entry)
        {
            if (entry == null)
            {
                return Reject(TriggerCheckCommitEventBuildRejectionReasons.LogEntryMissing);
            }

            if (entry.player_index < 0)
            {
                return Reject(TriggerCheckCommitEventBuildRejectionReasons.PlayerIndexInvalid);
            }

            if (string.IsNullOrWhiteSpace(entry.checked_card_id))
            {
                return Reject(TriggerCheckCommitEventBuildRejectionReasons.CheckedCardMissing);
            }

            var commitEvent = new TriggerCheckCommitEvent
            {
                event_id = BuildEventId(entry),
                event_type = TriggerCheckCommitEvent.EventType,
                source_log_entry_id = entry.log_entry_id ?? string.Empty,
                check_source = entry.check_source,
                player_index = entry.player_index,
                check_index = Math.Max(0, entry.check_index),
                checked_card_instance_id = entry.checked_card_instance_id ?? string.Empty,
                checked_card_id = entry.checked_card_id ?? string.Empty,
                trigger_type = entry.trigger_type,
                hides_checked_card_identity = entry.hides_checked_card_identity,
                accepted = entry.accepted,
                needs_manual_resolution = entry.needs_manual_resolution,
                rejection_reason = entry.rejection_reason ?? string.Empty,
                modifier_count = Math.Max(0, entry.modifier_count),
                rng_outcome_id = BuildOutcomeId(entry),
                summary = entry.summary ?? string.Empty
            };
            CopyModifierIds(entry.modifier_ids, commitEvent.modifier_ids);
            commitEvent.modifier_count = commitEvent.modifier_ids.Count;

            return new TriggerCheckCommitEventBuildResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                commit_event = commitEvent
            };
        }

        private static TriggerCheckCommitEventBuildResult Reject(string rejectionReason)
        {
            return new TriggerCheckCommitEventBuildResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                commit_event = null
            };
        }

        private static string BuildEventId(TriggerCheckLogEntry entry)
        {
            return "trigger-check-commit-event|" +
                   (entry.log_entry_id ?? string.Empty) +
                   "|" + BuildOutcomeId(entry);
        }

        private static string BuildOutcomeId(TriggerCheckLogEntry entry)
        {
            return "trigger-outcome|" +
                   entry.check_source +
                   "|" + entry.player_index +
                   "|" + Math.Max(0, entry.check_index) +
                   "|" + (entry.checked_card_instance_id ?? string.Empty) +
                   "|" + (entry.checked_card_id ?? string.Empty) +
                   "|" + entry.trigger_type;
        }

        private static void CopyModifierIds(IReadOnlyList<string> source, List<string> destination)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrEmpty(source[i]))
                {
                    destination.Add(source[i]);
                }
            }
        }
    }
}
