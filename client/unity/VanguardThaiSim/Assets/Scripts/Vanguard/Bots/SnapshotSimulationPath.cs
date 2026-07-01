using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public static class SnapshotSimulationRejectionReasons
    {
        public const string LiveStateMissing = "SNAPSHOT_SIM_LIVE_STATE_MISSING";
        public const string ActionsMissing = "SNAPSHOT_SIM_ACTIONS_MISSING";
        public const string ActionMissing = "SNAPSHOT_SIM_ACTION_MISSING";
        public const string ActionRejected = "SNAPSHOT_SIM_ACTION_REJECTED";
        public const string LiveStateMutated = "SNAPSHOT_SIM_LIVE_STATE_MUTATED";
    }

    [Serializable]
    public sealed class SnapshotSimulationActionResult
    {
        public int action_index;
        public bool accepted;
        public string rejection_reason;
        public GameActionType action_type;
        public GameEvent game_event;
    }

    [Serializable]
    public sealed class SnapshotSimulationPathResult
    {
        public bool accepted;
        public string rejection_reason;
        public string snapshot_id;
        public int requested_action_count;
        public int accepted_action_count;
        public int branch_event_count;
        public string live_before_hash;
        public string live_after_hash;
        public bool live_unchanged;
        public string branch_state_json;
        public string summary;
        public List<SnapshotSimulationActionResult> action_results =
            new List<SnapshotSimulationActionResult>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static SnapshotSimulationPathResult FromJson(string json)
        {
            SnapshotSimulationPathResult result =
                JsonUtility.FromJson<SnapshotSimulationPathResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Snapshot simulation result JSON could not be parsed.",
                    "json");
            }

            result.EnsureLists();
            return result;
        }

        public GameState RestoreBranchState()
        {
            if (string.IsNullOrWhiteSpace(branch_state_json))
            {
                throw new InvalidOperationException("Branch state JSON is missing.");
            }

            return GameState.FromJson(branch_state_json);
        }

        private void EnsureLists()
        {
            if (action_results == null)
            {
                action_results = new List<SnapshotSimulationActionResult>();
            }
        }
    }

    public static class SnapshotSimulationPath
    {
        public static SnapshotSimulationPathResult Simulate(
            GameState liveState,
            IReadOnlyList<LegalGameAction> actions)
        {
            if (liveState == null)
            {
                return Reject(SnapshotSimulationRejectionReasons.LiveStateMissing);
            }

            if (actions == null)
            {
                return Reject(SnapshotSimulationRejectionReasons.ActionsMissing);
            }

            string liveBeforeJson = NormalizeJson(liveState);
            string liveBeforeHash = StableHash(liveBeforeJson);
            GameStateSnapshot snapshot = GameStateSnapshot.Capture(liveState);
            GameState branch = snapshot.Clone();
            var actionResults = new List<SnapshotSimulationActionResult>(actions.Count);
            bool accepted = true;
            string rejectionReason = string.Empty;
            int acceptedActionCount = 0;

            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action == null)
                {
                    accepted = false;
                    rejectionReason = SnapshotSimulationRejectionReasons.ActionMissing;
                    actionResults.Add(CreateActionResult(i, false, rejectionReason, null, null));
                    break;
                }

                RulesCommandResult commandResult = RulesCore.TryExecute(branch, action);
                actionResults.Add(CreateActionResult(
                    i,
                    commandResult.accepted,
                    commandResult.rejection_reason,
                    action,
                    commandResult.game_event));

                if (!commandResult.accepted)
                {
                    accepted = false;
                    rejectionReason =
                        SnapshotSimulationRejectionReasons.ActionRejected +
                        ": " + commandResult.rejection_reason;
                    break;
                }

                acceptedActionCount++;
            }

            string liveAfterHash = StableHash(NormalizeJson(liveState));
            bool liveUnchanged = liveBeforeHash == liveAfterHash;
            if (!liveUnchanged)
            {
                accepted = false;
                rejectionReason = SnapshotSimulationRejectionReasons.LiveStateMutated;
            }

            string branchJson = branch.ToJson(false);
            string summary = accepted
                ? "Snapshot simulation accepted " + acceptedActionCount + "/" + actions.Count + " actions."
                : "Snapshot simulation rejected: " + rejectionReason;

            return new SnapshotSimulationPathResult
            {
                accepted = accepted,
                rejection_reason = rejectionReason ?? string.Empty,
                snapshot_id = snapshot.snapshot_id ?? string.Empty,
                requested_action_count = actions.Count,
                accepted_action_count = acceptedActionCount,
                branch_event_count = branch.event_log.Count,
                live_before_hash = liveBeforeHash,
                live_after_hash = liveAfterHash,
                live_unchanged = liveUnchanged,
                branch_state_json = branchJson,
                summary = summary,
                action_results = actionResults
            };
        }

        public static SnapshotSimulationPathResult SimulateSingle(
            GameState liveState,
            LegalGameAction action)
        {
            return Simulate(liveState, new List<LegalGameAction> { action });
        }

        private static SnapshotSimulationPathResult Reject(string rejectionReason)
        {
            return new SnapshotSimulationPathResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                summary = "Snapshot simulation rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static SnapshotSimulationActionResult CreateActionResult(
            int actionIndex,
            bool accepted,
            string rejectionReason,
            LegalGameAction action,
            GameEvent gameEvent)
        {
            return new SnapshotSimulationActionResult
            {
                action_index = actionIndex,
                accepted = accepted,
                rejection_reason = rejectionReason ?? string.Empty,
                action_type = action == null ? default(GameActionType) : action.action_type,
                game_event = gameEvent
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
