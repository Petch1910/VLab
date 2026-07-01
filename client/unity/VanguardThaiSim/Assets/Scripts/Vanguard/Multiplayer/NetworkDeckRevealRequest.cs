using System;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkDeckRevealRequest
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string room_id;
        public string requester_player_id;
        public string target_player_id;
        public string requested_at_utc;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkDeckRevealRequest FromJson(string json)
        {
            NetworkDeckRevealRequest request = JsonUtility.FromJson<NetworkDeckRevealRequest>(json);
            if (request == null)
            {
                throw new ArgumentException("Deck reveal request JSON could not be parsed.", nameof(json));
            }

            return request;
        }
    }
}
