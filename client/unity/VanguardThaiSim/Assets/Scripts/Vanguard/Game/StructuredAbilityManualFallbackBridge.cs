using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredAbilityManualFallbackBridgeRejectionReasons
    {
        public const string AbilityMissing = "STRUCTURED_ABILITY_MANUAL_FALLBACK_ABILITY_MISSING";
        public const string ManualFallbackDisabled = "STRUCTURED_ABILITY_MANUAL_FALLBACK_DISABLED";
        public const string RejectionReasonMissing = "STRUCTURED_ABILITY_MANUAL_FALLBACK_REASON_MISSING";
        public const string RequestRejected = "STRUCTURED_ABILITY_MANUAL_FALLBACK_REQUEST_REJECTED";
        public const string DecisionRejected = "STRUCTURED_ABILITY_MANUAL_FALLBACK_DECISION_REJECTED";
    }

    [Serializable]
    public sealed class StructuredAbilityManualFallbackBridgeResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbility pending_ability;
        public PendingAutoAbilityResolutionRequest request;
        public PendingAutoAbilityManualResolutionDecision decision;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredAbilityManualFallbackBridgeResult FromJson(string json)
        {
            StructuredAbilityManualFallbackBridgeResult result =
                JsonUtility.FromJson<StructuredAbilityManualFallbackBridgeResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Structured ability manual fallback bridge result JSON could not be parsed.",
                    "json");
            }

            return result;
        }
    }

    public static class StructuredAbilityManualFallbackBridge
    {
        public static StructuredAbilityManualFallbackBridgeResult CreateResolveDecision(
            StructuredAbility ability,
            string rejectionReason,
            int playerIndex,
            string timingEvent,
            bool hideSourceCardIdentity)
        {
            if (ability == null)
            {
                return Reject(StructuredAbilityManualFallbackBridgeRejectionReasons.AbilityMissing);
            }

            if (!ability.manual_fallback)
            {
                return Reject(StructuredAbilityManualFallbackBridgeRejectionReasons.ManualFallbackDisabled);
            }

            string safeReason = SanitizeReason(rejectionReason);
            if (string.IsNullOrEmpty(safeReason))
            {
                return Reject(StructuredAbilityManualFallbackBridgeRejectionReasons.RejectionReasonMissing);
            }

            PendingAutoAbility pending = BuildPendingAbility(
                ability,
                safeReason,
                playerIndex,
                timingEvent,
                hideSourceCardIdentity);
            var selection = new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = 0,
                selected_ability = pending
            };

            PendingAutoAbilityResolutionRequestResult requestResult =
                PendingAutoAbilityResolutionRequestFactory.Create(selection);
            if (!requestResult.accepted)
            {
                return Reject(
                    StructuredAbilityManualFallbackBridgeRejectionReasons.RequestRejected + ": " +
                    requestResult.rejection_reason);
            }

            PendingAutoAbilityManualResolutionDecisionResult decisionResult =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    requestResult.request,
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    safeReason);
            if (!decisionResult.accepted)
            {
                return Reject(
                    StructuredAbilityManualFallbackBridgeRejectionReasons.DecisionRejected + ": " +
                    decisionResult.rejection_reason);
            }

            return new StructuredAbilityManualFallbackBridgeResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                pending_ability = PendingAutoAbility.FromJson(pending.ToJson(false)),
                request = PendingAutoAbilityResolutionRequest.FromJson(requestResult.request.ToJson(false)),
                decision = PendingAutoAbilityManualResolutionDecision.FromJson(decisionResult.decision.ToJson(false)),
                summary = "Manual fallback bridge created Resolve decision for " + (ability.ability_id ?? string.Empty) + "."
            };
        }

        public static StructuredAbilityManualFallbackBridgeResult CreateResolveDecision(
            StructuredAbility ability,
            StructuredAbilityFixtureResult fixtureResult,
            int playerIndex,
            string timingEvent,
            bool hideSourceCardIdentity)
        {
            if (fixtureResult == null)
            {
                return CreateResolveDecision(
                    ability,
                    StructuredAbilityFixtureRejectionReasons.EffectRejected,
                    playerIndex,
                    timingEvent,
                    hideSourceCardIdentity);
            }

            return CreateResolveDecision(
                ability,
                fixtureResult.rejection_reason,
                playerIndex,
                timingEvent,
                hideSourceCardIdentity);
        }

        private static PendingAutoAbility BuildPendingAbility(
            StructuredAbility ability,
            string reason,
            int playerIndex,
            string timingEvent,
            bool hideSourceCardIdentity)
        {
            string safeTiming = string.IsNullOrEmpty(timingEvent)
                ? ability.timing == null ? string.Empty : ability.timing.window ?? string.Empty
                : timingEvent;
            string pendingId = "structured-manual-fallback|" +
                               (ability.ability_id ?? string.Empty) +
                               "|" + StableHash(reason);

            return new PendingAutoAbility
            {
                pending_id = pendingId,
                source_card_instance_id = hideSourceCardIdentity ? string.Empty : "structured-ability|" + (ability.ability_id ?? string.Empty),
                source_card_id = hideSourceCardIdentity ? GameStateViewFactory.HiddenCardId : ability.card_id ?? string.Empty,
                player_index = playerIndex,
                timing_event = safeTiming,
                summary = "Manual fallback for " + (ability.ability_id ?? string.Empty) + ": " + reason,
                hides_source_card_identity = hideSourceCardIdentity
            };
        }

        private static StructuredAbilityManualFallbackBridgeResult Reject(string rejectionReason)
        {
            return new StructuredAbilityManualFallbackBridgeResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                summary = "Structured ability manual fallback bridge rejected: " + (rejectionReason ?? string.Empty)
            };
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
