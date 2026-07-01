using System;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkCommandEnvelope
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string command_id;
        public string room_id;
        public string room_game_id;
        public string player_id;
        public int player_index = -1;
        public int sequence = -1;
        public int state_cursor = -1;
        public string sent_at_utc;
        public LegalGameAction action;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkCommandEnvelope FromJson(string json)
        {
            NetworkCommandEnvelope envelope = JsonUtility.FromJson<NetworkCommandEnvelope>(json);
            if (envelope == null)
            {
                throw new ArgumentException("Network command envelope JSON could not be parsed.", nameof(json));
            }

            if (envelope.protocol_version == 0)
            {
                envelope.protocol_version = MultiplayerProtocol.ProtocolVersion;
            }

            return envelope;
        }
    }
}
