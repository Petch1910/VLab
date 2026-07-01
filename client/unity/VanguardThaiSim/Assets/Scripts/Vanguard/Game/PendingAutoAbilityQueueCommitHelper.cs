using System;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityQueueCommitRejectionReasons
    {
        public const string QueueMissing = "PENDING_AUTO_ABILITY_COMMIT_QUEUE_MISSING";
        public const string DecisionMissing = "PENDING_AUTO_ABILITY_COMMIT_DECISION_MISSING";
        public const string DecisionTypeInvalid = "PENDING_AUTO_ABILITY_COMMIT_DECISION_TYPE_INVALID";
        public const string PendingIdMismatch = "PENDING_AUTO_ABILITY_COMMIT_PENDING_ID_MISMATCH";
    }

    public sealed class PendingAutoAbilityQueueCommitResult
    {
        public bool accepted;
        public string rejection_reason;
        public string queue_id;
        public string pending_id;
        public string decision_id;
        public string decision_type;
        public int player_index;
        public string before_queue_hash;
        public string after_queue_hash;
        public bool manual_resolution_recorded;
        public PendingAutoAbilityQueue queue;
    }

    public static class PendingAutoAbilityQueueCommitHelper
    {
        public static PendingAutoAbilityQueueCommitResult Commit(
            PendingAutoAbilityQueue queue,
            PendingAutoAbilityManualResolutionDecision decision)
        {
            PendingAutoAbility active = GetActivePending(queue);
            if (active == null)
            {
                return Reject(PendingAutoAbilityQueueCommitRejectionReasons.QueueMissing, queue, decision);
            }

            if (decision == null)
            {
                return Reject(PendingAutoAbilityQueueCommitRejectionReasons.DecisionMissing, queue, null);
            }

            if (!PendingAutoAbilityManualResolutionDecisionTypes.IsSupported(decision.decision_type))
            {
                return Reject(PendingAutoAbilityQueueCommitRejectionReasons.DecisionTypeInvalid, queue, decision);
            }

            if (!string.Equals(active.pending_id ?? string.Empty, decision.pending_id ?? string.Empty, StringComparison.Ordinal))
            {
                return Reject(PendingAutoAbilityQueueCommitRejectionReasons.PendingIdMismatch, queue, decision);
            }

            string beforeHash = StableHash(CloneQueue(queue).ToJson(false));
            PendingAutoAbilityQueue nextQueue = CloneQueue(queue);
            nextQueue.EnsureLists();
            bool manualResolutionRecorded = false;

            if (string.Equals(decision.decision_type, PendingAutoAbilityManualResolutionDecisionTypes.Defer, StringComparison.Ordinal))
            {
                PendingAutoAbility deferred = nextQueue.pending[0];
                nextQueue.pending.RemoveAt(0);
                nextQueue.pending.Add(deferred);
            }
            else
            {
                manualResolutionRecorded = string.Equals(
                    decision.decision_type,
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    StringComparison.Ordinal);
                nextQueue.pending.RemoveAt(0);
            }

            return new PendingAutoAbilityQueueCommitResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                queue_id = nextQueue.queue_id ?? string.Empty,
                pending_id = active.pending_id ?? string.Empty,
                decision_id = BuildSafeDecisionId(decision),
                decision_type = decision.decision_type ?? string.Empty,
                player_index = decision.player_index,
                before_queue_hash = beforeHash,
                after_queue_hash = StableHash(nextQueue.ToJson(false)),
                manual_resolution_recorded = manualResolutionRecorded,
                queue = nextQueue
            };
        }

        private static PendingAutoAbilityQueueCommitResult Reject(
            string rejectionReason,
            PendingAutoAbilityQueue queue,
            PendingAutoAbilityManualResolutionDecision decision)
        {
            PendingAutoAbilityQueue queueClone = queue == null ? null : CloneQueue(queue);
            string queueId = queueClone == null ? string.Empty : queueClone.queue_id ?? string.Empty;

            return new PendingAutoAbilityQueueCommitResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                queue_id = queueId,
                pending_id = decision == null ? string.Empty : decision.pending_id ?? string.Empty,
                decision_id = decision == null ? string.Empty : BuildSafeDecisionId(decision),
                decision_type = decision == null ? string.Empty : decision.decision_type ?? string.Empty,
                player_index = decision == null ? -1 : decision.player_index,
                before_queue_hash = queueClone == null ? string.Empty : StableHash(queueClone.ToJson(false)),
                after_queue_hash = queueClone == null ? string.Empty : StableHash(queueClone.ToJson(false)),
                manual_resolution_recorded = false,
                queue = queueClone
            };
        }

        private static PendingAutoAbility GetActivePending(PendingAutoAbilityQueue queue)
        {
            if (queue == null)
            {
                return null;
            }

            if (queue.pending == null || queue.pending.Count == 0)
            {
                return null;
            }

            return queue.pending[0];
        }

        private static PendingAutoAbilityQueue CloneQueue(PendingAutoAbilityQueue source)
        {
            var clone = new PendingAutoAbilityQueue
            {
                queue_id = source.queue_id ?? "pending-auto-ability-queue"
            };

            if (source.pending == null)
            {
                return clone;
            }

            for (int i = 0; i < source.pending.Count; i++)
            {
                PendingAutoAbility pending = source.pending[i];
                if (pending != null)
                {
                    clone.pending.Add(PendingAutoAbility.FromJson(pending.ToJson(false)));
                }
            }

            return clone;
        }

        private static string BuildSafeDecisionId(PendingAutoAbilityManualResolutionDecision decision)
        {
            return "pending-auto-manual-decision|" +
                   (decision.decision_type ?? string.Empty) +
                   "|" + decision.selected_index +
                   "|" + (decision.pending_id ?? string.Empty) +
                   "|" + StableHash(decision.reason ?? string.Empty);
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
