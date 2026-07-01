using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class TriggerCheckLogEntry
    {
        public string log_entry_id;
        public TriggerCheckSource check_source;
        public int player_index;
        public int check_index;
        public string checked_card_instance_id;
        public string checked_card_id;
        public TriggerType trigger_type;
        public bool hides_checked_card_identity;
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public int modifier_count;
        public List<string> modifier_ids = new List<string>();
        public List<string> notes = new List<string>();
        public string summary;

        public void EnsureLists()
        {
            if (modifier_ids == null) modifier_ids = new List<string>();
            if (notes == null) notes = new List<string>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TriggerCheckLogEntry FromJson(string json)
        {
            TriggerCheckLogEntry entry = JsonUtility.FromJson<TriggerCheckLogEntry>(json);
            if (entry == null)
            {
                throw new ArgumentException("Trigger check log entry JSON could not be parsed.", "json");
            }

            entry.EnsureLists();
            return entry;
        }
    }

    public static class TriggerCheckLogEntryFactory
    {
        public static TriggerCheckLogEntry FromBundle(TriggerCheckResolutionBundle bundle)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }

            bundle.EnsureLists();
            var entry = new TriggerCheckLogEntry
            {
                log_entry_id = BuildLogEntryId(bundle),
                check_source = bundle.check_source,
                player_index = bundle.player_index,
                check_index = bundle.check_index,
                checked_card_instance_id = bundle.checked_card_instance_id ?? string.Empty,
                checked_card_id = bundle.checked_card_id ?? string.Empty,
                trigger_type = bundle.trigger_type,
                accepted = bundle.accepted,
                needs_manual_resolution = bundle.needs_manual_resolution,
                rejection_reason = bundle.rejection_reason ?? string.Empty
            };

            CopyModifierIds(bundle.modifier_ledger, entry.modifier_ids);
            CopyNotes(bundle.notes, entry.notes);
            entry.modifier_count = entry.modifier_ids.Count;
            entry.summary = BuildSummary(entry);
            entry.EnsureLists();
            return entry;
        }

        private static string BuildLogEntryId(TriggerCheckResolutionBundle bundle)
        {
            return "trigger-log|" +
                   bundle.check_source +
                   "|" + bundle.player_index +
                   "|" + bundle.check_index +
                   "|" + (bundle.checked_card_instance_id ?? string.Empty) +
                   "|" + (bundle.checked_card_id ?? string.Empty) +
                   "|" + bundle.trigger_type;
        }

        private static string BuildSummary(TriggerCheckLogEntry entry)
        {
            string status = entry.accepted ? "accepted" : "manual";
            return entry.check_source +
                   " check " + entry.check_index +
                   " " + entry.checked_card_id +
                   " " + entry.trigger_type +
                   " " + status +
                   "; modifiers=" + entry.modifier_count;
        }

        private static void CopyModifierIds(CombatModifierLedger ledger, List<string> destination)
        {
            if (ledger == null)
            {
                return;
            }

            ledger.EnsureLists();
            for (int i = 0; i < ledger.modifiers.Count; i++)
            {
                CombatModifier modifier = ledger.modifiers[i];
                if (modifier != null && !string.IsNullOrEmpty(modifier.modifier_id))
                {
                    destination.Add(modifier.modifier_id);
                }
            }
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
