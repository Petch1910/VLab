using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class MultiplayerLobbyStatusFormatterTests
    {
        [Test]
        public void ConnectionStatusShowsPhotonSetupWithoutRawDebugOnlyText()
        {
            string formatted = MultiplayerLobbyStatusFormatter.FormatConnectionStatus(
                "Photon Realtime",
                MultiplayerTransportStatus.Disconnected,
                false,
                "PHOTON_APP_ID_MISSING: missing app id");

            Assert.IsTrue(formatted.Contains("Photon Realtime"));
            Assert.IsTrue(formatted.Contains("Disconnected"));
            Assert.IsTrue(formatted.Contains("AppId missing"));
            Assert.IsTrue(formatted.Contains("PHOTON_APP_ID_MISSING"));
        }

        [Test]
        public void DeckPackStatusIncludesSelectedDeckAndPack()
        {
            VanguardDeck deck = VanguardDeck.Create("Online Deck", "D", "vanguard_th", "test");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            deck.AddCard(DeckZone.Ride, "CARD-002", 4);
            CardPackManifest manifest = new CardPackManifest
            {
                pack_id = "vanguard_th",
                source_version = "2026.06"
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatDeckPackStatus(deck, manifest);

            Assert.IsTrue(formatted.Contains("Online Deck"));
            Assert.IsTrue(formatted.Contains("Main 50"));
            Assert.IsTrue(formatted.Contains("Ride 4"));
            Assert.IsTrue(formatted.Contains("vanguard_th"));
        }

        [Test]
        public void QuickDeckSelectorFormatsCountsLockAndNoDeckCode()
        {
            VanguardDeck deck = VanguardDeck.Create("Quick Deck", "D", "vanguard_th", "test");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            deck.AddCard(DeckZone.Ride, "CARD-002", 4);

            string formatted = MultiplayerLobbyStatusFormatter.FormatQuickDeckSelector(
                deck,
                3,
                2,
                true);

            Assert.IsTrue(formatted.Contains("Quick Deck"));
            Assert.IsTrue(formatted.Contains("Main 50"));
            Assert.IsTrue(formatted.Contains("Ride 4"));
            Assert.IsTrue(formatted.Contains("saved 2/3"));
            Assert.IsTrue(formatted.Contains("Leave Room"));
            Assert.IsFalse(formatted.Contains("deck_code"));
        }

        [Test]
        public void QuickEditStatusFormatsSessionImportGuidanceWithoutDeckCode()
        {
            VanguardDeck deck = VanguardDeck.Create("Import Target", "D", "vanguard_th", "test");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            deck.AddCard(DeckZone.Ride, "CARD-002", 4);

            string open = MultiplayerLobbyStatusFormatter.FormatQuickEditStatus(
                deck,
                "Import ready.",
                false);
            string lockedStatus = MultiplayerLobbyStatusFormatter.FormatQuickEditStatus(deck, null, true);

            Assert.IsTrue(open.Contains("Import Target"));
            Assert.IsTrue(open.Contains("Main 50"));
            Assert.IsTrue(open.Contains("Paste a deck code"));
            Assert.IsFalse(open.Contains("deck_code"));
            Assert.IsTrue(lockedStatus.Contains("Locked"));
            Assert.IsTrue(lockedStatus.Contains("Leave Room"));
        }

        [Test]
        public void RoomSummaryDoesNotExposeDeckCode()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-UI",
                "D",
                "p1",
                1234,
                Pack());
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "Host",
                deck_code = "VGTH1-secret-code",
                deck_hash = "hash-ready",
                main_deck_count = 50,
                ride_deck_count = 4,
                g_deck_count = 0,
                connected = true,
                event_cursor = 7
            });

            string formatted = MultiplayerLobbyStatusFormatter.FormatRoomSummary(room);

            Assert.IsTrue(formatted.Contains("ROOM-UI"));
            Assert.IsTrue(formatted.Contains("Host"));
            Assert.IsTrue(formatted.Contains("cursor 7"));
            Assert.IsTrue(formatted.Contains("deck 50/4/0"));
            Assert.IsTrue(formatted.Contains("not ready"));
            Assert.IsFalse(formatted.Contains("VGTH1-secret-code"));
        }

        [Test]
        public void RoomStatusShowsConnectionPlayersHashesAndCursorWithoutDeckCode()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-STATUS",
                "D",
                "p1",
                1234,
                Pack());
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "Host",
                deck_code = "VGTH1-secret-code",
                deck_hash = "hash-ready",
                connected = true,
                ready = true,
                event_cursor = 2
            });
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p2",
                display_name = "Guest",
                deck_hash = "guest-hash",
                connected = true,
                event_cursor = 5
            });

            string formatted = MultiplayerLobbyStatusFormatter.FormatRoomStatus(
                MultiplayerTransportStatus.ConnectedToLobby,
                room,
                Pack());

            Assert.IsTrue(formatted.Contains("Connection: ConnectedToLobby"));
            Assert.IsTrue(formatted.Contains("Players: 2/2 connected"));
            Assert.IsTrue(formatted.Contains("Deck hash: 2/2 ready"));
            Assert.IsTrue(formatted.Contains("Pack hash: match"));
            Assert.IsTrue(formatted.Contains("Public cursor: 5"));
            Assert.IsTrue(formatted.Contains("Host ready"));
            Assert.IsTrue(formatted.Contains("Guest not ready"));
            Assert.IsFalse(formatted.Contains("VGTH1-secret-code"));
        }

        [Test]
        public void RoomStatusShowsPackMismatchAndMissingDeckHash()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-MISMATCH",
                "D",
                "p1",
                1234,
                Pack("remote-hash"));
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "Host",
                connected = true
            });

            string formatted = MultiplayerLobbyStatusFormatter.FormatRoomStatus(
                MultiplayerTransportStatus.ConnectedToLobby,
                room,
                Pack("local-hash"));

            Assert.IsTrue(formatted.Contains("Deck hash: 0/1 ready"));
            Assert.IsTrue(formatted.Contains("Pack hash: mismatch"));
            Assert.IsTrue(formatted.Contains("deck hash missing"));
        }

        [Test]
        public void FlowSummaryGuidesNoRoomFlow()
        {
            string formatted = MultiplayerLobbyStatusFormatter.FormatFlowSummary(
                MultiplayerTransportStatus.ConnectedToLobby,
                null,
                null,
                null);

            Assert.IsTrue(formatted.Contains("host or join"));
            Assert.IsTrue(formatted.Contains("Start: unavailable"));
            Assert.IsTrue(formatted.Contains("Rematch"));
        }

        [Test]
        public void FlowSummaryShowsReadyStartGateWithoutDeckCode()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-FLOW",
                "D",
                "p1",
                1234,
                Pack());
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "Host",
                deck_code = "VGTH1-secret-code",
                deck_hash = "hash-ready",
                connected = true,
                ready = true
            });
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p2",
                display_name = "Guest",
                deck_hash = "hash-ready",
                connected = true,
                ready = true
            });

            string formatted = MultiplayerLobbyStatusFormatter.FormatFlowSummary(
                MultiplayerTransportStatus.ConnectedToLobby,
                room,
                room.players[0],
                new DeckPrivacyGameplayDecision
                {
                    can_start_gameplay = true
                });

            Assert.IsTrue(formatted.Contains("players ready 2/2"));
            Assert.IsTrue(formatted.Contains("Start Table available"));
            Assert.IsFalse(formatted.Contains("VGTH1-secret-code"));
        }

        [Test]
        public void FlowSummaryShowsRematchOnlyWhenEnded()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-END",
                "D",
                "p1",
                1234,
                Pack());
            room.state = RoomLifecycleStates.Ended;
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                connected = true
            });

            string formatted = MultiplayerLobbyStatusFormatter.FormatFlowSummary(
                MultiplayerTransportStatus.ConnectedToLobby,
                room,
                room.players[0],
                null);

            Assert.IsTrue(formatted.Contains("Start: room ended"));
            Assert.IsTrue(formatted.Contains("Rematch: available"));
        }

        [Test]
        public void NavigationLockoutExplainsLeaveRoomWithoutDeckCode()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-LOCK",
                "D",
                "p1",
                1234,
                Pack());
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "Host",
                deck_code = "VGTH1-secret-code",
                connected = true
            });

            string inRoom = MultiplayerLobbyStatusFormatter.FormatNavigationLockout(room);
            string noRoom = MultiplayerLobbyStatusFormatter.FormatNavigationLockout(null);

            Assert.IsTrue(inRoom.Contains("Back Home is locked"));
            Assert.IsTrue(inRoom.Contains("Leave Room"));
            Assert.IsFalse(inRoom.Contains("VGTH1-secret-code"));
            Assert.IsTrue(noRoom.Contains("Back Home is available"));
        }

        [Test]
        public void ReconnectSummaryShowsRequestAndBatchCounts()
        {
            NetworkReconnectRequest request = new NetworkReconnectRequest
            {
                player_id = "guest",
                from_event_index = 3
            };
            NetworkEventBatch batch = new NetworkEventBatch
            {
                from_event_index = 3
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(request, batch);

            Assert.IsTrue(formatted.Contains("guest"));
            Assert.IsTrue(formatted.Contains("from event 3"));
            Assert.IsTrue(formatted.Contains("0 events"));
            Assert.IsTrue(formatted.Contains("matching local state"));
            Assert.IsTrue(formatted.Contains("asking for events"));
        }

        [Test]
        public void ReconnectSummaryGuidesFreshAndResumePaths()
        {
            string formatted = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(null, null);

            Assert.IsTrue(formatted.Contains("no pending request or batch"));
            Assert.IsTrue(formatted.Contains("room id + cursor"));
            Assert.IsTrue(formatted.Contains("Reconnect to resume"));
            Assert.IsTrue(formatted.Contains("cursor 0"));
        }

        [Test]
        public void ReconnectSummaryShowsWaitingForPeerWhenRequestHasNoBatch()
        {
            NetworkReconnectRequest request = new NetworkReconnectRequest
            {
                player_id = "guest",
                from_event_index = 4
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(request, null);

            Assert.IsTrue(formatted.Contains("guest"));
            Assert.IsTrue(formatted.Contains("from event 4"));
            Assert.IsTrue(formatted.Contains("Waiting"));
            Assert.IsTrue(formatted.Contains("reconnect batch"));
        }

        [Test]
        public void ReconnectSummaryMarksZeroCursorBatchReadyForStartTable()
        {
            NetworkEventBatch batch = new NetworkEventBatch
            {
                from_event_index = 0
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(null, batch);

            Assert.IsTrue(formatted.Contains("0 events"));
            Assert.IsTrue(formatted.Contains("ready to apply"));
        }

        [Test]
        public void ReconnectSummaryExplainsRoomMismatch()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-A",
                "D",
                "p1",
                1234,
                Pack());
            NetworkEventBatch batch = new NetworkEventBatch
            {
                room_id = "ROOM-B",
                from_event_index = 0
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(
                null,
                batch,
                room,
                0,
                null);

            Assert.IsTrue(formatted.Contains("batch is for room ROOM-B"));
            Assert.IsTrue(formatted.Contains("current room is ROOM-A"));
        }

        [Test]
        public void ReconnectSummaryExplainsCursorGap()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-GAP",
                "D",
                "p1",
                1234,
                Pack());
            NetworkEventBatch batch = new NetworkEventBatch
            {
                room_id = "ROOM-GAP",
                from_event_index = 7
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(
                null,
                batch,
                room,
                0,
                null);

            Assert.IsTrue(formatted.Contains("Applying it would skip events"));
        }

        [Test]
        public void StartTableRejectionFormatsReconnectCursorMismatch()
        {
            string formatted = MultiplayerLobbyStatusFormatter.FormatStartTableRejection(
                "RECONNECT_BATCH_CURSOR_MISMATCH: batch starts at 7 but session is at 0");

            Assert.IsTrue(formatted.Contains("reconnect batch cursor does not match"));
            Assert.IsTrue(formatted.Contains("cursor 0"));
        }

        [Test]
        public void TrustSummaryKeepsTrustedClientWarningVisible()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-TRUST",
                "D",
                "p1",
                1234,
                Pack());
            DeckPrivacyGameplayDecision decision = DeckPrivacyGameplayPolicy.Evaluate(room);

            string formatted = MultiplayerLobbyStatusFormatter.FormatTrustSummary(room, decision);
            string lower = formatted.ToLowerInvariant();

            Assert.IsTrue(lower.Contains("trusted-client friend room"));
            Assert.IsTrue(lower.Contains("not ranked secure"));
        }

        [Test]
        public void RevealSummaryDoesNotExposeRevealedDeckCode()
        {
            NetworkDeckRevealResponse response = new NetworkDeckRevealResponse
            {
                player_id = "target",
                revealed_deck_code = "VGTH1-revealed-secret"
            };

            string formatted = MultiplayerLobbyStatusFormatter.FormatRevealSummary(
                null,
                response,
                true,
                "Deck reveal verified for target.");

            Assert.IsTrue(formatted.Contains("target"));
            Assert.IsTrue(formatted.Contains("verified"));
            Assert.IsFalse(formatted.Contains("VGTH1-revealed-secret"));
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
    }
}
