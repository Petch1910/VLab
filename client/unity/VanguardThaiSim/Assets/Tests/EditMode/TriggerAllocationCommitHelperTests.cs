using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerAllocationCommitHelperTests
    {
        [Test]
        public void CommitAppendsTriggerModifiersToLedgerCloneWithoutMutatingInputs()
        {
            CombatModifierLedger current = CreateCurrentLedger();
            TriggerCheckResolutionBundle bundle = CreateBundle("CARD-TRIGGER-1", "trigger-card-instance-1");
            string beforeCurrent = current.ToJson(false);
            string beforeBundle = bundle.ToJson(false);

            TriggerAllocationCommitResult result = TriggerAllocationCommitHelper.Commit(current, bundle);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.appended_modifier_count);
            Assert.AreEqual(3, result.ledger.modifiers.Count);
            Assert.AreEqual(10000, result.ledger.Summarize("unit-a").total_power_delta);
            Assert.AreEqual(1, result.ledger.Summarize("vg").total_critical_delta);
            Assert.AreEqual(TriggerType.Critical, result.trigger_type);
            Assert.IsTrue(result.source_id.Contains("CARD-TRIGGER-1"));
            Assert.AreEqual(beforeCurrent, current.ToJson(false));
            Assert.AreEqual(beforeBundle, bundle.ToJson(false));
        }

        [Test]
        public void CommitAcceptsEmptyTriggerModifierLedger()
        {
            TriggerCheckResolutionBundle bundle = CreateBundle("CARD-TRIGGER-1", "trigger-card-instance-1");
            bundle.modifier_ledger = new CombatModifierLedger();

            TriggerAllocationCommitResult result = TriggerAllocationCommitHelper.Commit(null, bundle);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(0, result.appended_modifier_count);
            Assert.AreEqual(0, result.ledger.modifiers.Count);
        }

        [Test]
        public void CommitRejectsMissingOrRejectedInputsWithoutMutatingLedger()
        {
            CombatModifierLedger current = CreateCurrentLedger();
            string before = current.ToJson(false);
            TriggerCheckResolutionBundle rejectedBundle = CreateBundle("CARD-TRIGGER-1", "trigger-card-instance-1");
            rejectedBundle.accepted = false;
            rejectedBundle.rejection_reason = "TRIGGER_CHECK_REJECTED";
            TriggerCheckResolutionBundle missingLedger = CreateBundle("CARD-TRIGGER-1", "trigger-card-instance-1");
            missingLedger.modifier_ledger = null;

            TriggerAllocationCommitResult missing =
                TriggerAllocationCommitHelper.Commit(current, null);
            TriggerAllocationCommitResult rejected =
                TriggerAllocationCommitHelper.Commit(current, rejectedBundle);
            TriggerAllocationCommitResult noLedger =
                TriggerAllocationCommitHelper.Commit(current, missingLedger);

            Assert.IsFalse(missing.accepted);
            Assert.AreEqual(TriggerAllocationCommitRejectionReasons.BundleMissing, missing.rejection_reason);
            Assert.IsFalse(rejected.accepted);
            Assert.AreEqual("TRIGGER_CHECK_REJECTED", rejected.rejection_reason);
            Assert.IsFalse(noLedger.accepted);
            Assert.AreEqual(TriggerAllocationCommitRejectionReasons.ModifierLedgerMissing, noLedger.rejection_reason);
            Assert.AreEqual(before, current.ToJson(false));
            Assert.AreEqual(1, rejected.ledger.modifiers.Count);
            Assert.AreEqual(1, noLedger.ledger.modifiers.Count);
        }

        [Test]
        public void CommitPreservesMaskedTriggerSourceMetadata()
        {
            TriggerCheckResolutionBundle bundle = CreateBundle(
                GameStateViewFactory.HiddenCardId,
                "hidden-trigger-check-0000");
            bundle.modifier_ledger.modifiers[0].source_id = "hidden-trigger-modifier-source";
            bundle.modifier_ledger.modifiers[1].source_id = "hidden-trigger-modifier-source";

            TriggerAllocationCommitResult result = TriggerAllocationCommitHelper.Commit(null, bundle);
            string json = result.ledger.ToJson(false);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, result.checked_card_id);
            Assert.IsTrue(result.source_id.Contains(GameStateViewFactory.HiddenCardId));
            Assert.IsFalse(json.Contains("CARD-TRIGGER-1"));
            Assert.IsFalse(json.Contains("trigger-card-instance-1"));
            Assert.IsTrue(json.Contains("hidden-trigger-modifier-source"));
        }

        private static CombatModifierLedger CreateCurrentLedger()
        {
            var ledger = new CombatModifierLedger();
            ledger.Add(
                new CombatModifier
                {
                    modifier_id = "existing",
                    source_id = "existing-source",
                    target_card_instance_id = "existing-unit",
                    power_delta = 5000,
                    expires_at = CombatModifierExpiration.EndOfTurn,
                    note = "existing"
                });
            return ledger;
        }

        private static TriggerCheckResolutionBundle CreateBundle(
            string checkedCardId,
            string checkedCardInstanceId)
        {
            var bundle = new TriggerCheckResolutionBundle
            {
                accepted = true,
                needs_manual_resolution = false,
                rejection_reason = string.Empty,
                check_source = TriggerCheckSource.Drive,
                player_index = 0,
                check_index = 0,
                checked_card_instance_id = checkedCardInstanceId,
                checked_card_id = checkedCardId,
                trigger_type = TriggerType.Critical,
                modifier_ledger = new CombatModifierLedger(),
                notes = new List<string>()
            };
            bundle.modifier_ledger.Add(
                new CombatModifier
                {
                    modifier_id = "trigger-power",
                    source_id = "trigger-source",
                    target_card_instance_id = "unit-a",
                    power_delta = 10000,
                    expires_at = CombatModifierExpiration.EndOfTurn,
                    note = "power"
                });
            bundle.modifier_ledger.Add(
                new CombatModifier
                {
                    modifier_id = "trigger-critical",
                    source_id = "trigger-source",
                    target_card_instance_id = "vg",
                    critical_delta = 1,
                    expires_at = CombatModifierExpiration.EndOfTurn,
                    note = "critical"
                });
            bundle.EnsureLists();
            return bundle;
        }
    }
}
