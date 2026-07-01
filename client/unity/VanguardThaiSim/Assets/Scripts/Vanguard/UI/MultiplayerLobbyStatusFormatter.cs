using System;
using System.Text;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class MultiplayerLobbyStatusFormatter
    {
        public const string TrustedClientModeLine =
            "Mode: trusted-client friend room. Not ranked secure.";

        public static string FormatConnectionStatus(
            string transportName,
            MultiplayerTransportStatus status,
            bool appIdConfigured,
            string lastMessage)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Transport: ");
            builder.Append(string.IsNullOrWhiteSpace(transportName) ? "Photon" : transportName.Trim());
            builder.Append(" / ");
            builder.Append(status);
            builder.Append('\n');
            builder.Append(OnlineRoomTransportPolicyFormatter.Format());
            builder.Append('\n');
            builder.Append(appIdConfigured
                ? "Photon setup: AppId configured."
                : "Photon setup: AppId missing. Add local config or env var.");
            builder.Append('\n');
            builder.Append(string.IsNullOrWhiteSpace(lastMessage) ? "Ready." : Trim(lastMessage, 130));
            return builder.ToString();
        }

        public static string FormatDeckPackStatus(VanguardDeck deck, CardPackManifest manifest)
        {
            StringBuilder builder = new StringBuilder();
            if (deck == null)
            {
                builder.Append("Deck: no deck selected.");
            }
            else
            {
                builder.Append("Deck: ");
                builder.Append(string.IsNullOrWhiteSpace(deck.name) ? "Untitled Deck" : deck.name.Trim());
                builder.Append('\n');
                builder.Append("Main ");
                builder.Append(deck.TotalCards(DeckZone.Main));
                builder.Append(" / Ride ");
                builder.Append(deck.TotalCards(DeckZone.Ride));
                builder.Append(" / G ");
                builder.Append(deck.TotalCards(DeckZone.G));
            }

            builder.Append('\n');
            builder.Append("Pack: ");
            if (manifest == null)
            {
                builder.Append("not loaded");
            }
            else
            {
                builder.Append(string.IsNullOrWhiteSpace(manifest.pack_id) ? "unknown" : manifest.pack_id.Trim());
                builder.Append(" / ");
                builder.Append(string.IsNullOrWhiteSpace(manifest.source_version) ? "unknown version" : manifest.source_version.Trim());
            }

            return builder.ToString();
        }

        public static string FormatQuickDeckSelector(
            VanguardDeck deck,
            int savedDeckCount,
            int choiceIndex,
            bool roomActive)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Quick Deck: ");
            if (deck == null)
            {
                builder.Append("no deck selected");
            }
            else
            {
                builder.Append(string.IsNullOrWhiteSpace(deck.name) ? "Untitled Deck" : deck.name.Trim());
                builder.Append(" | Main ");
                builder.Append(deck.TotalCards(DeckZone.Main));
                builder.Append(" / Ride ");
                builder.Append(deck.TotalCards(DeckZone.Ride));
                builder.Append(" / G ");
                builder.Append(deck.TotalCards(DeckZone.G));
            }

            builder.Append('\n');
            int safeSavedCount = Math.Max(0, savedDeckCount);
            builder.Append("Choices: session deck + ");
            builder.Append(safeSavedCount);
            builder.Append(" saved");
            if (safeSavedCount > 0 && choiceIndex > 0)
            {
                builder.Append(" | saved ");
                builder.Append(Math.Min(choiceIndex, safeSavedCount));
                builder.Append("/");
                builder.Append(safeSavedCount);
            }

            if (roomActive)
            {
                builder.Append("\nLocked: Leave Room before changing the online deck.");
            }

            return builder.ToString();
        }

        public static string FormatQuickEditStatus(
            VanguardDeck deck,
            string message,
            bool roomActive)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Quick Edit: ");
            if (deck == null)
            {
                builder.Append("no active deck");
            }
            else
            {
                builder.Append(string.IsNullOrWhiteSpace(deck.name) ? "Untitled Deck" : deck.name.Trim());
                builder.Append(" | Main ");
                builder.Append(deck.TotalCards(DeckZone.Main));
                builder.Append(" / Ride ");
                builder.Append(deck.TotalCards(DeckZone.Ride));
                builder.Append(" / G ");
                builder.Append(deck.TotalCards(DeckZone.G));
            }

            builder.Append('\n');
            builder.Append(roomActive
                ? "Locked: Leave Room before editing the online deck."
                : "Paste a deck code to replace the active lobby deck for this session.");

            if (!string.IsNullOrWhiteSpace(message))
            {
                builder.Append('\n');
                builder.Append(Trim(message, 120));
            }

            return builder.ToString();
        }

        public static string FormatRoomSummary(MultiplayerRoomState room)
        {
            if (room == null)
            {
                return "Room: none\nConnect, then host or join a friend room.";
            }

            room.EnsureLists();
            StringBuilder builder = new StringBuilder();
            builder.Append("Room: ");
            builder.Append(string.IsNullOrWhiteSpace(room.room_id) ? "unknown" : room.room_id.Trim());
            builder.Append(" / ");
            builder.Append(string.IsNullOrWhiteSpace(room.state) ? "unknown" : room.state.Trim());
            builder.Append(" / players ");
            builder.Append(room.players.Count);
            builder.Append("/2");

            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player == null)
                {
                    continue;
                }

                builder.Append('\n');
                builder.Append(player.connected ? "- " : "- offline ");
                builder.Append(string.IsNullOrWhiteSpace(player.display_name) ? player.player_id : player.display_name.Trim());
                builder.Append(" | cursor ");
                builder.Append(player.event_cursor);
                builder.Append(" | deck ");
                builder.Append(player.main_deck_count);
                builder.Append("/");
                builder.Append(player.ride_deck_count);
                builder.Append("/");
                builder.Append(player.g_deck_count);
                builder.Append(player.ready ? " | ready" : " | not ready");
                builder.Append(string.IsNullOrWhiteSpace(player.deck_hash) ? " | deck hash missing" : " | deck hash ready");
            }

            return builder.ToString();
        }

        public static string FormatRoomStatus(
            MultiplayerTransportStatus status,
            MultiplayerRoomState room,
            PackSyncInfo localPack)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Room Status:\n");
            builder.Append("Connection: ");
            builder.Append(status);
            if (room == null)
            {
                builder.Append("\nPlayers: 0/2 connected");
                builder.Append("\nDeck hash: 0/0 ready");
                builder.Append("\nPack hash: no room");
                builder.Append("\nPublic cursor: 0");
                return builder.ToString();
            }

            room.EnsureLists();
            int connected = 0;
            int deckHashReady = 0;
            int publicCursor = 0;
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player == null)
                {
                    continue;
                }

                publicCursor = System.Math.Max(publicCursor, player.event_cursor);
                if (!player.connected)
                {
                    continue;
                }

                connected++;
                if (!string.IsNullOrWhiteSpace(player.deck_hash))
                {
                    deckHashReady++;
                }
            }

            builder.Append("\nPlayers: ");
            builder.Append(connected);
            builder.Append("/2 connected");
            builder.Append("\nDeck hash: ");
            builder.Append(deckHashReady);
            builder.Append("/");
            builder.Append(connected);
            builder.Append(" ready");
            builder.Append("\nPack hash: ");
            builder.Append(FormatPackHashMatch(room.pack, localPack));
            builder.Append("\nPublic cursor: ");
            builder.Append(publicCursor);

            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player == null)
                {
                    continue;
                }

                builder.Append('\n');
                builder.Append(player.connected ? "- " : "- offline ");
                builder.Append(string.IsNullOrWhiteSpace(player.display_name) ? player.player_id : player.display_name.Trim());
                builder.Append(player.ready ? " ready" : " not ready");
                builder.Append(string.IsNullOrWhiteSpace(player.deck_hash) ? " / deck hash missing" : " / deck hash ready");
                builder.Append(" / cursor ");
                builder.Append(player.event_cursor);
            }

            return builder.ToString();
        }

        public static string FormatFlowSummary(
            MultiplayerTransportStatus status,
            MultiplayerRoomState room,
            RoomPlayerInfo localPlayer,
            DeckPrivacyGameplayDecision decision)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Flow:\n");
            builder.Append("1 Connect: ");
            builder.Append(status);
            builder.Append('\n');

            if (room == null)
            {
                builder.Append("2 Room: host or join a friend room\n");
                builder.Append("3 Ready: waiting for room\n");
                builder.Append("4 Start: unavailable\n");
                builder.Append("5 Rematch: available after ended games");
                return builder.ToString();
            }

            room.EnsureLists();
            int connected = 0;
            int ready = 0;
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player == null || !player.connected)
                {
                    continue;
                }

                connected++;
                if (player.ready)
                {
                    ready++;
                }
            }

            builder.Append("2 Room: ");
            builder.Append(string.IsNullOrWhiteSpace(room.room_id) ? "unknown" : room.room_id.Trim());
            builder.Append(" / players ");
            builder.Append(connected);
            builder.Append("/2\n");

            builder.Append("3 Ready: ");
            builder.Append(localPlayer != null && localPlayer.ready ? "you ready" : "you not ready");
            builder.Append(" / players ready ");
            builder.Append(ready);
            builder.Append("/");
            builder.Append(connected);
            builder.Append('\n');

            builder.Append("4 Start: ");
            if (string.Equals(room.state ?? "", RoomLifecycleStates.Playing, System.StringComparison.Ordinal))
            {
                builder.Append("room already started");
            }
            else if (string.Equals(room.state ?? "", RoomLifecycleStates.Ended, System.StringComparison.Ordinal))
            {
                builder.Append("room ended");
            }
            else if (decision != null && !decision.can_start_gameplay)
            {
                builder.Append(Trim(decision.rejection_reason, 70));
            }
            else if (connected >= 2 && ready == connected && connected > 0)
            {
                builder.Append("Start Table available");
            }
            else
            {
                builder.Append("wait until both players are ready");
            }

            builder.Append('\n');
            builder.Append("5 Rematch: ");
            builder.Append(string.Equals(room.state ?? "", RoomLifecycleStates.Ended, System.StringComparison.Ordinal)
                ? "available"
                : "available after ended games");

            return builder.ToString();
        }

        public static string FormatNavigationLockout(MultiplayerRoomState room)
        {
            if (room == null)
            {
                return "Navigation: Back Home is available before joining a room.";
            }

            return "Navigation: Back Home is locked while this Photon room is active. Use Leave Room first.";
        }

        public static string FormatReconnectSummary(NetworkReconnectRequest request, NetworkEventBatch batch)
        {
            return FormatReconnectSummary(request, batch, null, -1, null);
        }

        public static string FormatReconnectSummary(
            NetworkReconnectRequest request,
            NetworkEventBatch batch,
            MultiplayerRoomState room,
            int localCursor,
            string lastMessage)
        {
            if (request == null && batch == null)
            {
                return "Reconnect: no pending request or batch.\n" +
                    "Enter room id + cursor, then Reconnect to resume.\n" +
                    "Use cursor 0 for a fresh table.";
            }

            StringBuilder builder = new StringBuilder();
            if (request != null)
            {
                builder.Append("Request: ");
                builder.Append(string.IsNullOrWhiteSpace(request.player_id) ? "unknown player" : request.player_id.Trim());
                builder.Append(" from event ");
                builder.Append(request.from_event_index);
                builder.Append("\nReason: player is asking for events after that cursor.");
                if (batch == null)
                {
                    builder.Append("\nWaiting: peer should send a reconnect batch.");
                }
            }

            if (batch != null)
            {
                batch.EnsureLists();
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append("Batch: from event ");
                builder.Append(batch.from_event_index);
                builder.Append(" / ");
                builder.Append(batch.events.Count);
                builder.Append(" events");
                builder.Append('\n');
                builder.Append(FormatReconnectBatchAction(batch, room, localCursor));
            }

            if (!string.IsNullOrWhiteSpace(lastMessage) &&
                lastMessage.IndexOf("RECONNECT", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                builder.Append('\n');
                builder.Append("Last reconnect issue: ");
                builder.Append(Trim(FormatStartTableRejection(lastMessage), 120));
            }

            return builder.ToString();
        }

        public static string FormatStartTableRejection(string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                return "Cannot start table: unknown reason.";
            }

            string reason = rejectionReason.Trim();
            if (reason.IndexOf("RECONNECT_BATCH_CURSOR_MISMATCH", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Cannot start table: reconnect batch cursor does not match the local table cursor. Rejoin with a matching cursor or start a fresh room with cursor 0.";
            }

            if (reason.IndexOf("RECONNECT_BATCH_ROOM_MISMATCH", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Cannot start table: reconnect batch belongs to a different room. Rejoin the correct room and request a new batch.";
            }

            if (reason.IndexOf("RECONNECT_BATCH_APPLY_FAILED", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Cannot start table: reconnect batch could not replay cleanly. Request a fresh batch or restart the room.";
            }

            if (reason.IndexOf("PLAYERS_NOT_READY", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Cannot start table: wait until both connected players are ready.";
            }

            return "Cannot start table: " + reason + ".";
        }

        public static string FormatTrustSummary(MultiplayerRoomState room, DeckPrivacyGameplayDecision decision)
        {
            if (room == null)
            {
                return "Safety: no room joined.";
            }

            if (decision == null)
            {
                return TrustedClientModeLine + "\nSafety: room privacy not evaluated yet.";
            }

            if (decision.requires_client_trust_warning)
            {
                return TrustedClientModeLine + "\nTrust warning required: " + Trim(decision.warning_message, 160);
            }

            if (decision.can_start_gameplay)
            {
                return TrustedClientModeLine + "\nStart gate: ready for shared friend room.";
            }

            if (decision.client_trust_warning_acknowledged)
            {
                return TrustedClientModeLine + "\nTrust acknowledged. Start blocked: " +
                    Trim(decision.rejection_reason, 120);
            }

            return TrustedClientModeLine + "\nStart blocked: " + Trim(decision.rejection_reason, 120);
        }

        public static string FormatRevealSummary(
            NetworkDeckRevealRequest request,
            NetworkDeckRevealResponse response,
            bool responseAccepted,
            string responseMessage)
        {
            StringBuilder builder = new StringBuilder();
            if (request == null)
            {
                builder.Append("Reveal request: none.");
            }
            else
            {
                builder.Append("Reveal request: ");
                builder.Append(string.IsNullOrWhiteSpace(request.requester_player_id)
                    ? "unknown"
                    : request.requester_player_id.Trim());
                builder.Append(" -> ");
                builder.Append(string.IsNullOrWhiteSpace(request.target_player_id)
                    ? "unknown"
                    : request.target_player_id.Trim());
            }

            builder.Append('\n');
            if (response == null)
            {
                builder.Append("Reveal response: none.");
            }
            else
            {
                builder.Append("Reveal response: ");
                builder.Append(string.IsNullOrWhiteSpace(response.player_id) ? "unknown" : response.player_id.Trim());
                builder.Append(responseAccepted ? " verified. " : " failed. ");
                builder.Append(Trim(responseMessage, 120));
            }

            return builder.ToString();
        }

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string compact = string.Join(" ", value.Trim().Split(
                new[] { ' ', '\r', '\n', '\t' },
                System.StringSplitOptions.RemoveEmptyEntries));
            if (compact.Length <= maxLength)
            {
                return compact;
            }

            return compact.Substring(0, maxLength - 3) + "...";
        }

        private static string FormatPackHashMatch(PackSyncInfo roomPack, PackSyncInfo localPack)
        {
            if (roomPack == null || localPack == null)
            {
                return "missing";
            }

            return MultiplayerProtocol.PackMatches(roomPack, localPack, false)
                ? "match"
                : "mismatch";
        }

        private static string FormatReconnectBatchAction(NetworkEventBatch batch, MultiplayerRoomState room, int localCursor)
        {
            if (room == null && localCursor < 0)
            {
                return batch.from_event_index == 0
                    ? "Handoff: ready to apply when Start Table opens."
                    : "Handoff: needs matching local state at event " + batch.from_event_index + " before Start Table.";
            }

            if (room == null)
            {
                return "Handoff blocked: no room state yet. Rejoin the room before applying this batch.";
            }

            if (!string.IsNullOrWhiteSpace(batch.room_id) &&
                !string.Equals(room.room_id ?? "", batch.room_id ?? "", System.StringComparison.Ordinal))
            {
                return "Handoff blocked: batch is for room " + batch.room_id + ", current room is " +
                    (string.IsNullOrWhiteSpace(room.room_id) ? "unknown" : room.room_id.Trim()) + ".";
            }

            if (batch.from_event_index == localCursor)
            {
                return "Handoff: batch matches local cursor " + localCursor + " and is ready to apply.";
            }

            if (batch.from_event_index < localCursor)
            {
                return "Handoff blocked: batch starts at event " + batch.from_event_index +
                    " but local cursor is already " + localCursor + ". Request a new batch from " + localCursor + ".";
            }

            return "Handoff blocked: batch starts at event " + batch.from_event_index +
                " but local cursor is " + localCursor + ". Applying it would skip events.";
        }
    }
}
