using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class LocalOwnerPrivateSession
    {
        public string room_id;
        public string local_player_id;
        public int local_player_index = -1;
        public int event_cursor;
        public bool reconnect_enabled;
        public string reconnect_block_reason;
        public GameState local_true_state;
        public GameState opponent_public_view;
        public List<NetworkPublicGameEvent> public_event_log = new List<NetworkPublicGameEvent>();
        public List<GameEvent> local_private_event_log = new List<GameEvent>();

        public void EnsureLists()
        {
            if (public_event_log == null)
            {
                public_event_log = new List<NetworkPublicGameEvent>();
            }

            if (local_private_event_log == null)
            {
                local_private_event_log = new List<GameEvent>();
            }

            local_true_state?.EnsureLists();
            opponent_public_view?.EnsureLists();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static LocalOwnerPrivateSession FromJson(string json)
        {
            LocalOwnerPrivateSession session = JsonUtility.FromJson<LocalOwnerPrivateSession>(json);
            if (session == null)
            {
                throw new ArgumentException("Owner-private session JSON could not be parsed.", nameof(json));
            }

            session.EnsureLists();
            return session;
        }
    }
}
