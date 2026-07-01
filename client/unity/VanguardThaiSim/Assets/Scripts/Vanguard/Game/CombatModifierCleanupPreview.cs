using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class CombatModifierCleanupPreview
    {
        public CombatModifierExpiration cleanup_timing;
        public int expired_modifier_count;
        public int remaining_modifier_count;
        public List<string> expired_modifier_ids = new List<string>();
        public CombatModifierLedger remaining_ledger = new CombatModifierLedger();
        public string explanation;

        public void EnsureLists()
        {
            if (expired_modifier_ids == null) expired_modifier_ids = new List<string>();
            if (remaining_ledger == null) remaining_ledger = new CombatModifierLedger();
            remaining_ledger.EnsureLists();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CombatModifierCleanupPreview FromJson(string json)
        {
            CombatModifierCleanupPreview preview = JsonUtility.FromJson<CombatModifierCleanupPreview>(json);
            if (preview == null)
            {
                throw new ArgumentException("Combat modifier cleanup preview JSON could not be parsed.", "json");
            }

            preview.EnsureLists();
            return preview;
        }
    }

    public static class CombatModifierCleanupPreviewer
    {
        public static CombatModifierCleanupPreview Preview(
            CombatModifierLedger ledger,
            CombatModifierExpiration cleanupTiming)
        {
            var preview = new CombatModifierCleanupPreview
            {
                cleanup_timing = cleanupTiming,
                remaining_ledger = new CombatModifierLedger(),
                explanation = "cleanup preview only; no state or input ledger mutation"
            };

            if (ledger == null || ledger.modifiers == null)
            {
                preview.expired_modifier_count = 0;
                preview.remaining_modifier_count = 0;
                return preview;
            }

            for (int i = 0; i < ledger.modifiers.Count; i++)
            {
                CombatModifier modifier = ledger.modifiers[i];
                if (modifier == null)
                {
                    continue;
                }

                if (modifier.expires_at == cleanupTiming)
                {
                    preview.expired_modifier_ids.Add(modifier.modifier_id ?? string.Empty);
                    continue;
                }

                preview.remaining_ledger.Add(modifier);
            }

            preview.expired_modifier_count = preview.expired_modifier_ids.Count;
            preview.remaining_modifier_count = preview.remaining_ledger.modifiers.Count;
            return preview;
        }
    }
}
