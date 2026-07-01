using System;

namespace VanguardThaiSim.Game
{
    public static class TriggerAllocationCommitRejectionReasons
    {
        public const string BundleMissing = "TRIGGER_ALLOCATION_COMMIT_BUNDLE_MISSING";
        public const string BundleRejected = "TRIGGER_ALLOCATION_COMMIT_BUNDLE_REJECTED";
        public const string ModifierLedgerMissing = "TRIGGER_ALLOCATION_COMMIT_MODIFIER_LEDGER_MISSING";
    }

    public sealed class TriggerAllocationCommitResult
    {
        public bool accepted;
        public string rejection_reason;
        public string source_id;
        public TriggerCheckSource check_source;
        public int player_index;
        public int check_index;
        public string checked_card_id;
        public TriggerType trigger_type;
        public int appended_modifier_count;
        public CombatModifierLedger ledger;
    }

    public static class TriggerAllocationCommitHelper
    {
        public static TriggerAllocationCommitResult Commit(
            CombatModifierLedger currentLedger,
            TriggerCheckResolutionBundle bundle)
        {
            if (bundle == null)
            {
                return Reject(TriggerAllocationCommitRejectionReasons.BundleMissing, currentLedger, null);
            }

            if (!bundle.accepted)
            {
                string reason = string.IsNullOrWhiteSpace(bundle.rejection_reason)
                    ? TriggerAllocationCommitRejectionReasons.BundleRejected
                    : bundle.rejection_reason;
                return Reject(reason, currentLedger, bundle);
            }

            if (bundle.modifier_ledger == null)
            {
                return Reject(TriggerAllocationCommitRejectionReasons.ModifierLedgerMissing, currentLedger, bundle);
            }

            CombatModifierLedger nextLedger = CloneLedger(currentLedger);
            int beforeCount = nextLedger.modifiers.Count;
            CopyModifiers(bundle.modifier_ledger, nextLedger);

            return new TriggerAllocationCommitResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                source_id = BuildSourceId(bundle),
                check_source = bundle.check_source,
                player_index = bundle.player_index,
                check_index = Math.Max(0, bundle.check_index),
                checked_card_id = bundle.checked_card_id ?? string.Empty,
                trigger_type = bundle.trigger_type,
                appended_modifier_count = nextLedger.modifiers.Count - beforeCount,
                ledger = nextLedger
            };
        }

        private static TriggerAllocationCommitResult Reject(
            string rejectionReason,
            CombatModifierLedger currentLedger,
            TriggerCheckResolutionBundle bundle)
        {
            return new TriggerAllocationCommitResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                source_id = bundle == null ? string.Empty : BuildSourceId(bundle),
                check_source = bundle == null ? TriggerCheckSource.Manual : bundle.check_source,
                player_index = bundle == null ? -1 : bundle.player_index,
                check_index = bundle == null ? -1 : Math.Max(0, bundle.check_index),
                checked_card_id = bundle == null ? string.Empty : bundle.checked_card_id ?? string.Empty,
                trigger_type = bundle == null ? TriggerType.Unknown : bundle.trigger_type,
                appended_modifier_count = 0,
                ledger = CloneLedger(currentLedger)
            };
        }

        private static void CopyModifiers(CombatModifierLedger source, CombatModifierLedger destination)
        {
            if (source == null || source.modifiers == null)
            {
                return;
            }

            for (int i = 0; i < source.modifiers.Count; i++)
            {
                CombatModifier modifier = source.modifiers[i];
                if (modifier != null)
                {
                    destination.Add(modifier);
                }
            }
        }

        private static CombatModifierLedger CloneLedger(CombatModifierLedger source)
        {
            var clone = new CombatModifierLedger();
            if (source == null || source.modifiers == null)
            {
                return clone;
            }

            for (int i = 0; i < source.modifiers.Count; i++)
            {
                CombatModifier modifier = source.modifiers[i];
                if (modifier != null)
                {
                    clone.Add(modifier);
                }
            }

            return clone;
        }

        private static string BuildSourceId(TriggerCheckResolutionBundle bundle)
        {
            return "trigger-allocation-commit|" +
                   bundle.check_source +
                   "|" + bundle.player_index +
                   "|" + Math.Max(0, bundle.check_index) +
                   "|" + (bundle.checked_card_instance_id ?? string.Empty) +
                   "|" + (bundle.checked_card_id ?? string.Empty) +
                   "|" + bundle.trigger_type;
        }
    }
}
