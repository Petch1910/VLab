using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPublicEventBatch
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string room_id;
        public int from_event_index;
        public List<NetworkPublicGameEvent> events = new List<NetworkPublicGameEvent>();

        public void EnsureLists()
        {
            if (events == null)
            {
                events = new List<NetworkPublicGameEvent>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkPublicEventBatch FromJson(string json)
        {
            NetworkPublicEventBatch batch = JsonUtility.FromJson<NetworkPublicEventBatch>(json);
            if (batch == null)
            {
                throw new ArgumentException("Network public event batch JSON could not be parsed.", nameof(json));
            }

            batch.EnsureLists();
            if (batch.protocol_version == 0)
            {
                batch.protocol_version = MultiplayerProtocol.ProtocolVersion;
            }

            return batch;
        }
    }
}
