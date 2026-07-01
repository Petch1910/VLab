using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class MultiplayerLobbyTests
    {
        [Test]
        public void LobbyControllerCreatesRoomWithLocalPlayer()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck deck = CreateSampleDeck("p1");

            MultiplayerTransportResult result = controller.CreateRoom("room1", "Host", deck, 1234);

            Assert.IsTrue(result.ok, result.message);
            Assert.NotNull(transport.createdRoom);
            Assert.AreEqual("ROOM1", transport.createdRoom.room_id);
            Assert.AreEqual(1, transport.createdRoom.players.Count);
            Assert.AreEqual("Host", transport.createdRoom.players[0].display_name);
            Assert.AreEqual(deck.deck_id, transport.createdRoom.players[0].deck_id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(transport.createdRoom.players[0].deck_hash));
            Assert.IsFalse(string.IsNullOrWhiteSpace(transport.createdRoom.players[0].deck_code));
            Assert.AreSame(controller.CurrentRoom, transport.createdRoom);
        }

        [Test]
        public void LobbyControllerAddsGuestToReceivedRoomAndPublishesState()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck deck = CreateSampleDeck("guest");
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM2", "D", "host", 4321, Pack());
            room.players.Add(Player("host", "Host"));

            controller.JoinRoom("room2", "Guest", deck);
            transport.EmitRoom(room);

            Assert.NotNull(controller.CurrentRoom);
            Assert.AreEqual(2, controller.CurrentRoom.players.Count);
            Assert.NotNull(transport.sentRoom);
            Assert.AreEqual(2, transport.sentRoom.players.Count);
            Assert.AreEqual("Guest", transport.sentRoom.players[1].display_name);
            Assert.IsFalse(string.IsNullOrWhiteSpace(transport.sentRoom.players[1].deck_code));
        }

        [Test]
        public void LobbyControllerRejectsReceivedRoomPackMismatch()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack("hash-a"));
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM3", "D", "host", 9001, Pack("hash-b"));

            controller.JoinRoom("room3", "Guest", CreateSampleDeck("guest"));
            transport.EmitRoom(room);

            Assert.IsNull(controller.CurrentRoom);
            Assert.IsNull(transport.sentRoom);
            Assert.IsTrue(controller.LastMessage.Contains("PACK_HASH_MISMATCH"));
        }

        [Test]
        public void LobbyControllerReconnectSendsCursorRequestAfterRoomState()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM4", "D", "host", 1234, Pack());
            room.players.Add(Player("host", "Host"));

            controller.ReconnectRoom("room4", "Guest", CreateSampleDeck("guest"), 7);
            transport.EmitRoom(room);

            Assert.NotNull(transport.sentReconnectRequest);
            Assert.AreEqual("ROOM4", transport.sentReconnectRequest.room_id);
            Assert.AreEqual(7, transport.sentReconnectRequest.from_event_index);
        }

        [Test]
        public void LobbyControllerClearsStaleReconnectStateWhenJoiningOrDisconnecting()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM-OLD", "D", "host", 1234, Pack());
            room.players.Add(Player("host", "Host"));

            controller.JoinRoom("room-old", "Guest", CreateSampleDeck("guest"));
            transport.EmitRoom(room);
            transport.EmitReconnectRequest(new NetworkReconnectRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-OLD",
                player_id = "guest",
                from_event_index = 1
            });
            transport.EmitReconnectBatch(new NetworkEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-OLD",
                from_event_index = 0
            });

            Assert.NotNull(controller.LastReconnectRequest);
            Assert.NotNull(controller.LastReconnectBatch);

            controller.JoinRoom("room-new", "Guest", CreateSampleDeck("guest2"));

            Assert.IsNull(controller.LastReconnectRequest);
            Assert.IsNull(controller.LastReconnectBatch);

            transport.EmitReconnectBatch(new NetworkEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-OLD",
                from_event_index = 0
            });

            Assert.IsNull(controller.LastReconnectBatch);
            Assert.IsTrue(controller.LastMessage.Contains("RECONNECT_BATCH_ROOM_MISSING"));

            controller.Disconnect();

            Assert.IsNull(controller.LastReconnectRequest);
            Assert.IsNull(controller.LastReconnectBatch);
        }

        [Test]
        public void LobbyControllerRequestsAndVerifiesDeckRevealResponse()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck revealedDeck = CreateSampleDeck("target");
            string nonce = "nonce-target";
            string commitment = DeckCommitmentService.CreateCommitment(revealedDeck, nonce, "ROOM-REVEAL", Pack().definition_hash);
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM-REVEAL", "D", "target", 1234, Pack());
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "target",
                display_name = "Target",
                deck_commitment = commitment,
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                connected = true
            });

            controller.JoinRoom("room-reveal", "Requester", CreateSampleDeck("requester"));
            transport.EmitRoom(room);
            controller.RequestDeckReveal("target");
            transport.EmitDeckRevealResponse(new NetworkDeckRevealResponse
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-REVEAL",
                player_id = "target",
                revealed_deck_code = DeckCodeCodec.Export(revealedDeck),
                deck_reveal_nonce = nonce,
                deck_commitment = commitment,
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                pack_definition_hash = Pack().definition_hash
            });

            Assert.NotNull(transport.sentDeckRevealRequest);
            Assert.AreEqual("target", transport.sentDeckRevealRequest.target_player_id);
            Assert.NotNull(controller.LastDeckRevealResponse);
            Assert.IsTrue(controller.LastDeckRevealAccepted, controller.LastDeckRevealMessage);
            Assert.IsTrue(controller.LastDeckRevealMessage.Contains("verified"));
        }

        [Test]
        public void LobbyControllerSendsLocalDeckRevealResponse()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck localDeck = CreateSampleDeck("local");
            string nonce = "nonce-local";
            string commitment = DeckCommitmentService.CreateCommitment(localDeck, nonce, "ROOM-LOCAL", Pack().definition_hash);
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM-LOCAL", "D", "host", 2222, Pack());

            controller.JoinRoom("room-local", "Local", localDeck);
            transport.EmitRoom(room);
            controller.LocalPlayer.deck_commitment = commitment;
            controller.LocalPlayer.deck_commitment_algorithm = DeckCommitmentService.Algorithm;

            MultiplayerTransportResult result = controller.SendDeckRevealResponse(localDeck, nonce);

            Assert.IsTrue(result.ok, result.message);
            Assert.NotNull(transport.sentDeckRevealResponse);
            Assert.AreEqual(controller.LocalPlayer.player_id, transport.sentDeckRevealResponse.player_id);
            Assert.AreEqual(commitment, transport.sentDeckRevealResponse.deck_commitment);
            Assert.AreEqual(nonce, transport.sentDeckRevealResponse.deck_reveal_nonce);
            Assert.IsFalse(string.IsNullOrWhiteSpace(transport.sentDeckRevealResponse.revealed_deck_code));
        }

        [Test]
        public void LobbyControllerCreatesInitialStateFromSharedDeckCodes()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            controller.CreateRoom("start", "Host", hostDeck, 5555);
            transport.createdRoom.players.Add(new RoomPlayerInfo
            {
                player_id = "guest-player",
                display_name = "Guest",
                deck_id = guestDeck.deck_id,
                deck_hash = DeckCommitmentService.ComputeDeckHash(guestDeck),
                deck_code = DeckCodeCodec.Export(guestDeck),
                connected = true
            });

            GameState initialState;
            string rejectionReason;
            bool created = controller.TryCreateInitialGameState(out initialState, out rejectionReason);

            Assert.IsTrue(created, rejectionReason);
            Assert.NotNull(initialState);
            Assert.AreEqual(45, initialState.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(45, initialState.GetPlayer(1).CountZone(GameZone.Deck));
            Assert.IsTrue(initialState.GetPlayer(0).deck[0].card_id.StartsWith("host-MAIN-"));
            Assert.IsTrue(initialState.GetPlayer(1).deck[0].card_id.StartsWith("guest-MAIN-"));
        }

        [Test]
        public void LobbyControllerPublishesReadyStateWithoutMutatingOnReject()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck deck = CreateSampleDeck("host");

            controller.CreateRoom("ready", "Host", deck, 5555);
            MultiplayerTransportResult result = controller.SetLocalReady(true);

            Assert.IsTrue(result.ok, result.message);
            Assert.NotNull(transport.sentRoom);
            Assert.IsTrue(controller.CurrentRoom.players[0].ready);
            Assert.IsTrue(transport.sentRoom.players[0].ready);

            controller.CurrentRoom.state = RoomLifecycleStates.Playing;
            string before = controller.CurrentRoom.ToJson(false);
            MultiplayerTransportResult rejected = controller.SetLocalReady(false);

            Assert.IsFalse(rejected.ok);
            Assert.AreEqual(before, controller.CurrentRoom.ToJson(false));
        }

        [Test]
        public void LobbyControllerStartRejectsUntilBothPlayersReady()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            controller.CreateRoom("start-ready", "Host", hostDeck, 5657);
            transport.createdRoom.players.Add(new RoomPlayerInfo
            {
                player_id = "guest-player",
                display_name = "Guest",
                deck_id = guestDeck.deck_id,
                deck_hash = DeckCommitmentService.ComputeDeckHash(guestDeck),
                deck_code = DeckCodeCodec.Export(guestDeck),
                connected = true
            });

            MultiplayerTransportResult rejected = controller.TryStartRoom();

            Assert.IsFalse(rejected.ok);
            Assert.IsTrue(rejected.message.Contains("PLAYERS_NOT_READY"));
            Assert.AreEqual(RoomLifecycleStates.Waiting, controller.CurrentRoom.state);
        }

        [Test]
        public void LobbyControllerStartPublishesPlayingStateWhenReady()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            controller.CreateRoom("play", "Host", hostDeck, 5757);
            transport.createdRoom.players.Add(new RoomPlayerInfo
            {
                player_id = "guest-player",
                display_name = "Guest",
                deck_id = guestDeck.deck_id,
                deck_hash = DeckCommitmentService.ComputeDeckHash(guestDeck),
                deck_code = DeckCodeCodec.Export(guestDeck),
                connected = true,
                ready = true
            });
            controller.SetLocalReady(true);

            MultiplayerTransportResult result = controller.TryStartRoom();

            Assert.IsTrue(result.ok, result.message);
            Assert.AreEqual(RoomLifecycleStates.Playing, controller.CurrentRoom.state);
            Assert.AreEqual(RoomLifecycleStates.Playing, transport.sentRoom.state);
        }

        [Test]
        public void LobbyControllerRematchPublishesWaitingStateFromEndedRoom()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            controller.CreateRoom("again", "Host", hostDeck, 5858);
            transport.createdRoom.players.Add(new RoomPlayerInfo
            {
                player_id = "guest-player",
                display_name = "Guest",
                deck_id = guestDeck.deck_id,
                deck_hash = DeckCommitmentService.ComputeDeckHash(guestDeck),
                deck_code = DeckCodeCodec.Export(guestDeck),
                connected = true,
                ready = true,
                event_cursor = 4
            });
            controller.CurrentRoom.players[0].ready = true;
            controller.CurrentRoom.players[0].event_cursor = 3;
            controller.CurrentRoom.state = RoomLifecycleStates.Ended;

            MultiplayerTransportResult result = controller.TryRematchRoom();

            Assert.IsTrue(result.ok, result.message);
            Assert.AreEqual(RoomLifecycleStates.Waiting, controller.CurrentRoom.state);
            Assert.IsFalse(controller.CurrentRoom.players[0].ready);
            Assert.IsFalse(controller.CurrentRoom.players[1].ready);
            Assert.AreEqual(0, controller.CurrentRoom.players[0].event_cursor);
            Assert.AreEqual(0, controller.CurrentRoom.players[1].event_cursor);
            Assert.AreEqual(RoomLifecycleStates.Waiting, transport.sentRoom.state);
        }

        [Test]
        public void LobbyControllerRejectsSharedDeckHashMismatchOnStart()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            controller.CreateRoom("hashguard", "Host", hostDeck, 5657);
            transport.createdRoom.players.Add(new RoomPlayerInfo
            {
                player_id = "guest-player",
                display_name = "Guest",
                deck_id = guestDeck.deck_id,
                deck_hash = "wrong-hash",
                deck_code = DeckCodeCodec.Export(guestDeck),
                connected = true
            });

            GameState initialState;
            string rejectionReason;
            bool created = controller.TryCreateInitialGameState(out initialState, out rejectionReason);

            Assert.IsFalse(created);
            Assert.IsNull(initialState);
            Assert.AreEqual("GUEST_DECK_HASH_MISMATCH", rejectionReason);
        }

        [Test]
        public void LobbyControllerRejectsPublicSharedDeckCodeStart()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            controller.CreateRoom("public", "Host", hostDeck, 5656);
            transport.createdRoom.room_visibility = RoomVisibilityModes.Public;
            transport.createdRoom.deck_privacy_mode = DeckPrivacyModes.SharedDeckCode;
            transport.createdRoom.players.Add(new RoomPlayerInfo
            {
                player_id = "guest-player",
                display_name = "Guest",
                deck_id = guestDeck.deck_id,
                deck_hash = "guest-hash",
                deck_code = DeckCodeCodec.Export(guestDeck),
                connected = true
            });

            GameState initialState;
            string rejectionReason;
            bool created = controller.TryCreateInitialGameState(out initialState, out rejectionReason);

            Assert.IsFalse(created);
            Assert.IsNull(initialState);
            Assert.AreEqual("DECK_PRIVACY_SHARED_CODE_NOT_ALLOWED", rejectionReason);
        }

        [Test]
        public void LobbyControllerRejectsServerHeldPrivacyModeWithoutValidator()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck guestDeck = CreateSampleDeck("guest");
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM-PRIVATE", "D", "host-player", 5757, Pack());
            room.deck_privacy_mode = DeckPrivacyModes.ServerHeldDeck;
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "host-player",
                display_name = "Host",
                deck_id = "host-deck",
                deck_hash = "host-hash",
                connected = true
            });

            controller.JoinRoom("room-private", "Guest", guestDeck);
            transport.EmitRoom(room);

            GameState initialState;
            string rejectionReason;
            bool created = controller.TryCreateInitialGameState(out initialState, out rejectionReason);

            Assert.IsFalse(created);
            Assert.IsNull(initialState);
            Assert.AreEqual(DeckPrivacyGameplayPolicy.ServerHeldValidatorRequired, rejectionReason);
        }

        [Test]
        public void LobbyControllerRejectsCommitmentGameplayUntilClientTrustWarningAcknowledged()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck guestDeck = CreateSampleDeck("guest");
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM-COMMIT", "D", "host-player", 5858, Pack());
            room.room_visibility = RoomVisibilityModes.Public;
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.players.Add(CommittedPlayer("host-player", "Host"));

            controller.JoinRoom("room-commit", "Guest", guestDeck);
            transport.EmitRoom(room);
            RoomPlayerInfo guest = controller.CurrentRoom.players[1];
            guest.deck_code = "";
            guest.deck_commitment = "guest-player-commitment";
            guest.deck_commitment_algorithm = DeckCommitmentService.Algorithm;
            guest.deck_reveal_policy = "end_of_match";

            GameState initialState;
            string rejectionReason;
            bool created = controller.TryCreateInitialGameState(out initialState, out rejectionReason);
            DeckPrivacyGameplayDecision policy = DeckPrivacyGameplayPolicy.Evaluate(room);

            Assert.IsFalse(created);
            Assert.IsNull(initialState);
            Assert.AreEqual(DeckPrivacyGameplayPolicy.CommitmentClientTrustPolicyRequired, rejectionReason);
            Assert.IsTrue(policy.requires_client_trust_warning);
        }

        [Test]
        public void LobbyControllerKeepsCommitmentGameplayBlockedAfterTrustWarningAcknowledged()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck guestDeck = CreateSampleDeck("guest");
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM-COMMIT-ACK", "D", "host-player", 5959, Pack());
            room.room_visibility = RoomVisibilityModes.Public;
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.players.Add(CommittedPlayer("host-player", "Host"));

            controller.JoinRoom("room-commit-ack", "Guest", guestDeck);
            transport.EmitRoom(room);
            RoomPlayerInfo guest = controller.CurrentRoom.players[1];
            guest.deck_code = "";
            guest.deck_commitment = "guest-player-commitment";
            guest.deck_commitment_algorithm = DeckCommitmentService.Algorithm;
            guest.deck_reveal_policy = "end_of_match";

            controller.AcknowledgeClientTrustWarning();
            GameState initialState;
            string rejectionReason;
            bool created = controller.TryCreateInitialGameState(out initialState, out rejectionReason);
            DeckPrivacyGameplayDecision policy = DeckPrivacyGameplayPolicy.Evaluate(room, true);

            Assert.IsTrue(controller.ClientTrustWarningAcknowledged);
            Assert.IsFalse(created);
            Assert.IsNull(initialState);
            Assert.AreEqual(DeckPrivacyGameplayPolicy.OwnerPrivateGameplayPathIncomplete, rejectionReason);
            Assert.IsFalse(policy.requires_client_trust_warning);
            Assert.IsTrue(policy.client_trust_warning_acknowledged);
        }

        [Test]
        public void LobbyControllerCreatesGuestGameSessionWithPlayerIndexOne()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyController controller = new MultiplayerLobbyController(transport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom("ROOM6", "D", "host-player", 6666, Pack());
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "host-player",
                display_name = "Host",
                deck_id = hostDeck.deck_id,
                deck_hash = DeckCommitmentService.ComputeDeckHash(hostDeck),
                deck_code = DeckCodeCodec.Export(hostDeck),
                connected = true
            });

            controller.JoinRoom("room6", "Guest", guestDeck);
            transport.EmitRoom(room);

            GameState initialState;
            string rejectionReason;
            Assert.IsTrue(controller.TryCreateInitialGameState(out initialState, out rejectionReason), rejectionReason);

            MultiplayerGameSessionController session = controller.CreateGameSession(initialState);

            Assert.AreEqual(1, session.LocalPlayerIndex);
        }

        [Test]
        public void LobbyInitialStatesUseDeterministicGameIdForRoomSync()
        {
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerLobbyController hostController = new MultiplayerLobbyController(hostTransport, Pack());
            MultiplayerLobbyController guestController = new MultiplayerLobbyController(guestTransport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            hostController.CreateRoom("sync", "Host", hostDeck, 7777);
            guestController.JoinRoom("sync", "Guest", guestDeck);
            guestTransport.EmitRoom(hostTransport.createdRoom);

            GameState hostInitial;
            GameState guestInitial;
            string rejectionReason;
            Assert.IsTrue(hostController.TryCreateInitialGameState(out hostInitial, out rejectionReason), rejectionReason);
            Assert.IsTrue(guestController.TryCreateInitialGameState(out guestInitial, out rejectionReason), rejectionReason);

            MultiplayerGameSessionController hostSession = hostController.CreateGameSession(hostInitial);
            MultiplayerGameSessionController guestSession = guestController.CreateGameSession(guestInitial);
            RulesCommandResult result = hostSession.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostSession.State, 0), GameActionType.Draw));
            guestTransport.EmitEvent(hostTransport.sentEvents[0]);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(hostInitial.game_id, guestInitial.game_id);
            Assert.AreEqual(1, guestSession.EventCursor);
            Assert.AreEqual(hostSession.State.GetPlayer(0).CountZone(GameZone.Deck), guestSession.State.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(hostSession.State.GetPlayer(0).CountZone(GameZone.Hand), guestSession.State.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void LobbyControllerCreatesSessionWithPendingReconnectBatch()
        {
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerLobbyController hostController = new MultiplayerLobbyController(hostTransport, Pack());
            MultiplayerLobbyController guestController = new MultiplayerLobbyController(guestTransport, Pack());
            VanguardDeck hostDeck = CreateSampleDeck("host");
            VanguardDeck guestDeck = CreateSampleDeck("guest");

            hostController.CreateRoom("resume", "Host", hostDeck, 8888);
            guestController.JoinRoom("resume", "Guest", guestDeck);
            guestTransport.EmitRoom(hostTransport.createdRoom);

            MultiplayerGameSessionController hostSession;
            MultiplayerGameSessionController guestSession;
            string rejectionReason;
            Assert.IsTrue(hostController.TryCreateGameSession(out hostSession, out rejectionReason), rejectionReason);
            RulesCommandResult result = hostSession.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostSession.State, 0), GameActionType.Draw));
            Assert.IsTrue(result.accepted, result.rejection_reason);

            NetworkEventBatch batch = hostSession.CreateReconnectBatch(0);
            guestTransport.EmitReconnectBatch(batch);

            Assert.IsTrue(guestController.TryCreateGameSession(out guestSession, out rejectionReason), rejectionReason);
            Assert.AreEqual(1, guestSession.EventCursor);
            Assert.AreEqual(1, guestSession.LastReconnectAppliedCount);
            Assert.AreEqual(0, guestSession.LastReconnectFromEventIndex);
            Assert.AreEqual(hostSession.State.GetPlayer(0).CountZone(GameZone.Deck), guestSession.State.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(hostSession.State.GetPlayer(0).CountZone(GameZone.Hand), guestSession.State.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void PhotonConfigLoaderReadsLocalProjectConfig()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "vgth-photon-config-" + Guid.NewGuid().ToString("N"));
            string settingsDir = Path.Combine(tempRoot, "ProjectSettings");
            Directory.CreateDirectory(settingsDir);
            File.WriteAllText(
                Path.Combine(settingsDir, "VanguardPhotonLocal.json"),
                "{\"app_id\":\"test-local-app\",\"fixed_region\":\"asia\",\"app_version\":\"test-version\"}");

            try
            {
                PhotonRealtimeTransportConfig config = PhotonRealtimeConfigLoader.LoadFrom(tempRoot, null, null, null);

                Assert.AreEqual("test-local-app", config.app_id);
                Assert.AreEqual("asia", config.fixed_region);
                Assert.AreEqual("test-version", config.app_version);
            }
            finally
            {
                Directory.Delete(tempRoot, true);
            }
        }

        [Test]
        public void MultiplayerLobbyBootstrapCreatesRuntimeUiWithInjectedTransport()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyBootstrap.Show(CreateSampleDeck("p1"), Manifest(), transport);

            MultiplayerLobbyBootstrap lobby = UnityEngine.Object.FindAnyObjectByType<MultiplayerLobbyBootstrap>();

            Assert.NotNull(lobby);
            Assert.NotNull(lobby.Controller);
            Assert.NotNull(GameObject.Find("Connection Panel"));
            Assert.NotNull(GameObject.Find("Room Panel"));
            Assert.NotNull(GameObject.Find("Safety Reveal Panel"));
            Assert.NotNull(GameObject.Find("Back Home Button"));
            Assert.NotNull(GameObject.Find("Leave Room Button"));
            Assert.NotNull(GameObject.Find("Quick Deck Summary Text"));
            Assert.NotNull(GameObject.Find("Prev Deck Button"));
            Assert.NotNull(GameObject.Find("Next Deck Button"));
            Assert.NotNull(GameObject.Find("Quick Edit Button"));
            Assert.NotNull(GameObject.Find("Start Table Button"));
            Assert.NotNull(GameObject.Find("Ready Button"));
            Assert.NotNull(GameObject.Find("Not Ready Button"));
            Assert.NotNull(GameObject.Find("Rematch Button"));
            Assert.NotNull(GameObject.Find("Reconnect Button"));
            GameObject reconnectSummary = GameObject.Find("Reconnect Summary Text");
            Assert.NotNull(reconnectSummary);
            Assert.GreaterOrEqual(reconnectSummary.GetComponent<LayoutElement>().preferredHeight, 84f);
            Assert.NotNull(GameObject.Find("Reveal target Input"));
            Assert.NotNull(GameObject.Find("Reveal nonce Input"));
            Assert.NotNull(GameObject.Find("Acknowledge Trust Button"));
            Assert.NotNull(GameObject.Find("Request Reveal Button"));
            Assert.NotNull(GameObject.Find("Send Reveal Button"));

            UnityEngine.Object.DestroyImmediate(lobby.gameObject);
        }

        [Test]
        public void MultiplayerLobbyQuickDeckSelectorAppliesBeforeRoomAndLocksInsideRoom()
        {
            FakeTransport transport = new FakeTransport();
            VanguardDeck sessionDeck = CreateSampleDeck("session");
            VanguardDeck savedDeck = CreateSampleDeck("saved");
            MultiplayerLobbyBootstrap.Show(
                sessionDeck,
                Manifest(),
                transport,
                new List<VanguardDeck> { savedDeck });

            MultiplayerLobbyBootstrap lobby = UnityEngine.Object.FindAnyObjectByType<MultiplayerLobbyBootstrap>();
            Button nextDeckButton = GameObject.Find("Next Deck Button").GetComponent<Button>();
            Button quickEditButton = GameObject.Find("Quick Edit Button").GetComponent<Button>();
            Button connectButton = GameObject.Find("Connect Button").GetComponent<Button>();
            Button hostButton = GameObject.Find("Host Button").GetComponent<Button>();

            Assert.NotNull(lobby);
            Assert.IsTrue(nextDeckButton.interactable);
            Assert.IsTrue(quickEditButton.interactable);

            nextDeckButton.onClick.Invoke();
            connectButton.onClick.Invoke();
            hostButton.onClick.Invoke();

            Assert.NotNull(lobby.Controller.CurrentRoom);
            Assert.AreEqual(savedDeck.deck_id, lobby.Controller.LocalPlayer.deck_id);
            Assert.IsFalse(nextDeckButton.interactable);
            Assert.IsFalse(quickEditButton.interactable);

            string roomBefore = lobby.Controller.CurrentRoom.ToJson(false);
            nextDeckButton.onClick.Invoke();
            quickEditButton.onClick.Invoke();

            Assert.AreEqual(roomBefore, lobby.Controller.CurrentRoom.ToJson(false));

            UnityEngine.Object.DestroyImmediate(lobby.gameObject);
        }

        [Test]
        public void MultiplayerLobbyQuickEditDeckCodeAppliesBeforeRoomAndLocksInsideRoom()
        {
            FakeTransport transport = new FakeTransport();
            VanguardDeck sessionDeck = CreateSampleDeck("session");
            VanguardDeck importedDeck = CreateSampleDeck("imported");
            VanguardDeck blockedDeck = CreateSampleDeck("blocked");
            MultiplayerLobbyBootstrap.Show(sessionDeck, Manifest(), transport);

            MultiplayerLobbyBootstrap lobby = UnityEngine.Object.FindAnyObjectByType<MultiplayerLobbyBootstrap>();
            Button quickEditButton = GameObject.Find("Quick Edit Button").GetComponent<Button>();
            Button connectButton = GameObject.Find("Connect Button").GetComponent<Button>();
            Button hostButton = GameObject.Find("Host Button").GetComponent<Button>();

            Assert.NotNull(lobby);
            quickEditButton.onClick.Invoke();

            GameObject modal = GameObject.Find("Quick Edit Modal");
            Assert.NotNull(modal);
            Assert.IsTrue(modal.activeSelf);
            InputField input = GameObject.Find("Quick Edit Deck Code Input").GetComponent<InputField>();
            Button applyButton = GameObject.Find("Apply Deck Code Button").GetComponent<Button>();
            Assert.NotNull(input);
            Assert.NotNull(applyButton);

            input.text = DeckCodeCodec.Export(importedDeck);
            applyButton.onClick.Invoke();

            Assert.IsFalse(modal.activeSelf);
            connectButton.onClick.Invoke();
            hostButton.onClick.Invoke();

            Assert.NotNull(lobby.Controller.CurrentRoom);
            Assert.AreEqual(importedDeck.deck_id, lobby.Controller.LocalPlayer.deck_id);

            string roomBefore = lobby.Controller.CurrentRoom.ToJson(false);
            input.text = DeckCodeCodec.Export(blockedDeck);
            applyButton.onClick.Invoke();

            Assert.AreEqual(roomBefore, lobby.Controller.CurrentRoom.ToJson(false));

            UnityEngine.Object.DestroyImmediate(lobby.gameObject);
        }

        [Test]
        public void MultiplayerLobbyRejectsRoomActionsWhenOnlineDeckIsNotReady()
        {
            FakeTransport transport = new FakeTransport();
            transport.status = MultiplayerTransportStatus.ConnectedToLobby;
            VanguardDeck incompleteDeck = VanguardDeck.Create("Incomplete Online", "D", "vanguard_th", "test");
            incompleteDeck.AddCard(DeckZone.Main, "ONLY-CARD", 1);

            MultiplayerLobbyBootstrap.Show(incompleteDeck, Manifest(), transport);

            MultiplayerLobbyBootstrap lobby = UnityEngine.Object.FindAnyObjectByType<MultiplayerLobbyBootstrap>();
            Button hostButton = GameObject.Find("Host Button").GetComponent<Button>();
            Button joinButton = GameObject.Find("Join Button").GetComponent<Button>();
            Button reconnectButton = GameObject.Find("Reconnect Button").GetComponent<Button>();

            Assert.NotNull(lobby);

            hostButton.onClick.Invoke();
            joinButton.onClick.Invoke();
            reconnectButton.onClick.Invoke();

            Assert.IsNull(transport.createdRoom);
            Assert.IsNull(transport.localPlayer);
            Assert.IsNull(transport.sentReconnectRequest);
            Assert.IsNull(lobby.Controller.CurrentRoom);

            UnityEngine.Object.DestroyImmediate(lobby.gameObject);
        }

        [Test]
        public void MultiplayerLobbyLocksBackHomeWhileRoomIsActive()
        {
            FakeTransport transport = new FakeTransport();
            MultiplayerLobbyBootstrap.Show(CreateSampleDeck("p1"), Manifest(), transport);

            MultiplayerLobbyBootstrap lobby = UnityEngine.Object.FindAnyObjectByType<MultiplayerLobbyBootstrap>();
            Button connectButton = GameObject.Find("Connect Button").GetComponent<Button>();
            Button hostButton = GameObject.Find("Host Button").GetComponent<Button>();
            Button backHomeButton = GameObject.Find("Back Home Button").GetComponent<Button>();
            Button leaveRoomButton = GameObject.Find("Leave Room Button").GetComponent<Button>();

            Assert.NotNull(lobby);
            Assert.IsTrue(backHomeButton.interactable);
            Assert.IsFalse(leaveRoomButton.interactable);

            connectButton.onClick.Invoke();
            hostButton.onClick.Invoke();

            Assert.NotNull(lobby.Controller.CurrentRoom);
            Assert.IsFalse(backHomeButton.interactable);
            Assert.IsTrue(leaveRoomButton.interactable);

            backHomeButton.onClick.Invoke();

            Assert.NotNull(UnityEngine.Object.FindAnyObjectByType<MultiplayerLobbyBootstrap>());
            Assert.NotNull(lobby.Controller.CurrentRoom);
            Assert.IsFalse(backHomeButton.interactable);

            leaveRoomButton.onClick.Invoke();

            Assert.IsNull(lobby.Controller.CurrentRoom);
            Assert.IsTrue(backHomeButton.interactable);
            Assert.IsFalse(leaveRoomButton.interactable);

            UnityEngine.Object.DestroyImmediate(lobby.gameObject);
        }

        private static VanguardDeck CreateSampleDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "test");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            return deck;
        }

        private static CardPackManifest Manifest()
        {
            return new CardPackManifest
            {
                pack_id = "vanguard_th",
                source_version = "test",
                definition_hash = "hash-a",
                image_manifest_hash = "image-manifest",
                image_content_hash = "image-content"
            };
        }

        private static PackSyncInfo Pack(string definitionHash = "hash-a")
        {
            return new PackSyncInfo
            {
                pack_id = "vanguard_th",
                source_version = "test",
                definition_hash = definitionHash,
                image_manifest_hash = "image-manifest",
                image_content_hash = "image-content"
            };
        }

        private static RoomPlayerInfo Player(string playerId, string displayName)
        {
            return new RoomPlayerInfo
            {
                player_id = playerId,
                display_name = displayName,
                deck_id = playerId + "-deck",
                deck_hash = playerId + "-hash",
                connected = true
            };
        }

        private static RoomPlayerInfo CommittedPlayer(string playerId, string displayName)
        {
            RoomPlayerInfo player = Player(playerId, displayName);
            player.deck_commitment = playerId + "-commitment";
            player.deck_commitment_algorithm = DeckCommitmentService.Algorithm;
            player.deck_reveal_policy = "end_of_match";
            player.deck_code = "";
            return player;
        }

        private static LegalGameAction FirstAction(IReadOnlyList<LegalGameAction> actions, GameActionType actionType)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == actionType)
                {
                    return action;
                }
            }

            Assert.Fail("Missing action " + actionType);
            return null;
        }

        private sealed class FakeTransport : IMultiplayerTransport
        {
            public MultiplayerTransportStatus status = MultiplayerTransportStatus.Disconnected;
            public MultiplayerRoomState createdRoom;
            public MultiplayerRoomState sentRoom;
            public readonly List<NetworkEventEnvelope> sentEvents = new List<NetworkEventEnvelope>();
            public NetworkReconnectRequest sentReconnectRequest;
            public NetworkEventBatch sentReconnectBatch;
            public NetworkDeckRevealRequest sentDeckRevealRequest;
            public NetworkDeckRevealResponse sentDeckRevealResponse;
            public NetworkTriggerCheckReplayLogPayload sentTriggerCheckReplayLog;
            public RoomPlayerInfo localPlayer;

            public string TransportName
            {
                get { return "Fake"; }
            }

            public MultiplayerTransportStatus Status
            {
                get { return status; }
            }

            public string LastError
            {
                get { return null; }
            }

            public event Action<MultiplayerRoomState> RoomStateReceived;
            public event Action<NetworkEventEnvelope> GameEventReceived;
            public event Action<NetworkPublicGameEvent> PublicGameEventReceived;
            public event Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
            public event Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
            public event Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
            public event Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
            public event Action<NetworkReconnectRequest> ReconnectRequestReceived;
            public event Action<NetworkEventBatch> ReconnectBatchReceived;
            public event Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
            public event Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

            public MultiplayerTransportResult Connect()
            {
                status = MultiplayerTransportStatus.ConnectedToLobby;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo player)
            {
                createdRoom = room;
                localPlayer = player;
                status = MultiplayerTransportStatus.JoiningRoom;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo player, PackSyncInfo localPack)
            {
                localPlayer = player;
                status = MultiplayerTransportStatus.JoiningRoom;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
            {
                sentRoom = room;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
            {
                sentEvents.Add(envelope);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                sentTriggerCheckReplayLog = payload;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
            {
                sentReconnectRequest = request;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
            {
                sentReconnectBatch = batch;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                sentDeckRevealRequest = request;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                sentDeckRevealResponse = response;
                return MultiplayerTransportResult.Ok();
            }

            public void Tick()
            {
            }

            public void Disconnect()
            {
                status = MultiplayerTransportStatus.Disconnected;
            }

            public void EmitRoom(MultiplayerRoomState room)
            {
                status = MultiplayerTransportStatus.InRoom;
                Action<MultiplayerRoomState> handler = RoomStateReceived;
                if (handler != null)
                {
                    handler(room);
                }
            }

            public void EmitEvent(NetworkEventEnvelope envelope)
            {
                Action<NetworkEventEnvelope> handler = GameEventReceived;
                if (handler != null)
                {
                    handler(envelope);
                }
            }

            public void EmitReconnectRequest(NetworkReconnectRequest request)
            {
                Action<NetworkReconnectRequest> handler = ReconnectRequestReceived;
                if (handler != null)
                {
                    handler(request);
                }
            }

            public void EmitReconnectBatch(NetworkEventBatch batch)
            {
                Action<NetworkEventBatch> handler = ReconnectBatchReceived;
                if (handler != null)
                {
                    handler(batch);
                }
            }

            public void EmitDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                Action<NetworkDeckRevealRequest> handler = DeckRevealRequestReceived;
                if (handler != null)
                {
                    handler(request);
                }
            }

            public void EmitDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                Action<NetworkDeckRevealResponse> handler = DeckRevealResponseReceived;
                if (handler != null)
                {
                    handler(response);
                }
            }
        }
    }
}
