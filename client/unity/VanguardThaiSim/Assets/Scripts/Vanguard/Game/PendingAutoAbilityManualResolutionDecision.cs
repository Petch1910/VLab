using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityManualResolutionDecisionTypes
    {
        public const string Resolve = "Resolve";
        public const string Skip = "Skip";
        public const string Defer = "Defer";

        public static bool IsSupported(string decisionType)
        {
            return string.Equals(decisionType, Resolve, StringComparison.Ordinal) ||
                   string.Equals(decisionType, Skip, StringComparison.Ordinal) ||
                   string.Equals(decisionType, Defer, StringComparison.Ordinal);
        }
    }

    [Serializable]
    public sealed class PendingAutoAbilityManualResolutionDecision
    {
        public string decision_id;
        public string decision_type;
        public int selected_index = -1;
        public string pending_id;
        public int player_index;
        public string timing_event;
        public string source_card_instance_id;
        public string source_card_id;
        public bool hides_source_card_identity;
        public string reason;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityManualResolutionDecision FromJson(string json)
        {
            PendingAutoAbilityManualResolutionDecision decision =
                JsonUtility.FromJson<PendingAutoAbilityManualResolutionDecision>(json);
            if (decision == null)
            {
                throw new ArgumentException(
                    "Pending auto ability manual resolution decision JSON could not be parsed.",
                    "json");
            }

            return decision;
        }
    }

    public sealed class PendingAutoAbilityManualResolutionDecisionResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityManualResolutionDecision decision;
    }

    public static class PendingAutoAbilityManualResolutionDecisionFactory
    {
        public const string RequestMissingReason = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING";
        public const string DecisionTypeInvalidReason = "PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID";
        private const int MaxReasonLength = 240;

        public static PendingAutoAbilityManualResolutionDecisionResult Create(
            PendingAutoAbilityResolutionRequest request,
            string decisionType,
            string reason = "")
        {
            if (request == null)
            {
                return Reject(RequestMissingReason);
            }

            if (!PendingAutoAbilityManualResolutionDecisionTypes.IsSupported(decisionType))
            {
                return Reject(DecisionTypeInvalidReason);
            }

            bool hidesSource =
                request.hides_source_card_identity ||
                string.Equals(request.source_card_id, GameStateViewFactory.HiddenCardId, StringComparison.Ordinal);

            string safePendingId = request.pending_id ?? string.Empty;
            string safeDecisionType = decisionType ?? string.Empty;
            string safeReason = SanitizeReason(reason);

            var decision = new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = BuildDecisionId(
                    safeDecisionType,
                    request.selected_index,
                    safePendingId,
                    safeReason),
                decision_type = safeDecisionType,
                selected_index = request.selected_index,
                pending_id = safePendingId,
                player_index = request.player_index,
                timing_event = request.timing_event ?? string.Empty,
                source_card_instance_id = hidesSource ? string.Empty : request.source_card_instance_id ?? string.Empty,
                source_card_id = hidesSource
                    ? GameStateViewFactory.HiddenCardId
                    : request.source_card_id ?? string.Empty,
                hides_source_card_identity = hidesSource,
                reason = safeReason,
                summary = request.summary ?? string.Empty
            };

            return new PendingAutoAbilityManualResolutionDecisionResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                decision = decision
            };
        }

        private static PendingAutoAbilityManualResolutionDecisionResult Reject(string rejectionReason)
        {
            return new PendingAutoAbilityManualResolutionDecisionResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                decision = null
            };
        }

        private static string BuildDecisionId(
            string decisionType,
            int selectedIndex,
            string pendingId,
            string reason)
        {
            return "pending-auto-manual-decision|" +
                   decisionType +
                   "|" + selectedIndex +
                   "|" + pendingId +
                   "|" + StableHash(reason);
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

        private static string SanitizeReason(string reason)
        {
            string safeReason = reason ?? string.Empty;
            safeReason = safeReason.Replace('\r', ' ').Replace('\n', ' ').Trim();
            while (safeReason.Contains("  "))
            {
                safeReason = safeReason.Replace("  ", " ");
            }

            if (safeReason.Length > MaxReasonLength)
            {
                return safeReason.Substring(0, MaxReasonLength);
            }

            return safeReason;
        }
    }
}
