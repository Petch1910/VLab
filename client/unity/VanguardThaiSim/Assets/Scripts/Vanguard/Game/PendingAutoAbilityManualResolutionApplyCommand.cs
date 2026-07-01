using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityManualResolutionApplyRejectionReasons
    {
        public const string QueueMissing = "PENDING_AUTO_ABILITY_QUEUE_MISSING";
        public const string DecisionMissing = "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING";
        public const string PendingIdMismatch = "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_PENDING_ID_MISMATCH";
        public const string DecisionTypeInvalid = "PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID";
    }

    [Serializable]
    public sealed class PendingAutoAbilityManualResolutionApplyCommand
    {
        public string command_id;
        public string queue_id;
        public string pending_id;
        public string decision_id;
        public string decision_type;
        public int player_index;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityManualResolutionApplyCommand FromJson(string json)
        {
            PendingAutoAbilityManualResolutionApplyCommand command =
                JsonUtility.FromJson<PendingAutoAbilityManualResolutionApplyCommand>(json);
            if (command == null)
            {
                throw new ArgumentException(
                    "Pending auto ability manual resolution apply command JSON could not be parsed.",
                    "json");
            }

            return command;
        }
    }

    [Serializable]
    public sealed class PendingAutoAbilityManualResolutionApplyResult
    {
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

        public static PendingAutoAbilityManualResolutionApplyResult FromJson(string json)
        {
            PendingAutoAbilityManualResolutionApplyResult result =
                JsonUtility.FromJson<PendingAutoAbilityManualResolutionApplyResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Pending auto ability manual resolution apply result JSON could not be parsed.",
                    "json");
            }

            return result;
        }

        public static PendingAutoAbilityManualResolutionApplyResult Accepted(
            string queueId,
            string pendingId,
            string decisionType,
            string summary)
        {
            return new PendingAutoAbilityManualResolutionApplyResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                queue_id = queueId ?? string.Empty,
                pending_id = pendingId ?? string.Empty,
                decision_type = decisionType ?? string.Empty,
                summary = summary ?? string.Empty
            };
        }

        public static PendingAutoAbilityManualResolutionApplyResult Rejected(string rejectionReason)
        {
            return new PendingAutoAbilityManualResolutionApplyResult
            {
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
