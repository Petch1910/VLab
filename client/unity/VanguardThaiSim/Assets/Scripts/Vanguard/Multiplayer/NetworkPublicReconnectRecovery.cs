using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Multiplayer
{
    public static class NetworkPublicReconnectRecovery
    {
        public const string CommitmentTrueReconnectBlocked = "COMMITMENT_TRUE_RECONNECT_BLOCKED";

        public static bool RequiresPublicReconnect(MultiplayerRoomState room)
        {
            if (room == null)
            {
                return false;
            }

            string mode = string.IsNullOrWhiteSpace(room.deck_privacy_mode)
                ? DeckPrivacyModes.SharedDeckCode
                : room.deck_privacy_mode.Trim();
            return !string.Equals(mode, DeckPrivacyModes.SharedDeckCode, StringComparison.Ordinal);
        }

        public static NetworkPublicEventBatch CreateBatch(
            MultiplayerRoomState room,
            IReadOnlyList<NetworkPublicGameEvent> publicEvents,
            int fromEventIndex)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            int start = Math.Max(0, fromEventIndex);
            NetworkPublicEventBatch batch = new NetworkPublicEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = room.room_id,
                from_event_index = start
            };

            if (publicEvents != null)
            {
                for (int i = start; i < publicEvents.Count; i++)
                {
                    NetworkPublicGameEvent publicEvent = publicEvents[i];
                    if (publicEvent != null)
                    {
                        batch.events.Add(NetworkPublicGameEvent.FromJson(publicEvent.ToJson(false)));
                    }
                }
            }

            return batch;
        }

        public static NetworkPublicReconnectApplyResult ApplyBatch(
            LocalOwnerPrivateSession session,
            NetworkPublicEventBatch batch)
        {
            if (session == null)
            {
                return NetworkPublicReconnectApplyResult.Rejected("SESSION_MISSING");
            }

            if (batch == null)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PUBLIC_EVENT_BATCH_MISSING");
            }

            session.EnsureLists();
            batch.EnsureLists();
            if (batch.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PROTOCOL_VERSION_MISMATCH");
            }

            if (!string.Equals(session.room_id ?? "", batch.room_id ?? "", StringComparison.Ordinal))
            {
                return NetworkPublicReconnectApplyResult.Rejected("ROOM_ID_MISMATCH");
            }

            if (batch.from_event_index != session.event_cursor)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PUBLIC_RECONNECT_CURSOR_MISMATCH");
            }

            int applied = 0;
            for (int i = 0; i < batch.events.Count; i++)
            {
                NetworkPublicGameEventApplyResult result =
                    NetworkPublicGameEventApplier.ApplyToSession(session, batch.events[i]);
                if (!result.accepted)
                {
                    return NetworkPublicReconnectApplyResult.Rejected(result.rejection_reason);
                }

                applied++;
            }

            return NetworkPublicReconnectApplyResult.Accepted(applied);
        }
    }
}
