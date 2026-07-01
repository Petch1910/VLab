using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class SnapshotRollbackRejectionReasons
    {
        public const string LiveStateMissing = "SNAPSHOT_ROLLBACK_LIVE_STATE_MISSING";
        public const string BranchActionMissing = "SNAPSHOT_ROLLBACK_BRANCH_ACTION_MISSING";
        public const string BranchActionRejected = "SNAPSHOT_ROLLBACK_BRANCH_ACTION_REJECTED";
        public const string LiveStateMutated = "SNAPSHOT_ROLLBACK_LIVE_STATE_MUTATED";
        public const string RestoreMismatch = "SNAPSHOT_ROLLBACK_RESTORE_MISMATCH";
        public const string BranchDidNotChange = "SNAPSHOT_ROLLBACK_BRANCH_DID_NOT_CHANGE";
    }

    [Serializable]
    public sealed class SnapshotRollbackVerificationResult
    {
        public bool accepted;
        public string rejection_reason;
        public string snapshot_id;
        public string live_before_hash;
        public string live_after_hash;
        public string restored_hash;
        public string branch_after_hash;
        public bool live_unchanged;
        public bool restore_matches_live_before;
        public bool branch_changed;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static SnapshotRollbackVerificationResult FromJson(string json)
        {
            SnapshotRollbackVerificationResult result =
                JsonUtility.FromJson<SnapshotRollbackVerificationResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Snapshot rollback verification result JSON could not be parsed.",
                    "json");
            }

            return result;
        }
    }

    public static class SnapshotRollbackVerifier
    {
        public static SnapshotRollbackVerificationResult VerifyBranchIsolation(
            GameState liveState,
            LegalGameAction branchAction)
        {
            if (liveState == null)
            {
                return Reject(SnapshotRollbackRejectionReasons.LiveStateMissing);
            }

            if (branchAction == null)
            {
                return Reject(SnapshotRollbackRejectionReasons.BranchActionMissing);
            }

            string liveBeforeJson = NormalizeJson(liveState);
            string liveBeforeHash = StableHash(liveBeforeJson);
            GameStateSnapshot snapshot = GameStateSnapshot.Capture(liveState);
            GameState branch = snapshot.Clone();

            RulesCommandResult branchResult = RulesCore.TryExecute(branch, branchAction);
            string liveAfterHash = StableHash(NormalizeJson(liveState));
            if (!branchResult.accepted)
            {
                return Reject(
                    SnapshotRollbackRejectionReasons.BranchActionRejected + ": " + branchResult.rejection_reason,
                    snapshot.snapshot_id,
                    liveBeforeHash,
                    liveAfterHash,
                    string.Empty,
                    StableHash(NormalizeJson(branch)),
                    liveBeforeHash == liveAfterHash,
                    false,
                    false);
            }

            string branchAfterHash = StableHash(NormalizeJson(branch));
            GameState restored = snapshot.Restore();
            string restoredHash = StableHash(NormalizeJson(restored));
            bool liveUnchanged = liveBeforeHash == liveAfterHash;
            bool restoreMatches = liveBeforeHash == restoredHash;
            bool branchChanged = liveBeforeHash != branchAfterHash;

            if (!liveUnchanged)
            {
                return Reject(
                    SnapshotRollbackRejectionReasons.LiveStateMutated,
                    snapshot.snapshot_id,
                    liveBeforeHash,
                    liveAfterHash,
                    restoredHash,
                    branchAfterHash,
                    liveUnchanged,
                    restoreMatches,
                    branchChanged);
            }

            if (!restoreMatches)
            {
                return Reject(
                    SnapshotRollbackRejectionReasons.RestoreMismatch,
                    snapshot.snapshot_id,
                    liveBeforeHash,
                    liveAfterHash,
                    restoredHash,
                    branchAfterHash,
                    liveUnchanged,
                    restoreMatches,
                    branchChanged);
            }

            if (!branchChanged)
            {
                return Reject(
                    SnapshotRollbackRejectionReasons.BranchDidNotChange,
                    snapshot.snapshot_id,
                    liveBeforeHash,
                    liveAfterHash,
                    restoredHash,
                    branchAfterHash,
                    liveUnchanged,
                    restoreMatches,
                    branchChanged);
            }

            return new SnapshotRollbackVerificationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                snapshot_id = snapshot.snapshot_id ?? string.Empty,
                live_before_hash = liveBeforeHash,
                live_after_hash = liveAfterHash,
                restored_hash = restoredHash,
                branch_after_hash = branchAfterHash,
                live_unchanged = true,
                restore_matches_live_before = true,
                branch_changed = true,
                summary = "Snapshot branch mutated independently and restore matched live-before state."
            };
        }

        private static SnapshotRollbackVerificationResult Reject(string rejectionReason)
        {
            return Reject(
                rejectionReason,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                false,
                false);
        }

        private static SnapshotRollbackVerificationResult Reject(
            string rejectionReason,
            string snapshotId,
            string liveBeforeHash,
            string liveAfterHash,
            string restoredHash,
            string branchAfterHash,
            bool liveUnchanged,
            bool restoreMatches,
            bool branchChanged)
        {
            return new SnapshotRollbackVerificationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                snapshot_id = snapshotId ?? string.Empty,
                live_before_hash = liveBeforeHash ?? string.Empty,
                live_after_hash = liveAfterHash ?? string.Empty,
                restored_hash = restoredHash ?? string.Empty,
                branch_after_hash = branchAfterHash ?? string.Empty,
                live_unchanged = liveUnchanged,
                restore_matches_live_before = restoreMatches,
                branch_changed = branchChanged,
                summary = "Snapshot rollback rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static string NormalizeJson(GameState state)
        {
            if (state == null)
            {
                return "<null>";
            }

            GameState clone = GameState.FromJson(JsonUtility.ToJson(state, false));
            return clone.ToJson(false);
        }

        private static string StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                string safeValue = value ?? string.Empty;
                for (int i = 0; i < safeValue.Length; i++)
                {
                    hash ^= safeValue[i];
                    hash *= 16777619;
                }

                return hash.ToString("x8");
            }
        }
    }
}
