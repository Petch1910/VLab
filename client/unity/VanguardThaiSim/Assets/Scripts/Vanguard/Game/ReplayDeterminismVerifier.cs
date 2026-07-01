using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class ReplayDeterminismRejectionReasons
    {
        public const string InitialStateMissing = "REPLAY_DETERMINISM_INITIAL_STATE_MISSING";
        public const string FinalStateMissing = "REPLAY_DETERMINISM_FINAL_STATE_MISSING";
        public const string ReplayApplyFailed = "REPLAY_DETERMINISM_APPLY_FAILED";
        public const string FinalStateMismatch = "REPLAY_DETERMINISM_FINAL_STATE_MISMATCH";
    }

    [Serializable]
    public sealed class ReplayDeterminismVerificationResult
    {
        public bool accepted;
        public string rejection_reason;
        public string replay_id;
        public int event_count;
        public string expected_final_state_hash;
        public string replayed_final_state_hash;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static ReplayDeterminismVerificationResult FromJson(string json)
        {
            ReplayDeterminismVerificationResult result =
                JsonUtility.FromJson<ReplayDeterminismVerificationResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Replay determinism verification result JSON could not be parsed.",
                    "json");
            }

            return result;
        }
    }

    public static class ReplayDeterminismVerifier
    {
        public static ReplayDeterminismVerificationResult Verify(
            GameState initialState,
            GameState expectedFinalState,
            IReadOnlyList<GameEvent> events)
        {
            if (initialState == null)
            {
                return Reject(
                    ReplayDeterminismRejectionReasons.InitialStateMissing,
                    string.Empty,
                    0,
                    string.Empty,
                    string.Empty);
            }

            if (expectedFinalState == null)
            {
                return Reject(
                    ReplayDeterminismRejectionReasons.FinalStateMissing,
                    string.Empty,
                    CountEvents(events),
                    string.Empty,
                    string.Empty);
            }

            GameReplay replay = CreateReplay(initialState, events);
            GameReplayPlayer player;
            try
            {
                player = new GameReplayPlayer(replay);
                player.JumpToEnd();
            }
            catch (Exception exception)
            {
                return Reject(
                    ReplayDeterminismRejectionReasons.ReplayApplyFailed + ": " + exception.Message,
                    replay.replay_id,
                    CountEvents(events),
                    NormalizeHash(expectedFinalState),
                    string.Empty);
            }

            string expectedHash = NormalizeHash(expectedFinalState);
            string replayedHash = NormalizeHash(player.CurrentState);
            if (!string.Equals(expectedHash, replayedHash, StringComparison.Ordinal))
            {
                return Reject(
                    ReplayDeterminismRejectionReasons.FinalStateMismatch,
                    replay.replay_id,
                    CountEvents(events),
                    expectedHash,
                    replayedHash);
            }

            return new ReplayDeterminismVerificationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                replay_id = replay.replay_id,
                event_count = CountEvents(events),
                expected_final_state_hash = expectedHash,
                replayed_final_state_hash = replayedHash,
                summary = "Replay reproduced final state for " + CountEvents(events) + " event(s)."
            };
        }

        private static ReplayDeterminismVerificationResult Reject(
            string rejectionReason,
            string replayId,
            int eventCount,
            string expectedHash,
            string replayedHash)
        {
            return new ReplayDeterminismVerificationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                replay_id = replayId ?? string.Empty,
                event_count = eventCount,
                expected_final_state_hash = expectedHash ?? string.Empty,
                replayed_final_state_hash = replayedHash ?? string.Empty,
                summary = "Replay determinism rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static GameReplay CreateReplay(GameState initialState, IReadOnlyList<GameEvent> events)
        {
            GameState normalizedInitial = CloneState(initialState);
            normalizedInitial.EnsureLists();
            normalizedInitial.event_log.Clear();

            var replay = new GameReplay
            {
                replay_id = "replay-determinism-verifier|" +
                            StableHash(normalizedInitial.ToJson(false)) +
                            "|" + CountEvents(events),
                initial_state_json = normalizedInitial.ToJson(false),
                events = new List<GameEvent>()
            };

            if (events != null)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    if (events[i] != null)
                    {
                        replay.events.Add(CloneEvent(events[i]));
                    }
                }
            }

            return replay;
        }

        private static string NormalizeHash(GameState state)
        {
            return StableHash(NormalizeJson(state));
        }

        private static string NormalizeJson(GameState state)
        {
            if (state == null)
            {
                return "<null>";
            }

            GameState clone = CloneState(state);
            return clone.ToJson(false);
        }

        private static GameState CloneState(GameState state)
        {
            if (state == null)
            {
                return null;
            }

            return GameState.FromJson(JsonUtility.ToJson(state, false));
        }

        private static GameEvent CloneEvent(GameEvent gameEvent)
        {
            return JsonUtility.FromJson<GameEvent>(JsonUtility.ToJson(gameEvent, false));
        }

        private static int CountEvents(IReadOnlyList<GameEvent> events)
        {
            return events == null ? 0 : events.Count;
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
