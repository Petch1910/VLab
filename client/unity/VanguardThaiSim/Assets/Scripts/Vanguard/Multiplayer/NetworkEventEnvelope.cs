using System;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkEventEnvelope
    {
        public int protocol_version;
        public string room_id;
        public string game_id;
        public string player_id;
        public int event_index;
        public string previous_event_id;
        public string sent_at_utc;
        public GameEvent game_event;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkEventEnvelope FromJson(string json)
        {
            NetworkEventEnvelope envelope = JsonUtility.FromJson<NetworkEventEnvelope>(json);
            if (envelope == null)
            {
                throw new ArgumentException("Network event JSON could not be parsed.", nameof(json));
            }

            return envelope;
        }
    }
}
