using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class GameStateSnapshot
    {
        public string snapshot_id;
        public string state_json;
        public int event_count;

        public static GameStateSnapshot Capture(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            state.EnsureLists();
            return new GameStateSnapshot
            {
                snapshot_id = "snapshot-" + state.event_log.Count.ToString("D6"),
                state_json = state.ToJson(false),
                event_count = state.event_log.Count
            };
        }

        public GameState Restore()
        {
            if (string.IsNullOrWhiteSpace(state_json))
            {
                throw new InvalidOperationException("Snapshot does not contain state JSON.");
            }

            return GameState.FromJson(state_json);
        }

        public GameState Clone()
        {
            return Restore();
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static GameStateSnapshot FromJson(string json)
        {
            GameStateSnapshot snapshot = JsonUtility.FromJson<GameStateSnapshot>(json);
            if (snapshot == null)
            {
                throw new ArgumentException("Snapshot JSON could not be parsed.", nameof(json));
            }

            return snapshot;
        }
    }
}
