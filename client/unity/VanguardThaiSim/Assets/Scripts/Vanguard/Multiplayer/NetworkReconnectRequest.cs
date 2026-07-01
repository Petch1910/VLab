using System;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkReconnectRequest
    {
        public int protocol_version;
        public string room_id;
        public string player_id;
        public int from_event_index;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkReconnectRequest FromJson(string json)
        {
            NetworkReconnectRequest request = JsonUtility.FromJson<NetworkReconnectRequest>(json);
            if (request == null)
            {
                throw new ArgumentException("Reconnect request JSON could not be parsed.", nameof(json));
            }

            return request;
        }
    }
}
