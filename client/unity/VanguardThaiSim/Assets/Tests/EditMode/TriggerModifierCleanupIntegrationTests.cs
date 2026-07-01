using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerModifierCleanupIntegrationTests
    {
        [Test]
        public void CleanupEndOfBattleRemovesOnlyEndOfBattleTriggerModifiers()
        {
            TriggerAllocationCommitResult commit = CreateCommitResult();
            string before = commit.ledger.ToJson(false);

            TriggerModifierCleanupIntegrationResult result =
                TriggerModifierCleanupIntegration.Cleanup(commit, CombatModifierExpiration.EndOfBattle);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.expired_modifier_count);
            Assert.AreEqual(3, result.remaining_modifier_count);
            Assert.AreEqual("battle-power", result.cleanup_preview.expired_modifier_ids[0]);
            Assert.AreEqual(0, result.cleanup_preview.remaining_ledger.Summarize("unit-a").total_power_delta);
            Assert.AreEqual(1, result.cleanup_preview.remaining_ledger.Summarize("unit-a").total_critical_delta);
            Assert.AreEqual(before, commit.ledger.ToJson(false));
        }

        [Test]
        public void CleanupEndOfTurnRemovesOnlyEndOfTurnTriggerModifiers()
        {
            TriggerAllocationCommitResult commit = CreateCommitResult();

            TriggerModifierCleanupIntegrationResult result =
                TriggerModifierCleanupIntegration.Cleanup(commit, CombatModifierExpiration.EndOfTurn);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.expired_modifier_count);
            Assert.AreEqual("turn-critical", result.cleanup_preview.expired_modifier_ids[0]);
            Assert.AreEqual(3, result.remaining_modifier_count);
            Assert.AreEqual(10000, result.cleanup_preview.remaining_ledger.Summarize("unit-a").total_power_delta);
            Assert.AreEqual(0, result.cleanup_preview.remaining_ledger.Summarize("unit-a").total_critical_delta);
            Assert.AreEqual(3000, result.cleanup_preview.remaining_ledger.Summarize("unit-b").total_power_delta);
            Assert.AreEqual(2000, result.cleanup_preview.remaining_ledger.Summarize("unit-c").total_power_delta);
        }

        [Test]
        public void CleanupRejectsMissingRejectedOrLedgerlessCommitResult()
        {
            TriggerModifierCleanupIntegrationResult missing =
                TriggerModifierCleanupIntegration.Cleanup(null, CombatModifierExpiration.EndOfTurn);
            TriggerModifierCleanupIntegrationResult rejected =
                TriggerModifierCleanupIntegration.Cleanup(
                    new TriggerAllocationCommitResult
                    {
                        accepted = false,
                        rejection_reason = "NOPE",
                        ledger = CreateLedger()
                    },
                    CombatModifierExpiration.EndOfTurn);
            TriggerModifierCleanupIntegrationResult noLedger =
                TriggerModifierCleanupIntegration.Cleanup(
                    new TriggerAllocationCommitResult
                    {
                        accepted = true,
                        ledger = null
                    },
                    CombatModifierExpiration.EndOfTurn);

            Assert.IsFalse(missing.accepted);
            Assert.AreEqual(TriggerModifierCleanupIntegrationRejectionReasons.CommitResultMissing, missing.rejection_reason);
            Assert.IsNull(missing.cleanup_preview);
            Assert.IsFalse(rejected.accepted);
            Assert.AreEqual(TriggerModifierCleanupIntegrationRejectionReasons.CommitResultRejected, rejected.rejection_reason);
            Assert.IsFalse(noLedger.accepted);
            Assert.AreEqual(TriggerModifierCleanupIntegrationRejectionReasons.LedgerMissing, noLedger.rejection_reason);
        }

        [Test]
        public void CleanupPreviewDoesNotNormalizeNullModifierList()
        {
            var ledger = new CombatModifierLedger
            {
                modifiers = null
            };

            CombatModifierCleanupPreview preview = CombatModifierCleanupPreviewer.Preview(
                ledger,
                CombatModifierExpiration.EndOfTurn);

            Assert.AreEqual(0, preview.expired_modifier_count);
            Assert.AreEqual(0, preview.remaining_modifier_count);
            Assert.IsNull(ledger.modifiers);
        }

        private static TriggerAllocationCommitResult CreateCommitResult()
        {
            return new TriggerAllocationCommitResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                source_id = "trigger-allocation-commit|Drive|0|0|card|Critical",
                check_source = TriggerCheckSource.Drive,
                player_index = 0,
                check_index = 0,
                checked_card_id = "CARD-TRIGGER-1",
                trigger_type = TriggerType.Critical,
                appended_modifier_count = 4,
                ledger = CreateLedger()
            };
        }

        private static CombatModifierLedger CreateLedger()
        {
            var ledger = new CombatModifierLedger();
            ledger.Add(new CombatModifier
            {
                modifier_id = "battle-power",
                source_id = "trigger",
                target_card_instance_id = "unit-a",
                power_delta = 10000,
                expires_at = CombatModifierExpiration.EndOfBattle
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "turn-critical",
                source_id = "trigger",
                target_card_instance_id = "unit-a",
                critical_delta = 1,
                expires_at = CombatModifierExpiration.EndOfTurn
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "permanent-power",
                source_id = "ability",
                target_card_instance_id = "unit-b",
                power_delta = 3000,
                expires_at = CombatModifierExpiration.Permanent
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "manual-power",
                source_id = "manual",
                target_card_instance_id = "unit-c",
                power_delta = 2000,
                expires_at = CombatModifierExpiration.Manual
            });
            return ledger;
        }
    }
}
