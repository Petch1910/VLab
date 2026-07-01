using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkEventBatch
    {
        public int protocol_version;
        public string room_id;
        public int from_event_index;
        public List<NetworkEventEnvelope> events = new List<NetworkEventEnvelope>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkEventBatch FromJson(string json)
        {
            NetworkEventBatch batch = JsonUtility.FromJson<NetworkEventBatch>(json);
            if (batch == null)
            {
                throw new ArgumentException("Network event batch JSON could not be parsed.", nameof(json));
            }

            batch.EnsureLists();
            return batch;
        }

        public void EnsureLists()
        {
            if (events == null)
            {
                events = new List<NetworkEventEnvelope>();
            }
        }
    }
}
