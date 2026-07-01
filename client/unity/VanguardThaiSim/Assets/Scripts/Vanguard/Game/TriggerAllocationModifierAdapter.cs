using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public sealed class TriggerAllocationModifierResult
    {
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public CombatModifierLedger ledger = new CombatModifierLedger();
        public List<string> notes = new List<string>();

        public void EnsureLists()
        {
            if (ledger == null) ledger = new CombatModifierLedger();
            ledger.EnsureLists();
            if (notes == null) notes = new List<string>();
        }

        public static TriggerAllocationModifierResult Accepted()
        {
            return new TriggerAllocationModifierResult
            {
                accepted = true,
                needs_manual_resolution = false,
                rejection_reason = string.Empty
            };
        }

        public static TriggerAllocationModifierResult NeedsManualResolution(string reason)
        {
            return new TriggerAllocationModifierResult
            {
                accepted = false,
                needs_manual_resolution = true,
                rejection_reason = reason ?? string.Empty
            };
        }
    }

    public static class TriggerAllocationModifierAdapter
    {
        public static TriggerAllocationModifierResult ConvertToLedger(
            string sourceId,
            TriggerAllocationPlan plan,
            CombatModifierExpiration expiresAt)
        {
            if (plan == null)
            {
                return TriggerAllocationModifierResult.NeedsManualResolution("Trigger allocation plan is required.");
            }

            plan.EnsureLists();
            if (!plan.accepted)
            {
                return TriggerAllocationModifierResult.NeedsManualResolution(plan.rejection_reason);
            }

            TriggerAllocationModifierResult result = TriggerAllocationModifierResult.Accepted();
            string safeSourceId = string.IsNullOrEmpty(sourceId) ? "trigger-allocation" : sourceId;

            for (int i = 0; i < plan.power_targets.Count; i++)
            {
                TriggerAllocationTarget target = plan.power_targets[i];
                if (target == null || target.power_bonus == 0)
                {
                    continue;
                }

                result.ledger.Add(CreateModifier(
                    safeSourceId,
                    target,
                    target.power_bonus,
                    0,
                    expiresAt,
                    "power from " + plan.trigger_type));
            }

            for (int i = 0; i < plan.critical_targets.Count; i++)
            {
                TriggerAllocationTarget target = plan.critical_targets[i];
                if (target == null || target.critical_bonus == 0)
                {
                    continue;
                }

                result.ledger.Add(CreateModifier(
                    safeSourceId,
                    target,
                    0,
                    target.critical_bonus,
                    expiresAt,
                    "critical from " + plan.trigger_type));
            }

            CopyNotes(plan.side_effect_notes, result.notes);
            CopyNotes(plan.marker_notes, result.notes);
            result.EnsureLists();
            return result;
        }

        private static CombatModifier CreateModifier(
            string sourceId,
            TriggerAllocationTarget target,
            int powerDelta,
            int criticalDelta,
            CombatModifierExpiration expiresAt,
            string note)
        {
            return new CombatModifier
            {
                modifier_id = BuildModifierId(sourceId, target.card_instance_id, powerDelta, criticalDelta),
                source_id = sourceId,
                target_card_instance_id = target.card_instance_id,
                power_delta = powerDelta,
                critical_delta = criticalDelta,
                expires_at = expiresAt,
                note = note
            };
        }

        private static string BuildModifierId(
            string sourceId,
            string targetCardInstanceId,
            int powerDelta,
            int criticalDelta)
        {
            return sourceId +
                   "|" + (targetCardInstanceId ?? string.Empty) +
                   "|p" + powerDelta +
                   "|c" + criticalDelta;
        }

        private static void CopyNotes(IReadOnlyList<string> source, List<string> destination)
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
