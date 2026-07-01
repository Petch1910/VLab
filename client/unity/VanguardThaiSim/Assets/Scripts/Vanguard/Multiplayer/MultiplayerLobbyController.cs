using System;
using System.Security.Cryptography;
using System.Text;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public sealed class MultiplayerLobbyController
    {
        private readonly IMultiplayerTransport transport;
        private readonly PackSyncInfo localPack;
        private bool reconnectRequestSent;
        private bool clientTrustWarningAcknowledged;

        public MultiplayerLobbyController(IMultiplayerTransport transport, PackSyncInfo localPack)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.localPack = localPack ?? throw new ArgumentNullException(nameof(localPack));
            this.transport.RoomStateReceived += HandleRoomStateReceived;
            this.transport.ReconnectRequestReceived += HandleReconnectRequestReceived;
            this.transport.ReconnectBatchReceived += HandleReconnectBatchReceived;
            this.transport.DeckRevealRequestReceived += HandleDeckRevealRequestReceived;
            this.transport.DeckRevealResponseReceived += HandleDeckRevealResponseReceived;
        }

        public MultiplayerRoomState CurrentRoom { get; private set; }

        public RoomPlayerInfo LocalPlayer { get; private set; }

        public string LastMessage { get; private set; }

        public NetworkReconnectRequest LastReconnectRequest { get; private set; }

        public NetworkEventBatch LastReconnectBatch { get; private set; }

        public NetworkDeckRevealRequest LastDeckRevealRequest { get; private set; }

        public NetworkDeckRevealResponse LastDeckRevealResponse { get; private set; }

        public bool LastDeckRevealAccepted { get; private set; }

        public string LastDeckRevealMessage { get; private set; }

        public DeckPrivacyGameplayDecision LastDeckPrivacyGameplayDecision { get; private set; }

        public bool ClientTrustWarningAcknowledged
        {
            get { return clientTrustWarningAcknowledged; }
        }

        public MultiplayerTransportStatus Status
        {
            get { return transport.Status; }
        }

        public string TransportName
        {
            get { return transport.TransportName; }
        }

        public MultiplayerTransportResult Connect()
        {
            MultiplayerTransportResult result = transport.Connect();
            LastMessage = result.ok ? "Connecting to " + TransportName + "." : FormatFailure(result);
            return result;
        }

        public MultiplayerTransportResult CreateRoom(string roomId, string displayName, VanguardDeck deck, int randomSeed = 0)
        {
            string normalizedRoomId = NormalizeRoomId(roomId);
            LocalPlayer = CreateLocalPlayer(displayName, deck, 0, null);
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                normalizedRoomId,
                deck == null ? "D" : deck.format,
                LocalPlayer.player_id,
                randomSeed == 0 ? Environment.TickCount : randomSeed,
                localPack);
            room.players.Add(LocalPlayer);
            CurrentRoom = room;
            ResetReconnectState();
            ResetClientTrustWarningAcknowledgement();

            MultiplayerTransportResult result = transport.CreateRoom(room, LocalPlayer);
            LastMessage = result.ok ? "Creating room " + normalizedRoomId + "." : FormatFailure(result);
            return result;
        }

        public MultiplayerTransportResult JoinRoom(string roomId, string displayName, VanguardDeck deck, int eventCursor = 0)
        {
            string normalizedRoomId = NormalizeRoomId(roomId);
            LocalPlayer = CreateLocalPlayer(displayName, deck, eventCursor, null);
            CurrentRoom = null;
            ResetReconnectState();
            ResetClientTrustWarningAcknowledgement();
            MultiplayerTransportResult result = transport.JoinRoom(normalizedRoomId, LocalPlayer, localPack);
            LastMessage = result.ok ? "Joining room " + normalizedRoomId + "." : FormatFailure(result);
            return result;
        }

        public MultiplayerTransportResult ReconnectRoom(string roomId, string displayName, VanguardDeck deck, int eventCursor)
        {
            return JoinRoom(roomId, displayName, deck, Math.Max(0, eventCursor));
        }

        public MultiplayerTransportResult RequestReconnectBatch(int fromEventIndex)
        {
            if (CurrentRoom == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_MISSING", "Join a room before requesting reconnect events.");
                LastMessage = FormatFailure(result);
                return result;
            }

            if (LocalPlayer == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("PLAYER_MISSING", "Local player is required before requesting reconnect events.");
                LastMessage = FormatFailure(result);
                return result;
            }

            NetworkReconnectRequest request = new NetworkReconnectRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = CurrentRoom.room_id,
                player_id = LocalPlayer.player_id,
                from_event_index = Math.Max(0, fromEventIndex)
            };

            MultiplayerTransportResult sendResult = transport.SendReconnectRequest(request);
            if (sendResult.ok)
            {
                reconnectRequestSent = true;
                LastMessage = "Requested reconnect batch from event " + request.from_event_index + ".";
            }
            else
            {
                LastMessage = FormatFailure(sendResult);
            }

            return sendResult;
        }

        public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
        {
            if (batch == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("RECONNECT_BATCH_MISSING", "Reconnect batch is required.");
                LastMessage = FormatFailure(result);
                return result;
            }

            MultiplayerTransportResult sendResult = transport.SendReconnectBatch(batch);
            LastMessage = sendResult.ok
                ? "Sent reconnect batch from " + batch.from_event_index + " with " + (batch.events == null ? 0 : batch.events.Count) + " events."
                : FormatFailure(sendResult);
            return sendResult;
        }

        public MultiplayerTransportResult RequestDeckReveal(string targetPlayerId)
        {
            if (CurrentRoom == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_MISSING", "Join a room before requesting a deck reveal.");
                LastMessage = FormatFailure(result);
                return result;
            }

            if (LocalPlayer == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("PLAYER_MISSING", "Local player is required before requesting a deck reveal.");
                LastMessage = FormatFailure(result);
                return result;
            }

            NetworkDeckRevealRequest request = new NetworkDeckRevealRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = CurrentRoom.room_id,
                requester_player_id = LocalPlayer.player_id,
                target_player_id = targetPlayerId,
                requested_at_utc = DateTime.UtcNow.ToString("O")
            };

            MultiplayerTransportResult sendResult = transport.SendDeckRevealRequest(request);
            LastMessage = sendResult.ok
                ? "Requested deck reveal from " + targetPlayerId + "."
                : FormatFailure(sendResult);
            return sendResult;
        }

        public MultiplayerTransportResult SendDeckRevealResponse(VanguardDeck deck, string revealNonce)
        {
            if (CurrentRoom == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_MISSING", "Join a room before sending a deck reveal.");
                LastMessage = FormatFailure(result);
                return result;
            }

            if (LocalPlayer == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("PLAYER_MISSING", "Local player is required before sending a deck reveal.");
                LastMessage = FormatFailure(result);
                return result;
            }

            if (deck == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("DECK_MISSING", "A deck is required before sending a deck reveal.");
                LastMessage = FormatFailure(result);
                return result;
            }

            NetworkDeckRevealResponse response = new NetworkDeckRevealResponse
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = CurrentRoom.room_id,
                player_id = LocalPlayer.player_id,
                revealed_deck_code = DeckCodeCodec.Export(deck),
                deck_reveal_nonce = revealNonce,
                deck_commitment = LocalPlayer.deck_commitment,
                deck_commitment_algorithm = LocalPlayer.deck_commitment_algorithm,
                pack_definition_hash = localPack.definition_hash,
                revealed_at_utc = DateTime.UtcNow.ToString("O")
            };

            MultiplayerTransportResult sendResult = transport.SendDeckRevealResponse(response);
            LastMessage = sendResult.ok
                ? "Sent deck reveal response."
                : FormatFailure(sendResult);
            return sendResult;
        }

        public void Tick()
        {
            transport.Tick();
        }

        public void Disconnect()
        {
            transport.Disconnect();
            CurrentRoom = null;
            ResetReconnectState();
            ResetClientTrustWarningAcknowledgement();
            LastMessage = "Disconnected.";
        }

        public DeckPrivacyGameplayDecision EvaluateDeckPrivacyForGameplay()
        {
            LastDeckPrivacyGameplayDecision = DeckPrivacyGameplayPolicy.Evaluate(
                CurrentRoom,
                clientTrustWarningAcknowledged);
            return LastDeckPrivacyGameplayDecision;
        }

        public void AcknowledgeClientTrustWarning()
        {
            clientTrustWarningAcknowledged = true;
            LastMessage = "Trusted-client warning acknowledged.";
            EvaluateDeckPrivacyForGameplay();
        }

        public MultiplayerTransportResult SetLocalReady(bool ready)
        {
            if (CurrentRoom == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_MISSING", "Join a room before changing ready state.");
                LastMessage = FormatFailure(result);
                return result;
            }

            if (LocalPlayer == null)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("PLAYER_MISSING", "Local player is required before changing ready state.");
                LastMessage = FormatFailure(result);
                return result;
            }

            RoomLifecycleTransitionResult transition = RoomLifecycleController.SetPlayerReady(
                CurrentRoom,
                LocalPlayer.player_id,
                ready);
            if (!transition.accepted)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_READY_REJECTED", transition.rejection_reason);
                LastMessage = FormatFailure(result);
                return result;
            }

            MultiplayerTransportResult sendResult = transport.SendRoomState(transition.room);
            if (!sendResult.ok)
            {
                LastMessage = FormatFailure(sendResult);
                return sendResult;
            }

            CurrentRoom = transition.room;
            LastMessage = ready ? "Ready sent." : "Ready cleared.";
            return sendResult;
        }

        public MultiplayerTransportResult TryStartRoom()
        {
            if (CurrentRoom != null &&
                string.Equals(CurrentRoom.state ?? "", RoomLifecycleStates.Playing, StringComparison.Ordinal))
            {
                MultiplayerTransportResult alreadyPlaying = MultiplayerTransportResult.Ok();
                LastMessage = "Room already started.";
                return alreadyPlaying;
            }

            RoomLifecycleTransitionResult transition = RoomLifecycleController.Start(CurrentRoom);
            if (!transition.accepted)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_START_REJECTED", transition.rejection_reason);
                LastMessage = FormatFailure(result);
                return result;
            }

            MultiplayerTransportResult sendResult = transport.SendRoomState(transition.room);
            if (!sendResult.ok)
            {
                LastMessage = FormatFailure(sendResult);
                return sendResult;
            }

            CurrentRoom = transition.room;
            LastMessage = "Room started.";
            return sendResult;
        }

        public MultiplayerTransportResult TryRematchRoom()
        {
            RoomLifecycleTransitionResult transition = RoomLifecycleController.Rematch(CurrentRoom);
            if (!transition.accepted)
            {
                MultiplayerTransportResult result = MultiplayerTransportResult.Fail("ROOM_REMATCH_REJECTED", transition.rejection_reason);
                LastMessage = FormatFailure(result);
                return result;
            }

            MultiplayerTransportResult sendResult = transport.SendRoomState(transition.room);
            if (!sendResult.ok)
            {
                LastMessage = FormatFailure(sendResult);
                return sendResult;
            }

            CurrentRoom = transition.room;
            ResetReconnectState();
            ResetClientTrustWarningAcknowledgement();
            LastMessage = "Rematch room reset. Players need to ready again.";
            return sendResult;
        }

        public void ResetClientTrustWarningAcknowledgement()
        {
            clientTrustWarningAcknowledged = false;
            LastDeckPrivacyGameplayDecision = null;
        }

        public string GetRoomSummary()
        {
            if (CurrentRoom == null)
            {
                return "No room joined.";
            }

            CurrentRoom.EnsureLists();
            StringBuilder builder = new StringBuilder();
            builder.Append("Room ");
            builder.Append(CurrentRoom.room_id);
            builder.Append(" | ");
            builder.Append(CurrentRoom.state);
            builder.Append(" | players ");
            builder.Append(CurrentRoom.players.Count);
            builder.Append("/2\n");
            for (int i = 0; i < CurrentRoom.players.Count; i++)
            {
                RoomPlayerInfo player = CurrentRoom.players[i];
                if (player == null)
                {
                    continue;
                }

                builder.Append(player.connected ? "- " : "- offline ");
                builder.Append(string.IsNullOrWhiteSpace(player.display_name) ? player.player_id : player.display_name);
                builder.Append(" cursor ");
                builder.Append(player.event_cursor);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public bool TryCreateInitialGameState(out GameState initialState, out string rejectionReason)
        {
            initialState = null;
            rejectionReason = null;
            if (CurrentRoom == null)
            {
                rejectionReason = "ROOM_MISSING";
                return false;
            }

            CurrentRoom.EnsureLists();
            MultiplayerProtocolValidationResult validation = MultiplayerProtocol.ValidateRoomReady(CurrentRoom, localPack);
            if (!validation.accepted)
            {
                rejectionReason = validation.FirstIssue;
                return false;
            }

            DeckPrivacyGameplayDecision privacyDecision = EvaluateDeckPrivacyForGameplay();
            if (!privacyDecision.can_start_gameplay)
            {
                rejectionReason = privacyDecision.rejection_reason;
                return false;
            }

            RoomPlayerInfo host = FindPlayer(CurrentRoom, CurrentRoom.host_player_id);
            RoomPlayerInfo guest = FindFirstOtherConnectedPlayer(CurrentRoom, CurrentRoom.host_player_id);
            if (host == null || guest == null)
            {
                rejectionReason = "PLAYERS_NOT_READY";
                return false;
            }

            if (CurrentRoom.random_seed == 0)
            {
                rejectionReason = "RANDOM_SEED_MISSING";
                return false;
            }

            VanguardDeck hostDeck;
            if (!TryImportDeckCode(host.deck_code, out hostDeck, out rejectionReason))
            {
                rejectionReason = "HOST_" + rejectionReason;
                return false;
            }

            if (!TryValidateDeckHash(host, hostDeck, "HOST", out rejectionReason))
            {
                return false;
            }

            VanguardDeck guestDeck;
            if (!TryImportDeckCode(guest.deck_code, out guestDeck, out rejectionReason))
            {
                rejectionReason = "GUEST_" + rejectionReason;
                return false;
            }

            if (!TryValidateDeckHash(guest, guestDeck, "GUEST", out rejectionReason))
            {
                return false;
            }

            initialState = GameStateFactory.CreateTwoPlayerGame(
                hostDeck,
                guestDeck,
                CurrentRoom.random_seed);
            initialState.game_id = CreateDeterministicGameId(CurrentRoom);
            return true;
        }

        public bool TryCreateGameSession(out MultiplayerGameSessionController session, out string rejectionReason)
        {
            session = null;
            GameState initialState;
            if (!TryCreateInitialGameState(out initialState, out rejectionReason))
            {
                return false;
            }

            session = CreateGameSession(initialState);
            if (LastReconnectBatch == null)
            {
                LastMessage = "Created game session at event " + session.EventCursor + ".";
                return true;
            }

            LastReconnectBatch.EnsureLists();
            if (LastReconnectBatch.from_event_index != session.EventCursor)
            {
                rejectionReason = "RECONNECT_BATCH_CURSOR_MISMATCH: batch starts at " +
                    LastReconnectBatch.from_event_index + " but session is at " + session.EventCursor;
                LastMessage = rejectionReason;
                session = null;
                return false;
            }

            int applied = session.ApplyReconnectBatch(LastReconnectBatch);
            if (applied != LastReconnectBatch.events.Count)
            {
                rejectionReason = "RECONNECT_BATCH_APPLY_FAILED: " + session.LastMessage;
                LastMessage = rejectionReason;
                session = null;
                return false;
            }

            LastMessage = "Created game session at event " + session.EventCursor + ".";
            return true;
        }

        public MultiplayerGameSessionController CreateGameSession(GameState initialState)
        {
            if (CurrentRoom == null)
            {
                throw new InvalidOperationException("Join a room before creating a game session.");
            }

            if (LocalPlayer == null)
            {
                throw new InvalidOperationException("Local player is required before creating a game session.");
            }

            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            return new MultiplayerGameSessionController(transport, CurrentRoom, initialState, LocalPlayer.player_id);
        }

        private void HandleRoomStateReceived(MultiplayerRoomState room)
        {
            if (room == null)
            {
                return;
            }

            if (!MultiplayerProtocol.PackMatches(room.pack, localPack, false))
            {
                LastMessage = "PACK_HASH_MISMATCH: room uses a different card pack.";
                return;
            }

            room.EnsureLists();
            string previousRoomId = CurrentRoom == null ? "" : CurrentRoom.room_id;
            CurrentRoom = room;
            if (!string.Equals(previousRoomId ?? "", CurrentRoom.room_id ?? "", StringComparison.Ordinal))
            {
                ResetReconnectState();
                ResetClientTrustWarningAcknowledgement();
            }

            bool changed = EnsureLocalPlayerInRoom();
            LastMessage = "Room " + CurrentRoom.room_id + " ready. Players " + CurrentRoom.players.Count + "/2.";
            if (changed)
            {
                MultiplayerTransportResult result = transport.SendRoomState(CurrentRoom);
                if (!result.ok)
                {
                    LastMessage = FormatFailure(result);
                }
            }

            if (LocalPlayer != null && LocalPlayer.event_cursor > 0 && !reconnectRequestSent)
            {
                RequestReconnectBatch(LocalPlayer.event_cursor);
            }
        }

        private void HandleReconnectRequestReceived(NetworkReconnectRequest request)
        {
            if (request == null)
            {
                return;
            }

            LastReconnectRequest = request;
            LastMessage = "Reconnect request from " + request.player_id + " at event " + request.from_event_index + ".";
        }

        private void HandleReconnectBatchReceived(NetworkEventBatch batch)
        {
            if (batch == null)
            {
                return;
            }

            if (CurrentRoom == null)
            {
                LastMessage = "RECONNECT_BATCH_ROOM_MISSING: ignored batch before room state.";
                return;
            }

            if (!string.Equals(CurrentRoom.room_id ?? "", batch.room_id ?? "", StringComparison.Ordinal))
            {
                LastMessage = "RECONNECT_BATCH_ROOM_MISMATCH: ignored batch for " + batch.room_id + ".";
                return;
            }

            batch.EnsureLists();
            LastReconnectBatch = batch;
            LastMessage = "Received reconnect batch from " + batch.from_event_index + " with " + batch.events.Count + " events.";
        }

        private void HandleDeckRevealRequestReceived(NetworkDeckRevealRequest request)
        {
            if (request == null)
            {
                return;
            }

            LastDeckRevealRequest = request;
            LastMessage = "Deck reveal requested by " + request.requester_player_id + ".";
        }

        private void HandleDeckRevealResponseReceived(NetworkDeckRevealResponse response)
        {
            if (response == null)
            {
                return;
            }

            LastDeckRevealResponse = response;
            LastDeckRevealAccepted = false;
            if (CurrentRoom == null)
            {
                LastDeckRevealMessage = "ROOM_MISSING";
                LastMessage = LastDeckRevealMessage;
                return;
            }

            RoomPlayerInfo player = FindPlayer(CurrentRoom, response.player_id);
            VanguardDeck revealedDeck;
            string rejectionReason;
            LastDeckRevealAccepted = DeckRevealResponseVerifier.TryVerify(
                player,
                response,
                CurrentRoom.room_id,
                localPack.definition_hash,
                out revealedDeck,
                out rejectionReason);
            LastDeckRevealMessage = LastDeckRevealAccepted
                ? "Deck reveal verified for " + response.player_id + "."
                : rejectionReason;
            LastMessage = LastDeckRevealMessage;
        }

        private bool EnsureLocalPlayerInRoom()
        {
            if (CurrentRoom == null || LocalPlayer == null)
            {
                return false;
            }

            RoomPlayerInfo existing = FindPlayer(CurrentRoom, LocalPlayer.player_id);
            if (existing == null)
            {
                CurrentRoom.players.Add(LocalPlayer);
                return true;
            }

            bool changed = false;
            changed |= AssignIfDifferent(ref existing.display_name, LocalPlayer.display_name);
            changed |= AssignIfDifferent(ref existing.deck_id, LocalPlayer.deck_id);
            changed |= AssignIfDifferent(ref existing.deck_hash, LocalPlayer.deck_hash);
            changed |= AssignIfDifferent(ref existing.deck_code, LocalPlayer.deck_code);
            if (!existing.connected)
            {
                existing.connected = true;
                changed = true;
            }

            return changed;
        }

        private static bool AssignIfDifferent(ref string target, string value)
        {
            if (string.Equals(target ?? "", value ?? "", StringComparison.Ordinal))
            {
                return false;
            }

            target = value;
            return true;
        }

        private void ResetReconnectState()
        {
            reconnectRequestSent = false;
            LastReconnectRequest = null;
            LastReconnectBatch = null;
        }

        private static RoomPlayerInfo FindPlayer(MultiplayerRoomState room, string playerId)
        {
            if (room == null || string.IsNullOrWhiteSpace(playerId))
            {
                return null;
            }

            room.EnsureLists();
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player != null && string.Equals(player.player_id, playerId, StringComparison.Ordinal))
                {
                    return player;
                }
            }

            return null;
        }

        private static RoomPlayerInfo CreateLocalPlayer(string displayName, VanguardDeck deck, int eventCursor, string playerId)
        {
            string safeName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim();
            return new RoomPlayerInfo
            {
                player_id = string.IsNullOrWhiteSpace(playerId) ? "player-" + Guid.NewGuid().ToString("N").Substring(0, 8) : playerId,
                display_name = safeName,
                deck_id = deck == null ? "no-deck" : deck.deck_id,
                deck_hash = CreateDeckHash(deck),
                deck_code = CreateDeckCode(deck),
                main_deck_count = deck == null ? 0 : deck.TotalCards(DeckZone.Main),
                ride_deck_count = deck == null ? 0 : deck.TotalCards(DeckZone.Ride),
                g_deck_count = deck == null ? 0 : deck.TotalCards(DeckZone.G),
                opening_hand_count = deck == null ? 0 : GameStateFactory.OpeningHandSize,
                connected = true,
                event_cursor = Math.Max(0, eventCursor)
            };
        }

        private static string CreateDeckHash(VanguardDeck deck)
        {
            if (deck == null)
            {
                return "no-deck";
            }

            return DeckCommitmentService.ComputeDeckHash(deck);
        }

        private static string NormalizeRoomId(string roomId)
        {
            if (!string.IsNullOrWhiteSpace(roomId))
            {
                return roomId.Trim().ToUpperInvariant();
            }

            return "VGTH-" + DateTime.UtcNow.ToString("HHmmss");
        }

        private static RoomPlayerInfo FindFirstOtherConnectedPlayer(MultiplayerRoomState room, string hostPlayerId)
        {
            if (room == null)
            {
                return null;
            }

            room.EnsureLists();
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player != null &&
                    player.connected &&
                    !string.Equals(player.player_id ?? "", hostPlayerId ?? "", StringComparison.Ordinal))
                {
                    return player;
                }
            }

            return null;
        }

        private static bool TryImportDeckCode(string deckCode, out VanguardDeck deck, out string rejectionReason)
        {
            deck = null;
            rejectionReason = null;
            if (string.IsNullOrWhiteSpace(deckCode))
            {
                rejectionReason = "DECK_CODE_MISSING";
                return false;
            }

            try
            {
                deck = DeckCodeCodec.Import(deckCode);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "DECK_CODE_INVALID: " + exception.Message;
                return false;
            }
        }

        private static bool TryValidateDeckHash(
            RoomPlayerInfo player,
            VanguardDeck deck,
            string prefix,
            out string rejectionReason)
        {
            rejectionReason = null;
            if (player == null)
            {
                rejectionReason = prefix + "_PLAYER_MISSING";
                return false;
            }

            if (deck == null)
            {
                rejectionReason = prefix + "_DECK_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.deck_hash))
            {
                rejectionReason = prefix + "_DECK_HASH_MISSING";
                return false;
            }

            string actualHash = CreateDeckHash(deck);
            if (!string.Equals(actualHash, player.deck_hash ?? "", StringComparison.OrdinalIgnoreCase))
            {
                rejectionReason = prefix + "_DECK_HASH_MISMATCH";
                return false;
            }

            return true;
        }

        private static string CreateDeckCode(VanguardDeck deck)
        {
            if (deck == null)
            {
                return "";
            }

            return DeckCodeCodec.Export(deck);
        }

        private static string CreateDeterministicGameId(MultiplayerRoomState room)
        {
            string source = (room.room_id ?? "") + "|" + room.random_seed + "|" + (room.host_player_id ?? "");
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(source);
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder("room-game-");
                for (int i = 0; i < 8 && i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static string FormatFailure(MultiplayerTransportResult result)
        {
            if (result == null)
            {
                return "TRANSPORT_ERROR: no result returned.";
            }

            return result.error_code + ": " + result.message;
        }
    }
}
