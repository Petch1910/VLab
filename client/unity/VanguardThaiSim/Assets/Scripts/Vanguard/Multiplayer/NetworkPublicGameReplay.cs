using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPublicGameReplay
    {
        public string replay_id;
        public string initial_state_json;
        public string perspective = GameStateViewPerspective.Spectator.ToString();
        public int viewer_player_index = -1;
        public List<NetworkPublicGameEvent> events = new List<NetworkPublicGameEvent>();

        public static NetworkPublicGameReplay Create(
            GameState initialState,
            IReadOnlyList<NetworkPublicGameEvent> publicEvents,
            GameStateViewPerspective replayPerspective = GameStateViewPerspective.Spectator,
            int viewerPlayerIndex = -1)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            GameState initialView = GameStateViewFactory.CreateView(initialState, replayPerspective, viewerPlayerIndex);
            initialView.event_log.Clear();

            NetworkPublicGameReplay replay = new NetworkPublicGameReplay
            {
                replay_id = Guid.NewGuid().ToString("N"),
                initial_state_json = initialView.ToJson(false),
                perspective = replayPerspective.ToString(),
                viewer_player_index = viewerPlayerIndex
            };

            if (publicEvents != null)
            {
                for (int i = 0; i < publicEvents.Count; i++)
                {
                    replay.events.Add(CloneEvent(publicEvents[i]));
                }
            }

            return replay;
        }

        public static NetworkPublicGameReplay CreateFromTrueEvents(
            GameState initialState,
            IReadOnlyList<GameEvent> trueEvents,
            GameStateViewPerspective replayPerspective = GameStateViewPerspective.Spectator,
            int viewerPlayerIndex = -1)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            GameState cursor = GameState.FromJson(initialState.ToJson(false));
            cursor.event_log.Clear();
            List<NetworkPublicGameEvent> publicEvents = new List<NetworkPublicGameEvent>();
            if (trueEvents != null)
            {
                foreach (GameEvent gameEvent in trueEvents)
                {
                    publicEvents.Add(NetworkPublicGameEventFactory.Create(cursor, gameEvent));
                    GameEventReducer.Apply(cursor, gameEvent, true);
                }
            }

            return Create(initialState, publicEvents, replayPerspective, viewerPlayerIndex);
        }

        public GameState CreateInitialStateView()
        {
            if (string.IsNullOrWhiteSpace(initial_state_json))
            {
                throw new InvalidOperationException("Public replay does not contain an initial state view.");
            }

            GameState state = GameState.FromJson(initial_state_json);
            state.event_log.Clear();
            return state;
        }

        public string ToJson(bool prettyPrint = false)
        {
            if (events == null)
            {
                events = new List<NetworkPublicGameEvent>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkPublicGameReplay FromJson(string json)
        {
            NetworkPublicGameReplay replay = JsonUtility.FromJson<NetworkPublicGameReplay>(json);
            if (replay == null)
            {
                throw new ArgumentException("Public replay JSON could not be parsed.", nameof(json));
            }

            if (replay.events == null)
            {
                replay.events = new List<NetworkPublicGameEvent>();
            }

            if (string.IsNullOrWhiteSpace(replay.perspective))
            {
                replay.perspective = GameStateViewPerspective.Spectator.ToString();
            }

            return replay;
        }

        internal static NetworkPublicGameEvent CloneEvent(NetworkPublicGameEvent publicEvent)
        {
            return publicEvent == null
                ? null
                : NetworkPublicGameEvent.FromJson(publicEvent.ToJson(false));
        }
    }
}
