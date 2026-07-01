using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public enum CombatModifierExpiration
    {
        Manual,
        EndOfBattle,
        EndOfTurn,
        Permanent
    }

    [Serializable]
    public sealed class CombatModifier
    {
        public string modifier_id;
        public string source_id;
        public string target_card_instance_id;
        public int power_delta;
        public int critical_delta;
        public CombatModifierExpiration expires_at;
        public string note;
    }

    [Serializable]
    public sealed class CombatModifierSummary
    {
        public string target_card_instance_id;
        public int total_power_delta;
        public int total_critical_delta;
        public int modifier_count;
    }

    [Serializable]
    public sealed class CombatModifierLedger
    {
        public List<CombatModifier> modifiers = new List<CombatModifier>();

        public void EnsureLists()
        {
            if (modifiers == null) modifiers = new List<CombatModifier>();
        }

        public void Add(CombatModifier modifier)
        {
            EnsureLists();
            if (modifier == null)
            {
                throw new ArgumentNullException("modifier");
            }

            modifiers.Add(CloneModifier(modifier));
        }

        public CombatModifierSummary Summarize(string targetCardInstanceId)
        {
            EnsureLists();
            var summary = new CombatModifierSummary
            {
                target_card_instance_id = targetCardInstanceId ?? string.Empty
            };

            for (int i = 0; i < modifiers.Count; i++)
            {
                CombatModifier modifier = modifiers[i];
                if (modifier == null || modifier.target_card_instance_id != targetCardInstanceId)
                {
                    continue;
                }

                summary.total_power_delta += modifier.power_delta;
                summary.total_critical_delta += modifier.critical_delta;
                summary.modifier_count++;
            }

            return summary;
        }

        public List<CombatModifier> ListForTarget(string targetCardInstanceId)
        {
            EnsureLists();
            var result = new List<CombatModifier>();
            for (int i = 0; i < modifiers.Count; i++)
            {
                CombatModifier modifier = modifiers[i];
                if (modifier != null && modifier.target_card_instance_id == targetCardInstanceId)
                {
                    result.Add(CloneModifier(modifier));
                }
            }

            return result;
        }

        public CombatModifierLedger WithoutExpired(CombatModifierExpiration expiredTiming)
        {
            EnsureLists();
            var filtered = new CombatModifierLedger();
            for (int i = 0; i < modifiers.Count; i++)
            {
                CombatModifier modifier = modifiers[i];
                if (modifier == null || modifier.expires_at == expiredTiming)
                {
                    continue;
                }

                filtered.modifiers.Add(CloneModifier(modifier));
            }

            return filtered;
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CombatModifierLedger FromJson(string json)
        {
            CombatModifierLedger ledger = JsonUtility.FromJson<CombatModifierLedger>(json);
            if (ledger == null)
            {
                throw new ArgumentException("Combat modifier ledger JSON could not be parsed.", "json");
            }

            ledger.EnsureLists();
            return ledger;
        }

        private static CombatModifier CloneModifier(CombatModifier modifier)
        {
            return new CombatModifier
            {
                modifier_id = modifier.modifier_id,
                source_id = modifier.source_id,
                target_card_instance_id = modifier.target_card_instance_id,
                power_delta = modifier.power_delta,
                critical_delta = modifier.critical_delta,
                expires_at = modifier.expires_at,
                note = modifier.note
            };
        }
    }
}
