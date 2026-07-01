using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityQueueCommitEventBuildRejectionReasons
    {
        public const string CommitResultMissing = "PENDING_AUTO_ABILITY_QUEUE_COMMIT_RESULT_MISSING";
        public const string CommitResultRejected = "PENDING_AUTO_ABILITY_QUEUE_COMMIT_RESULT_REJECTED";
    }

    [Serializable]
    public sealed class PendingAutoAbilityQueueCommitEvent
    {
        public const string EventType = "PendingAutoAbilityQueueCommitted";

        public string event_id;
        public string event_type = EventType;
        public string queue_id;
        public string pending_id;
        public string decision_id;
        public string decision_type;
        public int player_index;
        public string before_queue_hash;
        public string after_queue_hash;
        public bool manual_resolution_recorded;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityQueueCommitEvent FromJson(string json)
        {
            PendingAutoAbilityQueueCommitEvent commitEvent =
                JsonUtility.FromJson<PendingAutoAbilityQueueCommitEvent>(json);
            if (commitEvent == null)
            {
                throw new ArgumentException(
                    "Pending auto ability queue commit event JSON could not be parsed.",
                    "json");
            }

            if (string.IsNullOrWhiteSpace(commitEvent.event_type))
            {
                commitEvent.event_type = EventType;
            }

            return commitEvent;
        }
    }

    public sealed class PendingAutoAbilityQueueCommitEventBuildResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityQueueCommitEvent commit_event;
    }

    public static class PendingAutoAbilityQueueCommitEventBuilder
    {
        public static PendingAutoAbilityQueueCommitEventBuildResult Build(
            PendingAutoAbilityQueueCommitResult commitResult)
        {
            if (commitResult == null)
            {
                return Reject(PendingAutoAbilityQueueCommitEventBuildRejectionReasons.CommitResultMissing);
            }

            if (!commitResult.accepted)
            {
                return Reject(PendingAutoAbilityQueueCommitEventBuildRejectionReasons.CommitResultRejected);
            }

            var commitEvent = new PendingAutoAbilityQueueCommitEvent
            {
                event_id = BuildEventId(commitResult),
                event_type = PendingAutoAbilityQueueCommitEvent.EventType,
                queue_id = commitResult.queue_id ?? string.Empty,
                pending_id = commitResult.pending_id ?? string.Empty,
                decision_id = commitResult.decision_id ?? string.Empty,
                decision_type = commitResult.decision_type ?? string.Empty,
                player_index = commitResult.player_index,
                before_queue_hash = commitResult.before_queue_hash ?? string.Empty,
                after_queue_hash = commitResult.after_queue_hash ?? string.Empty,
                manual_resolution_recorded = commitResult.manual_resolution_recorded,
                summary = BuildSummary(commitResult)
            };

            return new PendingAutoAbilityQueueCommitEventBuildResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                commit_event = commitEvent
            };
        }

        private static PendingAutoAbilityQueueCommitEventBuildResult Reject(string rejectionReason)
        {
            return new PendingAutoAbilityQueueCommitEventBuildResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                commit_event = null
            };
        }

        private static string BuildEventId(PendingAutoAbilityQueueCommitResult commitResult)
        {
            return "pending-auto-queue-commit-event|" +
                   (commitResult.queue_id ?? string.Empty) +
                   "|" + (commitResult.pending_id ?? string.Empty) +
                   "|" + (commitResult.decision_id ?? string.Empty) +
                   "|" + (commitResult.decision_type ?? string.Empty) +
                   "|" + (commitResult.before_queue_hash ?? string.Empty) +
                   "|" + (commitResult.after_queue_hash ?? string.Empty);
        }

        private static string BuildSummary(PendingAutoAbilityQueueCommitResult commitResult)
        {
            return "Committed pending AUTO " +
                   (commitResult.decision_type ?? string.Empty) +
                   " for " +
                   (commitResult.pending_id ?? string.Empty) +
                   ".";
        }
    }
}
