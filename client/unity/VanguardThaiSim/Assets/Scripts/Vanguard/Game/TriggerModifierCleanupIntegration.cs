namespace VanguardThaiSim.Game
{
    public static class TriggerModifierCleanupIntegrationRejectionReasons
    {
        public const string CommitResultMissing = "TRIGGER_MODIFIER_CLEANUP_COMMIT_RESULT_MISSING";
        public const string CommitResultRejected = "TRIGGER_MODIFIER_CLEANUP_COMMIT_RESULT_REJECTED";
        public const string LedgerMissing = "TRIGGER_MODIFIER_CLEANUP_LEDGER_MISSING";
    }

    public sealed class TriggerModifierCleanupIntegrationResult
    {
        public bool accepted;
        public string rejection_reason;
        public CombatModifierExpiration cleanup_timing;
        public int expired_modifier_count;
        public int remaining_modifier_count;
        public CombatModifierCleanupPreview cleanup_preview;
    }

    public static class TriggerModifierCleanupIntegration
    {
        public static TriggerModifierCleanupIntegrationResult Cleanup(
            TriggerAllocationCommitResult commitResult,
            CombatModifierExpiration cleanupTiming)
        {
            if (commitResult == null)
            {
                return Reject(TriggerModifierCleanupIntegrationRejectionReasons.CommitResultMissing, cleanupTiming);
            }

            if (!commitResult.accepted)
            {
                return Reject(TriggerModifierCleanupIntegrationRejectionReasons.CommitResultRejected, cleanupTiming);
            }

            if (commitResult.ledger == null)
            {
                return Reject(TriggerModifierCleanupIntegrationRejectionReasons.LedgerMissing, cleanupTiming);
            }

            CombatModifierCleanupPreview preview = CombatModifierCleanupPreviewer.Preview(
                commitResult.ledger,
                cleanupTiming);

            return new TriggerModifierCleanupIntegrationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                cleanup_timing = cleanupTiming,
                expired_modifier_count = preview.expired_modifier_count,
                remaining_modifier_count = preview.remaining_modifier_count,
                cleanup_preview = preview
            };
        }

        private static TriggerModifierCleanupIntegrationResult Reject(
            string rejectionReason,
            CombatModifierExpiration cleanupTiming)
        {
            return new TriggerModifierCleanupIntegrationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                cleanup_timing = cleanupTiming,
                expired_modifier_count = 0,
                remaining_modifier_count = 0,
                cleanup_preview = null
            };
        }
    }
}
