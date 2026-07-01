using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredModifierEffectTemplateRejectionReasons
    {
        public const string StateMissing = "STRUCTURED_MODIFIER_EFFECT_STATE_MISSING";
        public const string EffectMissing = "STRUCTURED_MODIFIER_EFFECT_MISSING";
        public const string TargetMissing = "STRUCTURED_MODIFIER_EFFECT_TARGET_MISSING";
        public const string UnsupportedEffectType = "STRUCTURED_MODIFIER_EFFECT_UNSUPPORTED_TYPE";
        public const string AmountMissingOrZero = "STRUCTURED_MODIFIER_EFFECT_AMOUNT_MISSING_OR_ZERO";
        public const string NegativeAmount = "STRUCTURED_MODIFIER_EFFECT_NEGATIVE_AMOUNT";
        public const string UnsupportedDuration = "STRUCTURED_MODIFIER_EFFECT_UNSUPPORTED_DURATION";
        public const string TargetProjectionRejected = "STRUCTURED_MODIFIER_EFFECT_TARGET_PROJECTION_REJECTED";
    }

    [Serializable]
    public sealed class StructuredModifierEffectTemplateResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public bool preview_only;
        public CombatModifier modifier;
        public CombatModifierLedger ledger_after = new CombatModifierLedger();
        public CombatStatProjection projection_after;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            EnsureObjects();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredModifierEffectTemplateResult FromJson(string json)
        {
            StructuredModifierEffectTemplateResult result =
                JsonUtility.FromJson<StructuredModifierEffectTemplateResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Structured modifier effect template result JSON could not be parsed.",
                    "json");
            }

            result.EnsureObjects();
            return result;
        }

        public void EnsureObjects()
        {
            if (ledger_after == null)
            {
                ledger_after = new CombatModifierLedger();
            }

            ledger_after.EnsureLists();
        }
    }

    public static class StructuredModifierEffectTemplate
    {
        public static StructuredModifierEffectTemplateResult Preview(
            GameState state,
            int playerIndex,
            CombatModifierLedger sourceLedger,
            StructuredAbilityEffect effect,
            StructuredTargetCandidate target,
            StructuredAbilityDuration duration,
            string sourceId)
        {
            CombatModifierLedger workingLedger = CloneLedger(sourceLedger);
            StructuredModifierEffectTemplateResult result =
                BuildAcceptedResult(state, playerIndex, workingLedger, effect, target, duration, sourceId);
            result.preview_only = true;
            return result;
        }

        public static StructuredModifierEffectTemplateResult ApplyToLedger(
            GameState state,
            int playerIndex,
            CombatModifierLedger ledger,
            StructuredAbilityEffect effect,
            StructuredTargetCandidate target,
            StructuredAbilityDuration duration,
            string sourceId)
        {
            if (ledger == null)
            {
                ledger = new CombatModifierLedger();
            }

            StructuredModifierEffectTemplateResult result =
                BuildAcceptedResult(state, playerIndex, CloneLedger(ledger), effect, target, duration, sourceId);
            if (!result.accepted)
            {
                return result;
            }

            ledger.Add(result.modifier);
            result.ledger_after = CloneLedger(ledger);
            result.projection_after = CombatStatProjector.Project(
                state,
                target.player_index,
                target.instance_id,
                result.ledger_after);
            result.preview_only = false;
            return result;
        }

        private static StructuredModifierEffectTemplateResult BuildAcceptedResult(
            GameState state,
            int playerIndex,
            CombatModifierLedger workingLedger,
            StructuredAbilityEffect effect,
            StructuredTargetCandidate target,
            StructuredAbilityDuration duration,
            string sourceId)
        {
            StructuredModifierEffectTemplateResult rejected = Validate(
                state,
                playerIndex,
                workingLedger,
                effect,
                target,
                duration,
                sourceId,
                out CombatModifier modifier,
                out CombatStatProjection beforeProjection);
            if (rejected != null)
            {
                return rejected;
            }

            workingLedger.Add(modifier);
            CombatStatProjection projection = CombatStatProjector.Project(
                state,
                target.player_index,
                target.instance_id,
                workingLedger);

            return new StructuredModifierEffectTemplateResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                preview_only = false,
                modifier = CloneModifier(modifier),
                ledger_after = CloneLedger(workingLedger),
                projection_after = projection,
                summary = "Structured modifier effect applied to " + target.instance_id + "."
            };
        }

        private static StructuredModifierEffectTemplateResult Validate(
            GameState state,
            int playerIndex,
            CombatModifierLedger workingLedger,
            StructuredAbilityEffect effect,
            StructuredTargetCandidate target,
            StructuredAbilityDuration duration,
            string sourceId,
            out CombatModifier modifier,
            out CombatStatProjection beforeProjection)
        {
            modifier = null;
            beforeProjection = null;

            if (state == null)
            {
                return Reject(StructuredModifierEffectTemplateRejectionReasons.StateMissing, false);
            }

            if (effect == null)
            {
                return Reject(StructuredModifierEffectTemplateRejectionReasons.EffectMissing, false);
            }

            if (target == null || string.IsNullOrEmpty(target.instance_id))
            {
                return Reject(StructuredModifierEffectTemplateRejectionReasons.TargetMissing, false);
            }

            string effectType = effect.type ?? string.Empty;
            if (effectType != "power_plus" && effectType != "critical_plus")
            {
                return Reject(
                    StructuredModifierEffectTemplateRejectionReasons.UnsupportedEffectType + ": " + effectType,
                    true);
            }

            if (effect.amount < 0)
            {
                return Reject(StructuredModifierEffectTemplateRejectionReasons.NegativeAmount, false);
            }

            if (effect.amount == 0)
            {
                return Reject(StructuredModifierEffectTemplateRejectionReasons.AmountMissingOrZero, false);
            }

            if (!TryResolveExpiration(duration, effect.duration_ref, out CombatModifierExpiration expiration))
            {
                return Reject(
                    StructuredModifierEffectTemplateRejectionReasons.UnsupportedDuration + ": " +
                    (duration == null ? effect.duration_ref : duration.type + "/" + duration.cleanup_timing),
                    true);
            }

            int targetPlayerIndex = target.player_index;
            beforeProjection = CombatStatProjector.Project(
                state,
                targetPlayerIndex,
                target.instance_id,
                workingLedger ?? new CombatModifierLedger());
            if (!beforeProjection.accepted)
            {
                return Reject(
                    StructuredModifierEffectTemplateRejectionReasons.TargetProjectionRejected + ": " +
                    beforeProjection.rejection_reason,
                    false);
            }

            modifier = new CombatModifier
            {
                modifier_id = BuildModifierId(sourceId, effectType, target.instance_id, effect.amount, expiration),
                source_id = string.IsNullOrEmpty(sourceId) ? "structured-modifier-effect" : sourceId,
                target_card_instance_id = target.instance_id,
                power_delta = effectType == "power_plus" ? effect.amount : 0,
                critical_delta = effectType == "critical_plus" ? effect.amount : 0,
                expires_at = expiration,
                note = effectType + " from structured ability effect"
            };
            return null;
        }

        private static bool TryResolveExpiration(
            StructuredAbilityDuration duration,
            string durationRef,
            out CombatModifierExpiration expiration)
        {
            expiration = CombatModifierExpiration.Manual;
            string cleanupTiming = duration == null ? string.Empty : duration.cleanup_timing ?? string.Empty;
            string durationType = duration == null ? durationRef ?? string.Empty : duration.type ?? string.Empty;

            switch (cleanupTiming)
            {
                case "end_of_battle":
                    expiration = CombatModifierExpiration.EndOfBattle;
                    return true;
                case "end_of_turn":
                    expiration = CombatModifierExpiration.EndOfTurn;
                    return true;
                case "manual":
                    expiration = CombatModifierExpiration.Manual;
                    return true;
            }

            switch (durationType)
            {
                case "until_end_of_battle":
                    expiration = CombatModifierExpiration.EndOfBattle;
                    return true;
                case "until_end_of_turn":
                    expiration = CombatModifierExpiration.EndOfTurn;
                    return true;
                case "continuous":
                    expiration = CombatModifierExpiration.Permanent;
                    return true;
                case "manual":
                    expiration = CombatModifierExpiration.Manual;
                    return true;
                default:
                    return false;
            }
        }

        private static StructuredModifierEffectTemplateResult Reject(
            string rejectionReason,
            bool requiresManualResolution)
        {
            return new StructuredModifierEffectTemplateResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                requires_manual_resolution = requiresManualResolution,
                preview_only = false,
                ledger_after = new CombatModifierLedger(),
                summary = "Structured modifier effect template rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static string BuildModifierId(
            string sourceId,
            string effectType,
            string targetCardInstanceId,
            int amount,
            CombatModifierExpiration expiration)
        {
            string safeSource = string.IsNullOrEmpty(sourceId) ? "structured-modifier-effect" : sourceId;
            return safeSource +
                   "|" + (effectType ?? string.Empty) +
                   "|" + (targetCardInstanceId ?? string.Empty) +
                   "|" + amount +
                   "|" + expiration;
        }

        private static CombatModifierLedger CloneLedger(CombatModifierLedger ledger)
        {
            if (ledger == null)
            {
                return new CombatModifierLedger();
            }

            return CombatModifierLedger.FromJson(ledger.ToJson(false));
        }

        private static CombatModifier CloneModifier(CombatModifier modifier)
        {
            if (modifier == null)
            {
                return null;
            }

            return JsonUtility.FromJson<CombatModifier>(JsonUtility.ToJson(modifier, false));
        }
    }
}
