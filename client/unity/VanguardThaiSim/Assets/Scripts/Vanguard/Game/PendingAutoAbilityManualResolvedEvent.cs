using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityManualResolvedEventBuildRejectionReasons
    {
        public const string CommitResultMissing = "PENDING_AUTO_ABILITY_MANUAL_RESOLVED_COMMIT_RESULT_MISSING";
        public const string CommitResultRejected = "PENDING_AUTO_ABILITY_MANUAL_RESOLVED_COMMIT_RESULT_REJECTED";
        public const string ManualResolutionNotRecorded = "PENDING_AUTO_ABILITY_MANUAL_RESOLVED_NOT_RECORDED";
        public const string DecisionMissing = "PENDING_AUTO_ABILITY_MANUAL_RESOLVED_DECISION_MISSING";
        public const string DecisionTypeInvalid = "PENDING_AUTO_ABILITY_MANUAL_RESOLVED_DECISION_TYPE_INVALID";
        public const string PendingIdMismatch = "PENDING_AUTO_ABILITY_MANUAL_RESOLVED_PENDING_ID_MISMATCH";
    }

    [Serializable]
    public sealed class PendingAutoAbilityManualResolvedEvent
    {
        public const string EventType = "PendingAutoAbilityManualResolved";

        public string event_id;
        public string event_type = EventType;
        public string queue_id;
        public string pending_id;
        public string decision_id;
        public string decision_type;
        public int player_index;
        public string timing_event;
        public string before_queue_hash;
        public string after_queue_hash;
        public string manual_resolution_reason;
        public string manual_resolution_reason_hash;
        public bool hides_source_card_identity;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityManualResolvedEvent FromJson(string json)
        {
            PendingAutoAbilityManualResolvedEvent resolvedEvent =
                JsonUtility.FromJson<PendingAutoAbilityManualResolvedEvent>(json);
            if (resolvedEvent == null)
            {
                throw new ArgumentException(
                    "Pending auto ability manual resolved event JSON could not be parsed.",
                    "json");
            }

            if (string.IsNullOrWhiteSpace(resolvedEvent.event_type))
            {
                resolvedEvent.event_type = EventType;
            }

            return resolvedEvent;
        }
    }

    public sealed class PendingAutoAbilityManualResolvedEventBuildResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityManualResolvedEvent resolved_event;
    }

    public static class PendingAutoAbilityManualResolvedEventBuilder
    {
        public static PendingAutoAbilityManualResolvedEventBuildResult Build(
            PendingAutoAbilityQueueCommitResult commitResult,
            PendingAutoAbilityManualResolutionDecision decision)
        {
            if (commitResult == null)
            {
                return Reject(PendingAutoAbilityManualResolvedEventBuildRejectionReasons.CommitResultMissing);
            }

            if (!commitResult.accepted)
            {
                return Reject(PendingAutoAbilityManualResolvedEventBuildRejectionReasons.CommitResultRejected);
            }

            if (!commitResult.manual_resolution_recorded)
            {
                return Reject(PendingAutoAbilityManualResolvedEventBuildRejectionReasons.ManualResolutionNotRecorded);
            }

            if (decision == null)
            {
                return Reject(PendingAutoAbilityManualResolvedEventBuildRejectionReasons.DecisionMissing);
            }

            if (!string.Equals(
                    decision.decision_type,
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    StringComparison.Ordinal))
            {
                return Reject(PendingAutoAbilityManualResolvedEventBuildRejectionReasons.DecisionTypeInvalid);
            }

            if (!string.Equals(
                    commitResult.pending_id ?? string.Empty,
                    decision.pending_id ?? string.Empty,
                    StringComparison.Ordinal))
            {
                return Reject(PendingAutoAbilityManualResolvedEventBuildRejectionReasons.PendingIdMismatch);
            }

            bool hidesSource = decision.hides_source_card_identity ||
                string.Equals(decision.source_card_id, GameStateViewFactory.HiddenCardId, StringComparison.Ordinal);
            string safePendingId = BuildSafePendingId(commitResult.pending_id, decision, hidesSource);
            string safeReason = BuildSafeReason(decision.reason, hidesSource);
            string safeDecisionId = BuildSafeDecisionId(commitResult, decision, safePendingId, hidesSource);

            var resolvedEvent = new PendingAutoAbilityManualResolvedEvent
            {
                event_id = BuildEventId(commitResult, decision, safePendingId, safeDecisionId),
                event_type = PendingAutoAbilityManualResolvedEvent.EventType,
                queue_id = commitResult.queue_id ?? string.Empty,
                pending_id = safePendingId,
                decision_id = safeDecisionId,
                decision_type = PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                player_index = decision.player_index,
                timing_event = decision.timing_event ?? string.Empty,
                before_queue_hash = commitResult.before_queue_hash ?? string.Empty,
                after_queue_hash = commitResult.after_queue_hash ?? string.Empty,
                manual_resolution_reason = safeReason,
                manual_resolution_reason_hash = StableHash(decision.reason ?? string.Empty),
                hides_source_card_identity = hidesSource,
                summary = BuildSummary(decision, safePendingId)
            };

            return new PendingAutoAbilityManualResolvedEventBuildResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                resolved_event = resolvedEvent
            };
        }

        private static PendingAutoAbilityManualResolvedEventBuildResult Reject(string rejectionReason)
        {
            return new PendingAutoAbilityManualResolvedEventBuildResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                resolved_event = null
            };
        }

        private static string BuildEventId(
            PendingAutoAbilityQueueCommitResult commitResult,
            PendingAutoAbilityManualResolutionDecision decision,
            string safePendingId,
            string safeDecisionId)
        {
            return "pending-auto-manual-resolved-event|" +
                   (commitResult.queue_id ?? string.Empty) +
                   "|" + safePendingId +
                   "|" + safeDecisionId +
                   "|" + (decision.timing_event ?? string.Empty) +
                   "|" + (commitResult.before_queue_hash ?? string.Empty) +
                   "|" + (commitResult.after_queue_hash ?? string.Empty);
        }

        private static string BuildSafePendingId(
            string pendingId,
            PendingAutoAbilityManualResolutionDecision decision,
            bool hidesSource)
        {
            if (!hidesSource)
            {
                return pendingId ?? string.Empty;
            }

            return "pending-auto-hidden|" +
                   decision.player_index +
                   "|" + (decision.timing_event ?? string.Empty) +
                   "|" + StableHash(pendingId ?? string.Empty);
        }

        private static string BuildSafeDecisionId(
            PendingAutoAbilityQueueCommitResult commitResult,
            PendingAutoAbilityManualResolutionDecision decision,
            string safePendingId,
            bool hidesSource)
        {
            if (!hidesSource)
            {
                return commitResult.decision_id ?? string.Empty;
            }

            return "pending-auto-manual-decision|" +
                   PendingAutoAbilityManualResolutionDecisionTypes.Resolve +
                   "|" + decision.selected_index +
                   "|" + safePendingId +
                   "|" + StableHash(decision.reason ?? string.Empty);
        }

        private static string BuildSafeReason(string reason, bool hidesSource)
        {
            if (hidesSource)
            {
                return "<hidden>";
            }

            return SanitizeReason(reason);
        }

        private static string BuildSummary(
            PendingAutoAbilityManualResolutionDecision decision,
            string safePendingId)
        {
            return "Manual resolved pending AUTO " +
                   safePendingId +
                   " player=" +
                   decision.player_index +
                   " timing=" +
                   (decision.timing_event ?? string.Empty) +
                   ".";
        }

        private static string SanitizeReason(string reason)
        {
            string safeReason = reason ?? string.Empty;
            safeReason = safeReason.Replace('\r', ' ').Replace('\n', ' ').Trim();
            while (safeReason.Contains("  "))
            {
                safeReason = safeReason.Replace("  ", " ");
            }

            return safeReason;
        }

        private static string StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                string safeValue = value ?? string.Empty;
                for (int i = 0; i < safeValue.Length; i++)
                {
                    hash ^= safeValue[i];
                    hash *= 16777619;
                }

                return hash.ToString("x8");
            }
        }
    }
}
