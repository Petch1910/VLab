using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class GameReplay
    {
        public string replay_id;
        public string initial_state_json;
        public List<GameEvent> events = new List<GameEvent>();

        public static GameReplay Create(GameState initialState, IReadOnlyList<GameEvent> events)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            GameState normalizedInitial = GameState.FromJson(initialState.ToJson(false));
            normalizedInitial.event_log.Clear();

            GameReplay replay = new GameReplay
            {
                replay_id = Guid.NewGuid().ToString("N"),
                initial_state_json = normalizedInitial.ToJson(false)
            };

            if (events != null)
            {
                replay.events.AddRange(events);
            }

            return replay;
        }

        public GameState CreateInitialState()
        {
            if (string.IsNullOrWhiteSpace(initial_state_json))
            {
                throw new InvalidOperationException("Replay does not contain an initial state.");
            }

            GameState state = GameState.FromJson(initial_state_json);
            state.event_log.Clear();
            return state;
        }

        public string ToJson(bool prettyPrint = false)
        {
            if (events == null)
            {
                events = new List<GameEvent>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static GameReplay FromJson(string json)
        {
            GameReplay replay = JsonUtility.FromJson<GameReplay>(json);
            if (replay == null)
            {
                throw new ArgumentException("Replay JSON could not be parsed.", nameof(json));
            }

            if (replay.events == null)
            {
                replay.events = new List<GameEvent>();
            }

            return replay;
        }
    }
}
