using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PhotonRealtimeTransportTests
    {
        [Test]
        public void PhotonRoomPropertiesRoundTripRoomState()
        {
            MultiplayerRoomState room = CreateRoom(Pack("hash-a"));
            room.players.Add(Player("p1", true));
            room.players.Add(Player("p2", false));

            System.Collections.Generic.Dictionary<string, string> properties = PhotonRealtimePayloadCodec.RoomToProperties(room);
            MultiplayerRoomState roundTrip = PhotonRealtimePayloadCodec.RoomFromProperties(properties);

            Assert.AreEqual(room.protocol_version, roundTrip.protocol_version);
            Assert.AreEqual(room.room_id, roundTrip.room_id);
            Assert.AreEqual(room.format, roundTrip.format);
            Assert.AreEqual(room.host_player_id, roundTrip.host_player_id);
            Assert.AreEqual(room.random_seed, roundTrip.random_seed);
            Assert.AreEqual(room.pack.definition_hash, roundTrip.pack.definition_hash);
            Assert.AreEqual(2, roundTrip.players.Count);
        }

        [Test]
        public void PhotonPayloadRoundTripsNetworkEventEnvelope()
        {
            GameState initial = CreateState(901);
            GameState live = GameState.FromJson(initial.ToJson(false));
            RulesCore.ExecuteOrThrow(live, FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw));

            NetworkEventEnvelope envelope = MultiplayerProtocol.CreateEnvelope(CreateRoom(Pack("hash-a")), "p1", live, live.event_log[0]);
            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodeGameEvent(envelope);

            NetworkEventEnvelope roundTrip;
            string rejectionReason;
            Assert.IsTrue(PhotonRealtimePayloadCodec.TryDecodeGameEvent(payload, out roundTrip, out rejectionReason), rejectionReason);
            Assert.AreEqual(PhotonRealtimePayloadCodec.GameEventCode, payload.event_code);
            Assert.AreEqual(envelope.player_id, payload.sender_player_id);
            Assert.AreEqual(envelope.event_index, roundTrip.event_index);
            Assert.AreEqual(envelope.game_event.event_id, roundTrip.game_event.event_id);
        }

        [Test]
        public void PhotonPayloadRoundTripsPublicGameEventWithoutPrivateCardLeak()
        {
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                event_id = "public-1",
                source_event_id = "event-1",
                visibility = PublicEventVisibility.Public,
                action_type = GameActionType.Draw,
                actor_index = 0,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };

            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodePublicGameEvent(publicEvent, "p1");

            NetworkPublicGameEvent roundTrip;
            string rejectionReason;
            Assert.IsTrue(PhotonRealtimePayloadCodec.TryDecodePublicGameEvent(payload, out roundTrip, out rejectionReason), rejectionReason);
            Assert.AreEqual(PhotonRealtimePayloadCodec.PublicGameEventCode, payload.event_code);
            Assert.AreEqual("p1", payload.sender_player_id);
            Assert.AreEqual(publicEvent.event_id, roundTrip.event_id);
            Assert.AreEqual(GameActionType.Draw, roundTrip.action_type);
            Assert.IsTrue(roundTrip.hides_card_identity);
            Assert.IsTrue(string.IsNullOrEmpty(roundTrip.public_card_id));
            Assert.IsTrue(string.IsNullOrEmpty(roundTrip.public_card_instance_id));
            Assert.AreEqual(1, roundTrip.to_zone_count_delta);
        }

        [Test]
        public void PhotonPayloadRoundTripsPublicRevealEvent()
        {
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                event_id = "public-2",
                source_event_id = "event-2",
                visibility = PublicEventVisibility.Public,
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                from_zone = GameZone.Hand,
                to_zone = GameZone.Vanguard,
                public_card_id = "BT01-001TH",
                public_card_instance_id = "public-instance-1",
                reveal_proof = "commitment-proof-placeholder"
            };

            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodePublicGameEvent(publicEvent, "p1");

            NetworkPublicGameEvent roundTrip;
            string rejectionReason;
            Assert.IsTrue(PhotonRealtimePayloadCodec.TryDecodePublicGameEvent(payload, out roundTrip, out rejectionReason), rejectionReason);
            Assert.AreEqual(GameActionType.MoveCard, roundTrip.action_type);
            Assert.IsFalse(roundTrip.hides_card_identity);
            Assert.AreEqual("BT01-001TH", roundTrip.public_card_id);
            Assert.AreEqual("public-instance-1", roundTrip.public_card_instance_id);
            Assert.AreEqual("commitment-proof-placeholder", roundTrip.reveal_proof);
        }

        [Test]
        public void PhotonPayloadRoundTripsDeckRevealRequestAndResponse()
        {
            NetworkDeckRevealRequest request = new NetworkDeckRevealRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-D",
                requester_player_id = "p1",
                target_player_id = "p2"
            };
            NetworkDeckRevealResponse response = new NetworkDeckRevealResponse
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-D",
                player_id = "p2",
                revealed_deck_code = "VGTH1.fake",
                deck_reveal_nonce = "nonce",
                deck_commitment = "commitment",
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                pack_definition_hash = "hash-a"
            };

            NetworkDeckRevealRequest roundTripRequest;
            NetworkDeckRevealResponse roundTripResponse;
            string rejectionReason;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodeDeckRevealRequest(
                    PhotonRealtimePayloadCodec.EncodeDeckRevealRequest(request),
                    out roundTripRequest,
                    out rejectionReason),
                rejectionReason);
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodeDeckRevealResponse(
                    PhotonRealtimePayloadCodec.EncodeDeckRevealResponse(response),
                    out roundTripResponse,
                    out rejectionReason),
                rejectionReason);

            Assert.AreEqual("p1", roundTripRequest.requester_player_id);
            Assert.AreEqual("p2", roundTripRequest.target_player_id);
            Assert.AreEqual("p2", roundTripResponse.player_id);
            Assert.AreEqual("nonce", roundTripResponse.deck_reveal_nonce);
        }

        [Test]
        public void PhotonPayloadRoundTripsReconnectRequestAndBatch()
        {
            NetworkReconnectRequest request = new NetworkReconnectRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-R",
                player_id = "p2",
                from_event_index = 3
            };
            PhotonRealtimePayload requestPayload = PhotonRealtimePayloadCodec.EncodeReconnectRequest(request);

            NetworkReconnectRequest roundTripRequest;
            string rejectionReason;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodeReconnectRequest(requestPayload, out roundTripRequest, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(PhotonRealtimePayloadCodec.ReconnectRequestEventCode, requestPayload.event_code);
            Assert.AreEqual(request.from_event_index, roundTripRequest.from_event_index);

            NetworkEventBatch batch = new NetworkEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-R",
                from_event_index = 3
            };
            PhotonRealtimePayload batchPayload = PhotonRealtimePayloadCodec.EncodeReconnectBatch(batch, "p1");

            NetworkEventBatch roundTripBatch;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodeReconnectBatch(batchPayload, out roundTripBatch, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(PhotonRealtimePayloadCodec.ReconnectBatchEventCode, batchPayload.event_code);
            Assert.AreEqual(batch.room_id, roundTripBatch.room_id);
            Assert.AreEqual(batch.from_event_index, roundTripBatch.from_event_index);
        }

        [Test]
        public void PhotonAdapterRejectsConnectWhenAppIdMissing()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig());

            MultiplayerTransportResult result = adapter.Connect();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PHOTON_APP_ID_MISSING", result.error_code);
            Assert.AreEqual(MultiplayerTransportStatus.Failed, adapter.Status);
        }

        [Test]
        public void PhotonAdapterReportsSdkBridgeAvailability()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });

            if (adapter.IsSdkBridgeEnabled)
            {
                Assert.IsTrue(adapter.IsSdkBridgeEnabled);
                return;
            }

            MultiplayerTransportResult result = adapter.Connect();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PHOTON_SDK_NOT_ENABLED", result.error_code);
            Assert.IsFalse(adapter.IsSdkBridgeEnabled);
        }

        [Test]
        public void PhotonAdapterDispatchesDecodedIncomingPayloads()
        {
            MultiplayerRoomState room = CreateRoom(Pack("hash-a"));
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });

            MultiplayerRoomState receivedRoom = null;
            adapter.RoomStateReceived += received => receivedRoom = received;

            adapter.ReceiveRoomStateForTest(PhotonRealtimePayloadCodec.EncodeRoomState(room, "p1"));

            Assert.NotNull(receivedRoom);
            Assert.AreEqual(room.room_id, receivedRoom.room_id);
            Assert.AreEqual(room.pack.definition_hash, receivedRoom.pack.definition_hash);
        }

        [Test]
        public void PhotonAdapterDispatchesPublicGameEventPayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });
            NetworkPublicGameEvent received = null;
            adapter.PublicGameEventReceived += publicEvent => received = publicEvent;
            NetworkPublicGameEvent publicEventPayload = new NetworkPublicGameEvent
            {
                event_id = "public-dispatch",
                source_event_id = "event-dispatch",
                action_type = GameActionType.Draw,
                actor_index = 0,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };

            adapter.ReceivePublicGameEventForTest(PhotonRealtimePayloadCodec.EncodePublicGameEvent(publicEventPayload, "p1"));

            Assert.NotNull(received);
            Assert.AreEqual("public-dispatch", received.event_id);
            Assert.IsTrue(received.hides_card_identity);
        }

        [Test]
        public void PhotonAdapterDispatchesDeckRevealPayloads()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });
            NetworkDeckRevealRequest receivedRequest = null;
            NetworkDeckRevealResponse receivedResponse = null;
            adapter.DeckRevealRequestReceived += request => receivedRequest = request;
            adapter.DeckRevealResponseReceived += response => receivedResponse = response;
            NetworkDeckRevealRequest requestPayload = new NetworkDeckRevealRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-D",
                requester_player_id = "p1",
                target_player_id = "p2"
            };
            NetworkDeckRevealResponse responsePayload = new NetworkDeckRevealResponse
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-D",
                player_id = "p2",
                revealed_deck_code = "VGTH1.fake",
                deck_reveal_nonce = "nonce",
                deck_commitment = "commitment",
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                pack_definition_hash = "hash-a"
            };

            adapter.ReceiveDeckRevealRequestForTest(PhotonRealtimePayloadCodec.EncodeDeckRevealRequest(requestPayload));
            adapter.ReceiveDeckRevealResponseForTest(PhotonRealtimePayloadCodec.EncodeDeckRevealResponse(responsePayload));

            Assert.NotNull(receivedRequest);
            Assert.NotNull(receivedResponse);
            Assert.AreEqual("p2", receivedRequest.target_player_id);
            Assert.AreEqual("nonce", receivedResponse.deck_reveal_nonce);
        }

        [Test]
        public void PhotonAdapterDispatchesReconnectPayloads()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });
            NetworkReconnectRequest receivedRequest = null;
            NetworkEventBatch receivedBatch = null;
            adapter.ReconnectRequestReceived += request => receivedRequest = request;
            adapter.ReconnectBatchReceived += batch => receivedBatch = batch;

            NetworkReconnectRequest requestPayload = new NetworkReconnectRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-R",
                player_id = "p2",
                from_event_index = 4
            };
            NetworkEventBatch batchPayload = new NetworkEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-R",
                from_event_index = 4
            };

            adapter.ReceiveReconnectRequestForTest(PhotonRealtimePayloadCodec.EncodeReconnectRequest(requestPayload));
            adapter.ReceiveReconnectBatchForTest(PhotonRealtimePayloadCodec.EncodeReconnectBatch(batchPayload, "p1"));

            Assert.NotNull(receivedRequest);
            Assert.NotNull(receivedBatch);
            Assert.AreEqual(4, receivedRequest.from_event_index);
            Assert.AreEqual("ROOM-R", receivedBatch.room_id);
        }

        private static GameState CreateState(int seed)
        {
            return GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), seed);
        }

        private static MultiplayerRoomState CreateRoom(PackSyncInfo pack)
        {
            return MultiplayerProtocol.CreateRoom("ROOM1", "D", "p1", 9001, pack);
        }

        private static PackSyncInfo Pack(string definitionHash)
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

        private static RoomPlayerInfo Player(string playerId, bool connected)
        {
            return new RoomPlayerInfo
            {
                player_id = playerId,
                display_name = playerId,
                deck_id = playerId + "-deck",
                deck_hash = playerId + "-deck-hash",
                connected = connected
            };
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

        private static LegalGameAction FirstAction(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GameActionType actionType)
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
    }
}
