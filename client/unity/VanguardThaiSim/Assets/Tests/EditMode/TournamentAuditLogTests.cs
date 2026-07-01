using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class TournamentAuditLogTests
    {
        [Test]
        public void AuditLogExportsRoomPackPlayersPublicEventsAndResult()
        {
            MultiplayerRoomState room = CreateRoom();
            NetworkPublicGameEvent hiddenDraw = HiddenDraw();
            NetworkPublicGameEvent publicReveal = PublicReveal();
            TournamentAuditResult matchResult = TournamentAuditResult.Create(
                "completed",
                "p1",
                0,
                "damage_six",
                "2026-06-20T12:00:00.0000000Z");

            TournamentAuditLog auditLog = TournamentAuditLogFactory.Create(
                room,
                new[] { hiddenDraw, publicReveal },
                matchResult,
                "2026-06-20T12:01:00.0000000Z");
            string json = auditLog.ToJson(false);

            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, auditLog.protocol_version);
            Assert.AreEqual("ROOM-AUDIT", auditLog.room_id);
            Assert.AreEqual("D", auditLog.format);
            Assert.AreEqual(RoomLifecycleStates.Ended, auditLog.room_state);
            Assert.AreEqual(20260620, auditLog.random_seed);
            Assert.AreEqual("vanguard_th", auditLog.pack.pack_id);
            Assert.AreEqual("pack-definition", auditLog.pack.definition_hash);
            Assert.AreEqual(2, auditLog.players.Count);
            Assert.AreEqual("deck-hash-p1", auditLog.players[0].deck_hash);
            Assert.AreEqual("commitment-p2", auditLog.players[1].deck_commitment);
            Assert.AreEqual(50, auditLog.players[0].main_deck_count);
            Assert.AreEqual(2, auditLog.public_event_count);
            Assert.AreEqual(2, auditLog.public_events.Count);
            Assert.AreEqual("", auditLog.public_events[0].public_card_id);
            Assert.AreEqual("", auditLog.public_events[0].public_card_instance_id);
            Assert.AreEqual("BT01-001TH", auditLog.public_events[1].public_card_id);
            Assert.AreEqual("public-call-card", auditLog.public_events[1].public_card_instance_id);
            Assert.AreEqual("completed", auditLog.result.status);
            Assert.AreEqual("p1", auditLog.result.winner_player_id);
            Assert.AreEqual(0, auditLog.result.winner_player_index);
            Assert.IsFalse(json.Contains("VGTH1.SECRET"));
            Assert.IsFalse(json.Contains("nonce-secret"));
            Assert.IsFalse(json.Contains("REAL-OPPONENT"));
            Assert.IsFalse(json.Contains("SHOULD-NOT-LEAK"));
        }

        [Test]
        public void AuditLogJsonRoundTripsWithLists()
        {
            TournamentAuditLog auditLog = TournamentAuditLogFactory.Create(
                CreateRoom(),
                new[] { PublicReveal() },
                TournamentAuditResult.Create("completed", "p2", 1, "deck_out", "2026-06-20T13:00:00.0000000Z"),
                "2026-06-20T13:01:00.0000000Z");

            TournamentAuditLog roundTrip = TournamentAuditLog.FromJson(auditLog.ToJson(false));

            Assert.AreEqual(auditLog.room_id, roundTrip.room_id);
            Assert.AreEqual(2, roundTrip.players.Count);
            Assert.AreEqual(1, roundTrip.public_events.Count);
            Assert.AreEqual("BT01-001TH", roundTrip.public_events[0].public_card_id);
            Assert.AreEqual("completed", roundTrip.result.status);
        }

        [Test]
        public void AuditLogCreationDoesNotMutateRoomOrEvents()
        {
            MultiplayerRoomState room = CreateRoom();
            NetworkPublicGameEvent hiddenDraw = HiddenDraw();
            string roomBefore = room.ToJson(false);
            string eventBefore = hiddenDraw.ToJson(false);

            TournamentAuditLogFactory.Create(
                room,
                new[] { hiddenDraw },
                TournamentAuditResult.Create("completed"),
                "2026-06-20T14:00:00.0000000Z");

            Assert.AreEqual(roomBefore, room.ToJson(false));
            Assert.AreEqual(eventBefore, hiddenDraw.ToJson(false));
            Assert.AreEqual("SHOULD-NOT-LEAK", hiddenDraw.public_card_id);
            Assert.AreEqual("REAL-OPPONENT-HAND-0", hiddenDraw.public_card_instance_id);
        }

        [Test]
        public void AuditLogAllowsMissingPublicEventsAndResult()
        {
            TournamentAuditLog auditLog = TournamentAuditLogFactory.Create(
                CreateRoom(),
                null,
                null,
                "2026-06-20T15:00:00.0000000Z");

            Assert.AreEqual(0, auditLog.public_event_count);
            Assert.AreEqual(0, auditLog.public_events.Count);
            Assert.NotNull(auditLog.result);
            Assert.AreEqual("", auditLog.result.status);
        }

        private static MultiplayerRoomState CreateRoom()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-AUDIT",
                "D",
                "p1",
                20260620,
                new PackSyncInfo
                {
                    pack_id = "vanguard_th",
                    source_version = "test-source",
                    definition_hash = "pack-definition",
                    image_manifest_hash = "image-manifest",
                    image_content_hash = "image-content"
                });
            room.state = RoomLifecycleStates.Ended;
            room.room_visibility = RoomVisibilityModes.Friend;
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "Host",
                deck_id = "deck-p1",
                deck_hash = "deck-hash-p1",
                deck_code = "VGTH1.SECRET-HOST",
                deck_commitment = "commitment-p1",
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                deck_reveal_policy = "on_match_end",
                deck_reveal_nonce = "nonce-secret-host",
                main_deck_count = 50,
                ride_deck_count = 4,
                g_deck_count = 0,
                opening_hand_count = 5,
                connected = true,
                ready = true,
                event_cursor = 3
            });
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p2",
                display_name = "Guest",
                deck_id = "deck-p2",
                deck_hash = "deck-hash-p2",
                deck_code = "VGTH1.SECRET-GUEST",
                deck_commitment = "commitment-p2",
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                deck_reveal_policy = "on_match_end",
                deck_reveal_nonce = "nonce-secret-guest",
                main_deck_count = 50,
                ride_deck_count = 4,
                g_deck_count = 0,
                opening_hand_count = 5,
                connected = true,
                ready = true,
                event_cursor = 3
            });
            return room;
        }

        private static NetworkPublicGameEvent HiddenDraw()
        {
            return new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-hidden-draw",
                source_event_id = "true-hidden-draw",
                action_type = GameActionType.Draw,
                actor_index = 1,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true,
                public_card_id = "SHOULD-NOT-LEAK",
                public_card_instance_id = "REAL-OPPONENT-HAND-0",
                reveal_proof = "hidden-event-proof-should-clear"
            };
        }

        private static NetworkPublicGameEvent PublicReveal()
        {
            return new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-reveal",
                source_event_id = "true-reveal",
                action_type = GameActionType.MoveCard,
                actor_index = 1,
                from_zone = GameZone.Hand,
                to_zone = GameZone.RearGuard,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = false,
                public_card_id = "BT01-001TH",
                public_card_instance_id = "public-call-card",
                reveal_proof = "public-reveal-proof"
            };
        }
    }
}
