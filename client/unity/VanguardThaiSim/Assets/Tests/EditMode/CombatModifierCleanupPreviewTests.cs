using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CombatModifierCleanupPreviewTests
    {
        [Test]
        public void EndOfBattlePreviewExpiresOnlyEndOfBattleModifiers()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierCleanupPreview preview = CombatModifierCleanupPreviewer.Preview(
                ledger,
                CombatModifierExpiration.EndOfBattle);

            Assert.AreEqual(CombatModifierExpiration.EndOfBattle, preview.cleanup_timing);
            Assert.AreEqual(1, preview.expired_modifier_count);
            Assert.AreEqual("battle-power", preview.expired_modifier_ids[0]);
            Assert.AreEqual(2, preview.remaining_modifier_count);
            Assert.AreEqual(0, preview.remaining_ledger.Summarize("unit-a").total_power_delta);
            Assert.AreEqual(1, preview.remaining_ledger.Summarize("unit-a").total_critical_delta);
            Assert.AreEqual(3000, preview.remaining_ledger.Summarize("unit-b").total_power_delta);
        }

        [Test]
        public void EndOfTurnPreviewExpiresOnlyEndOfTurnModifiers()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierCleanupPreview preview = CombatModifierCleanupPreviewer.Preview(
                ledger,
                CombatModifierExpiration.EndOfTurn);

            Assert.AreEqual(1, preview.expired_modifier_count);
            Assert.AreEqual("turn-critical", preview.expired_modifier_ids[0]);
            Assert.AreEqual(2, preview.remaining_modifier_count);
            Assert.AreEqual(10000, preview.remaining_ledger.Summarize("unit-a").total_power_delta);
            Assert.AreEqual(0, preview.remaining_ledger.Summarize("unit-a").total_critical_delta);
        }

        [Test]
        public void PreviewDoesNotMutateOriginalLedger()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierCleanupPreviewer.Preview(ledger, CombatModifierExpiration.EndOfBattle);

            Assert.AreEqual(3, ledger.modifiers.Count);
            Assert.AreEqual(10000, ledger.Summarize("unit-a").total_power_delta);
            Assert.AreEqual(1, ledger.Summarize("unit-a").total_critical_delta);
        }

        [Test]
        public void PreviewJsonRoundTrips()
        {
            CombatModifierCleanupPreview preview = CombatModifierCleanupPreviewer.Preview(
                CreateLedger(),
                CombatModifierExpiration.EndOfBattle);

            CombatModifierCleanupPreview roundTrip = CombatModifierCleanupPreview.FromJson(preview.ToJson());

            Assert.AreEqual(preview.cleanup_timing, roundTrip.cleanup_timing);
            Assert.AreEqual(preview.expired_modifier_count, roundTrip.expired_modifier_count);
            Assert.AreEqual(preview.expired_modifier_ids[0], roundTrip.expired_modifier_ids[0]);
            Assert.AreEqual(preview.remaining_modifier_count, roundTrip.remaining_modifier_count);
        }

        [Test]
        public void PreviewIsDeterministic()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierCleanupPreview first = CombatModifierCleanupPreviewer.Preview(
                ledger,
                CombatModifierExpiration.EndOfTurn);
            CombatModifierCleanupPreview second = CombatModifierCleanupPreviewer.Preview(
                ledger,
                CombatModifierExpiration.EndOfTurn);

            Assert.AreEqual(first.expired_modifier_count, second.expired_modifier_count);
            Assert.AreEqual(first.remaining_modifier_count, second.remaining_modifier_count);
            Assert.AreEqual(first.expired_modifier_ids[0], second.expired_modifier_ids[0]);
            Assert.AreEqual(
                first.remaining_ledger.Summarize("unit-a").total_power_delta,
                second.remaining_ledger.Summarize("unit-a").total_power_delta);
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
                critical_delta = 0,
                expires_at = CombatModifierExpiration.EndOfBattle
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "turn-critical",
                source_id = "trigger",
                target_card_instance_id = "unit-a",
                power_delta = 0,
                critical_delta = 1,
                expires_at = CombatModifierExpiration.EndOfTurn
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "permanent-power",
                source_id = "ability",
                target_card_instance_id = "unit-b",
                power_delta = 3000,
                critical_delta = 0,
                expires_at = CombatModifierExpiration.Permanent
            });
            return ledger;
        }
    }
}
