using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class PendingAutoAbilityManualResolutionApplyPreviewLogEntry
    {
        public string log_entry_id;
        public bool accepted;
        public string rejection_reason;
        public string queue_id;
        public string pending_id;
        public string decision_type;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityManualResolutionApplyPreviewLogEntry FromJson(string json)
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                JsonUtility.FromJson<PendingAutoAbilityManualResolutionApplyPreviewLogEntry>(json);
            if (entry == null)
            {
                throw new ArgumentException(
                    "Pending auto ability manual resolution apply preview log entry JSON could not be parsed.",
                    "json");
            }

            return entry;
        }

        public static PendingAutoAbilityManualResolutionApplyPreviewLogEntry FromApplyResult(
            PendingAutoAbilityManualResolutionApplyResult result,
            string logEntryId)
        {
            if (result == null)
            {
                return Rejected(logEntryId, string.Empty);
            }

            if (!result.accepted)
            {
                return Rejected(logEntryId, result.rejection_reason);
            }

            return Accepted(
                logEntryId,
                result.queue_id,
                result.pending_id,
                result.decision_type,
                result.summary);
        }

        public static PendingAutoAbilityManualResolutionApplyPreviewLogEntry Accepted(
            string logEntryId,
            string queueId,
            string pendingId,
            string decisionType,
            string summary)
        {
            return new PendingAutoAbilityManualResolutionApplyPreviewLogEntry
            {
                log_entry_id = logEntryId ?? string.Empty,
                accepted = true,
                rejection_reason = string.Empty,
                queue_id = queueId ?? string.Empty,
                pending_id = pendingId ?? string.Empty,
                decision_type = decisionType ?? string.Empty,
                summary = summary ?? string.Empty
            };
        }

        public static PendingAutoAbilityManualResolutionApplyPreviewLogEntry Rejected(
            string logEntryId,
            string rejectionReason)
        {
            return new PendingAutoAbilityManualResolutionApplyPreviewLogEntry
            {
                log_entry_id = logEntryId ?? string.Empty,
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                queue_id = string.Empty,
                pending_id = string.Empty,
                decision_type = string.Empty,
                summary = string.Empty
            };
        }
    }
}
