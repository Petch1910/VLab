using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public enum TriggerCheckSource
    {
        Manual,
        Drive,
        Damage
    }

    [Serializable]
    public sealed class TriggerCheckResolutionBundle
    {
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public TriggerCheckSource check_source;
        public int player_index;
        public int check_index;
        public string checked_card_instance_id;
        public string checked_card_id;
        public TriggerType trigger_type;
        public TriggerResolveResult trigger_result = new TriggerResolveResult();
        public TriggerAllocationPlan allocation_plan = new TriggerAllocationPlan();
        public CombatModifierLedger modifier_ledger = new CombatModifierLedger();
        public List<string> notes = new List<string>();
        public string explanation;

        public void EnsureLists()
        {
            if (trigger_result == null) trigger_result = new TriggerResolveResult();
            if (allocation_plan == null) allocation_plan = new TriggerAllocationPlan();
            allocation_plan.EnsureLists();
            if (modifier_ledger == null) modifier_ledger = new CombatModifierLedger();
            modifier_ledger.EnsureLists();
            if (notes == null) notes = new List<string>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TriggerCheckResolutionBundle FromJson(string json)
        {
            TriggerCheckResolutionBundle bundle = JsonUtility.FromJson<TriggerCheckResolutionBundle>(json);
            if (bundle == null)
            {
                throw new ArgumentException("Trigger check resolution bundle JSON could not be parsed.", "json");
            }

            bundle.EnsureLists();
            return bundle;
        }
    }

    public static class TriggerCheckResolutionBundler
    {
        public static TriggerCheckResolutionBundle Build(
            GameState state,
            int playerIndex,
            TriggerCheckSource checkSource,
            int checkIndex,
            string checkedCardInstanceId,
            string checkedCardId,
            TriggerType triggerType,
            CombatModifierExpiration modifierExpiration)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            TriggerResolveResult triggerResult = TriggerResolver.Resolve(triggerType);
            TriggerAllocationPlan allocationPlan = TriggerAllocationPlanner.Plan(state, playerIndex, triggerResult);
            TriggerAllocationModifierResult modifierResult =
                TriggerAllocationModifierAdapter.ConvertToLedger(
                    BuildSourceId(checkSource, checkIndex, checkedCardInstanceId, checkedCardId),
                    allocationPlan,
                    modifierExpiration);

            var bundle = new TriggerCheckResolutionBundle
            {
                accepted = triggerResult.accepted && allocationPlan.accepted && modifierResult.accepted,
                needs_manual_resolution = triggerResult.needs_manual_resolution ||
                                          allocationPlan.needs_manual_resolution ||
                                          modifierResult.needs_manual_resolution,
                rejection_reason = FirstNonEmpty(
                    triggerResult.rejection_reason,
                    allocationPlan.rejection_reason,
                    modifierResult.rejection_reason),
                check_source = checkSource,
                player_index = playerIndex,
                check_index = Math.Max(0, checkIndex),
                checked_card_instance_id = checkedCardInstanceId ?? string.Empty,
                checked_card_id = checkedCardId ?? string.Empty,
                trigger_type = triggerType,
                trigger_result = triggerResult,
                allocation_plan = allocationPlan,
                modifier_ledger = modifierResult.ledger,
                explanation = "advisory trigger check bundle; no card movement or state mutation"
            };

            bundle.notes.Add("Checked-card movement remains manual/future RulesCore work.");
            CopyNotes(modifierResult.notes, bundle.notes);
            bundle.EnsureLists();
            return bundle;
        }

        private static string BuildSourceId(
            TriggerCheckSource checkSource,
            int checkIndex,
            string checkedCardInstanceId,
            string checkedCardId)
        {
            return "trigger-check|" +
                   checkSource +
                   "|" + Math.Max(0, checkIndex) +
                   "|" + (checkedCardInstanceId ?? string.Empty) +
                   "|" + (checkedCardId ?? string.Empty);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
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
