using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class MultiplayerProtocolTests
    {
        [Test]
        public void RoomReadyValidationRejectsPackMismatch()
        {
            MultiplayerRoomState room = CreateRoom(Pack("hash-a"));
            room.players.Add(Player("p1", true));
            room.players.Add(Player("p2", true));

            MultiplayerProtocolValidationResult accepted = MultiplayerProtocol.ValidateRoomReady(room, Pack("hash-a"));
            MultiplayerProtocolValidationResult rejected = MultiplayerProtocol.ValidateRoomReady(room, Pack("hash-b"));

            Assert.IsTrue(accepted.accepted, accepted.FirstIssue);
            Assert.IsFalse(rejected.accepted);
            Assert.Contains("PACK_HASH_MISMATCH", rejected.issues);
        }

        [Test]
        public void DeckCommitmentUsesCanonicalDeckOrderAndInputs()
        {
            VanguardDeck first = VanguardDeck.Create("A", "D", "vanguard_th", "test");
            first.AddCard(DeckZone.Main, "CARD-B", 2);
            first.AddCard(DeckZone.Main, "CARD-A", 1);
            first.AddCard(DeckZone.Ride, "RIDE-1", 1);
            VanguardDeck second = VanguardDeck.Create("B", "D", "vanguard_th", "test");
            second.AddCard(DeckZone.Ride, "RIDE-1", 1);
            second.AddCard(DeckZone.Main, "CARD-A", 1);
            second.AddCard(DeckZone.Main, "CARD-B", 2);

            string firstCommitment = DeckCommitmentService.CreateCommitment(first, "nonce", "ROOM", "pack-hash");
            string secondCommitment = DeckCommitmentService.CreateCommitment(second, "nonce", "ROOM", "pack-hash");
            string differentNonce = DeckCommitmentService.CreateCommitment(first, "other", "ROOM", "pack-hash");
            string differentRoom = DeckCommitmentService.CreateCommitment(first, "nonce", "ROOM2", "pack-hash");
            first.AddCard(DeckZone.Main, "CARD-C", 1);
            string differentDeck = DeckCommitmentService.CreateCommitment(first, "nonce", "ROOM", "pack-hash");

            Assert.AreEqual(secondCommitment, firstCommitment);
            Assert.AreNotEqual(firstCommitment, differentNonce);
            Assert.AreNotEqual(firstCommitment, differentRoom);
            Assert.AreNotEqual(firstCommitment, differentDeck);
            Assert.IsTrue(DeckCommitmentService.VerifyCommitment(second, "nonce", "ROOM", "pack-hash", firstCommitment));
        }

        [Test]
        public void DeckRevealVerifierRejectsSwappedDeck()
        {
            VanguardDeck original = VanguardDeck.Create("Original", "D", "vanguard_th", "test");
            original.AddCard(DeckZone.Main, "CARD-A", 4);
            original.AddCard(DeckZone.Ride, "RIDE-1", 1);
            VanguardDeck swapped = VanguardDeck.Create("Swapped", "D", "vanguard_th", "test");
            swapped.AddCard(DeckZone.Main, "CARD-A", 3);
            swapped.AddCard(DeckZone.Main, "CARD-B", 1);
            swapped.AddCard(DeckZone.Ride, "RIDE-1", 1);
            RoomPlayerInfo player = Player("p1", true);
            player.deck_reveal_nonce = "nonce";
            player.deck_commitment_algorithm = DeckCommitmentService.Algorithm;
            player.deck_commitment = DeckCommitmentService.CreateCommitment(original, player.deck_reveal_nonce, "ROOM", "pack-hash");

            string rejectionReason;
            bool originalAccepted = DeckCommitmentService.TryVerifyReveal(player, original, "ROOM", "pack-hash", out rejectionReason);
            bool swappedAccepted = DeckCommitmentService.TryVerifyReveal(player, swapped, "ROOM", "pack-hash", out rejectionReason);

            Assert.IsTrue(originalAccepted, rejectionReason);
            Assert.IsFalse(swappedAccepted);
            Assert.AreEqual("DECK_REVEAL_COMMITMENT_MISMATCH", rejectionReason);
        }

        [Test]
        public void DeckRevealVerifierRequiresRevealNonce()
        {
            VanguardDeck original = VanguardDeck.Create("Original", "D", "vanguard_th", "test");
            original.AddCard(DeckZone.Main, "CARD-A", 1);
            RoomPlayerInfo player = Player("p1", true);
            player.deck_commitment_algorithm = DeckCommitmentService.Algorithm;
            player.deck_commitment = DeckCommitmentService.CreateCommitment(original, "nonce", "ROOM", "pack-hash");

            string rejectionReason;
            bool accepted = DeckCommitmentService.TryVerifyReveal(player, original, "ROOM", "pack-hash", out rejectionReason);

            Assert.IsFalse(accepted);
            Assert.AreEqual("DECK_REVEAL_NONCE_MISSING", rejectionReason);
        }

        [Test]
        public void RoomReadyValidationRejectsPublicSharedDeckCode()
        {
            MultiplayerRoomState room = CreateRoom(Pack("hash-a"));
            room.room_visibility = RoomVisibilityModes.Public;
            room.deck_privacy_mode = DeckPrivacyModes.SharedDeckCode;
            room.players.Add(Player("p1", true));
            room.players.Add(Player("p2", true));

            MultiplayerProtocolValidationResult result = MultiplayerProtocol.ValidateRoomReady(room, Pack("hash-a"));

            Assert.IsFalse(result.accepted);
            Assert.Contains("DECK_PRIVACY_SHARED_CODE_NOT_ALLOWED", result.issues);
        }

        [Test]
        public void RoomReadyValidationRequiresCommitmentMetadata()
        {
            MultiplayerRoomState acceptedRoom = CreateRoom(Pack("hash-a"));
            acceptedRoom.room_visibility = RoomVisibilityModes.Public;
            acceptedRoom.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            acceptedRoom.players.Add(CommittedPlayer("p1"));
            acceptedRoom.players.Add(CommittedPlayer("p2"));
            MultiplayerRoomState rejectedRoom = MultiplayerRoomState.FromJson(acceptedRoom.ToJson());
            rejectedRoom.players[0].deck_code = "VGTH1.fake";
            rejectedRoom.players[1].deck_commitment_algorithm = "old";

            MultiplayerProtocolValidationResult accepted = MultiplayerProtocol.ValidateRoomReady(acceptedRoom, Pack("hash-a"));
            MultiplayerProtocolValidationResult rejected = MultiplayerProtocol.ValidateRoomReady(rejectedRoom, Pack("hash-a"));

            Assert.IsTrue(accepted.accepted, accepted.FirstIssue);
            Assert.IsFalse(rejected.accepted);
            Assert.Contains("DECK_CODE_FORBIDDEN_FOR_COMMITMENT", rejected.issues);
            Assert.Contains("DECK_COMMITMENT_ALGORITHM_MISMATCH", rejected.issues);
        }

        [Test]
        public void NetworkEventEnvelopeSerializesGameEvent()
        {
            GameState initial = CreateState(801);
            GameState live = GameState.FromJson(initial.ToJson(false));
            RulesCore.ExecuteOrThrow(live, FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw));

            NetworkEventEnvelope envelope = MultiplayerProtocol.CreateEnvelope(CreateRoom(Pack("hash-a")), "p1", live, live.event_log[0]);
            NetworkEventEnvelope roundTrip = NetworkEventEnvelope.FromJson(envelope.ToJson());

            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, roundTrip.protocol_version);
            Assert.AreEqual(0, roundTrip.event_index);
            Assert.AreEqual(live.game_id, roundTrip.game_id);
            Assert.NotNull(roundTrip.game_event);
            Assert.AreEqual(live.event_log[0].event_id, roundTrip.game_event.event_id);
            Assert.AreEqual(GameActionType.Draw, roundTrip.game_event.action_type);
        }

        [Test]
        public void PublicEventFactoryMasksPrivateDraw()
        {
            GameState state = CreateState(811);
            GameCardInstance card = state.GetPlayer(0).GetZone(GameZone.Deck)[0];
            GameEvent gameEvent = MoveEvent("event-draw", 0, card.instance_id, GameZone.Deck, GameZone.Hand);

            NetworkPublicGameEvent publicEvent = NetworkPublicGameEventFactory.Create(state, gameEvent);

            Assert.AreEqual("event-draw", publicEvent.source_event_id);
            Assert.AreEqual(GameActionType.MoveCard, publicEvent.action_type);
            Assert.AreEqual(GameZone.Deck, publicEvent.from_zone);
            Assert.AreEqual(GameZone.Hand, publicEvent.to_zone);
            Assert.AreEqual(-1, publicEvent.from_zone_count_delta);
            Assert.AreEqual(1, publicEvent.to_zone_count_delta);
            Assert.IsTrue(publicEvent.hides_card_identity);
            Assert.AreEqual("", publicEvent.public_card_id);
            Assert.AreEqual("", publicEvent.public_card_instance_id);
        }

        [Test]
        public void PublicEventFactoryRevealsCardEnteringPublicZone()
        {
            GameState state = CreateState(812);
            GameCardInstance card = state.GetPlayer(0).GetZone(GameZone.Hand)[0];
            GameEvent gameEvent = MoveEvent("event-call", 0, card.instance_id, GameZone.Hand, GameZone.RearGuard);

            NetworkPublicGameEvent publicEvent = NetworkPublicGameEventFactory.Create(state, gameEvent);

            Assert.IsFalse(publicEvent.hides_card_identity);
            Assert.AreEqual(card.card_id, publicEvent.public_card_id);
            Assert.AreNotEqual(card.instance_id, publicEvent.public_card_instance_id);
            Assert.AreEqual("public-event-call-card", publicEvent.public_card_instance_id);
            Assert.AreEqual(GameZone.Hand, publicEvent.from_zone);
            Assert.AreEqual(GameZone.RearGuard, publicEvent.to_zone);
        }

        [Test]
        public void PublicEventFactoryAddsCommitmentRevealProofWhenContextExists()
        {
            GameState state = CreateState(816);
            RoomPlayerInfo player = CommittedPlayer("p1");
            GameCardInstance card = state.GetPlayer(0).GetZone(GameZone.Hand)[0];
            GameEvent gameEvent = MoveEvent("event-call-proof", 0, card.instance_id, GameZone.Hand, GameZone.RearGuard);

            NetworkPublicGameEvent publicEvent = NetworkPublicGameEventFactory.Create(state, gameEvent, player, "ROOM1", "pack-hash");

            string rejectionReason;
            Assert.IsFalse(string.IsNullOrWhiteSpace(publicEvent.reveal_proof));
            Assert.IsTrue(DeckCommitmentService.TryVerifyRevealProof(player, "ROOM1", "pack-hash", publicEvent, out rejectionReason), rejectionReason);
        }

        [Test]
        public void RevealProofVerificationRejectsTamperedPublicCard()
        {
            RoomPlayerInfo player = CommittedPlayer("p1");
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                event_id = "public-proof",
                source_event_id = "event-proof",
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                from_zone = GameZone.Hand,
                to_zone = GameZone.RearGuard,
                public_card_id = "BT01-001TH",
                public_card_instance_id = "public-proof-card"
            };

            string revealProof;
            string rejectionReason;
            Assert.IsTrue(DeckCommitmentService.TryCreateRevealProof(player, "ROOM1", "pack-hash", publicEvent, out revealProof, out rejectionReason), rejectionReason);
            publicEvent.reveal_proof = revealProof;
            publicEvent.public_card_id = "BT01-002TH";

            Assert.IsFalse(DeckCommitmentService.TryVerifyRevealProof(player, "ROOM1", "pack-hash", publicEvent, out rejectionReason));
            Assert.AreEqual("REVEAL_PROOF_MISMATCH", rejectionReason);
        }

        [Test]
        public void PublicEventFactoryKeepsNonCardActionsPublicWithoutCardIdentity()
        {
            GameState state = CreateState(813);
            GameEvent gameEvent = new GameEvent
            {
                event_id = "event-phase",
                action_type = GameActionType.SetPhase,
                actor_index = 0,
                previous_phase = GamePhase.StandAndDraw,
                new_phase = GamePhase.Main
            };

            NetworkPublicGameEvent publicEvent = NetworkPublicGameEventFactory.Create(state, gameEvent);

            Assert.AreEqual("event-phase", publicEvent.source_event_id);
            Assert.AreEqual(GameActionType.SetPhase, publicEvent.action_type);
            Assert.IsFalse(publicEvent.hides_card_identity);
            Assert.AreEqual(0, publicEvent.from_zone_count_delta);
            Assert.AreEqual(0, publicEvent.to_zone_count_delta);
            Assert.IsTrue(string.IsNullOrEmpty(publicEvent.public_card_id));
            Assert.IsTrue(string.IsNullOrEmpty(publicEvent.public_card_instance_id));
        }

        [Test]
        public void PublicReplayFromTrueEventsDoesNotLeakPrivateDrawIdentity()
        {
            GameState initial = CreateState(814);
            GameState live = GameState.FromJson(initial.ToJson(false));
            GameCardInstance drawCard = live.GetPlayer(0).GetZone(GameZone.Deck)[0];
            ApplyMoveEvent(live, "event-draw", 0, drawCard.instance_id, GameZone.Deck, GameZone.Hand);
            GameCardInstance callCard = live.GetPlayer(0).GetZone(GameZone.Hand)[0];
            string revealedCardId = callCard.card_id;
            string trueCallInstanceId = callCard.instance_id;
            ApplyMoveEvent(live, "event-call", 0, callCard.instance_id, GameZone.Hand, GameZone.RearGuard);

            NetworkPublicGameReplay replay = NetworkPublicGameReplay.CreateFromTrueEvents(initial, live.event_log);
            string json = replay.ToJson(false);

            Assert.AreEqual(2, replay.events.Count);
            Assert.IsTrue(replay.events[0].hides_card_identity);
            Assert.AreEqual("", replay.events[0].public_card_id);
            Assert.IsFalse(json.Contains(drawCard.card_id));
            Assert.IsFalse(json.Contains(drawCard.instance_id));
            Assert.IsFalse(replay.events[1].hides_card_identity);
            Assert.AreEqual(revealedCardId, replay.events[1].public_card_id);
            Assert.AreNotEqual(trueCallInstanceId, replay.events[1].public_card_instance_id);
            Assert.IsTrue(json.Contains(revealedCardId));
            Assert.IsFalse(json.Contains(trueCallInstanceId));
        }

        [Test]
        public void PublicReplayPlayerStepsVisibleEventLog()
        {
            GameState initial = CreateState(815);
            NetworkPublicGameEvent first = new NetworkPublicGameEvent
            {
                event_id = "public-1",
                source_event_id = "event-1",
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };
            NetworkPublicGameEvent second = new NetworkPublicGameEvent
            {
                event_id = "public-2",
                source_event_id = "event-2",
                action_type = GameActionType.SetPhase,
                actor_index = 0,
                previous_phase = GamePhase.Mulligan,
                new_phase = GamePhase.Main
            };
            NetworkPublicGameReplay replay = NetworkPublicGameReplay.Create(
                initial,
                new[] { first, second },
                GameStateViewPerspective.Spectator);
            NetworkPublicGameReplayPlayer player = new NetworkPublicGameReplayPlayer(NetworkPublicGameReplay.FromJson(replay.ToJson(false)));

            Assert.AreEqual(2, player.EventCount);
            Assert.AreEqual(0, player.CreateVisibleEventLog().Count);
            Assert.IsTrue(player.StepForward());
            Assert.AreEqual(1, player.CurrentIndex);
            Assert.AreEqual("public-1", player.CreateVisibleEventLog()[0].event_id);
            player.JumpToEnd();
            Assert.AreEqual(2, player.CurrentIndex);
            Assert.IsFalse(player.StepForward());
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, player.InitialStateView.GetPlayer(0).GetZone(GameZone.Hand)[0].card_id);
        }

        [Test]
        public void NetworkEventAppliesToClientStateThroughReducer()
        {
            GameState initial = CreateState(802);
            GameState host = GameState.FromJson(initial.ToJson(false));
            GameState client = GameState.FromJson(initial.ToJson(false));

            RulesCore.ExecuteOrThrow(host, FirstAction(RulesCore.GetLegalActions(host, 0), GameActionType.Draw));
            NetworkEventEnvelope envelope = MultiplayerProtocol.CreateEnvelope(CreateRoom(Pack("hash-a")), "p1", host, host.event_log[0]);

            string rejectionReason;
            Assert.IsTrue(MultiplayerProtocol.TryApplyEnvelope(client, envelope, out rejectionReason), rejectionReason);
            Assert.AreEqual(host.GetPlayer(0).CountZone(GameZone.Deck), client.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(host.GetPlayer(0).CountZone(GameZone.Hand), client.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(1, client.event_log.Count);
        }

        [Test]
        public void NetworkEventRejectsInvalidFirstPreviousEvent()
        {
            GameState initial = CreateState(805);
            GameState host = GameState.FromJson(initial.ToJson(false));
            GameState client = GameState.FromJson(initial.ToJson(false));

            RulesCore.ExecuteOrThrow(host, FirstAction(RulesCore.GetLegalActions(host, 0), GameActionType.Draw));
            NetworkEventEnvelope envelope = MultiplayerProtocol.CreateEnvelope(CreateRoom(Pack("hash-a")), "p1", host, host.event_log[0]);
            envelope.previous_event_id = "event-does-not-exist";

            string rejectionReason;
            Assert.IsFalse(MultiplayerProtocol.TryApplyEnvelope(client, envelope, out rejectionReason));
            Assert.AreEqual("PREVIOUS_EVENT_MISMATCH", rejectionReason);
            Assert.AreEqual(0, client.event_log.Count);
        }

        [Test]
        public void NetworkEventsCanRebuildReplay()
        {
            GameState initial = CreateState(803);
            GameState live = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom(Pack("hash-a"));

            RulesCore.ExecuteOrThrow(live, FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw));
            RulesCore.ExecuteOrThrow(live, FirstPhase(RulesCore.GetLegalActions(live, 0), GamePhase.Main));

            NetworkEventBatch batch = MultiplayerProtocol.CreateReconnectBatch(room, "p1", live, 0);
            GameReplay replay = MultiplayerProtocol.CreateReplayFromNetwork(initial, batch.events);
            GameReplayPlayer player = new GameReplayPlayer(replay);
            player.JumpToEnd();

            Assert.AreEqual(2, batch.events.Count);
            Assert.AreEqual(GamePhase.Main, player.CurrentState.phase);
            Assert.AreEqual(2, player.CurrentState.event_log.Count);
        }

        [Test]
        public void MockMultiplayerRoomSyncsPublishedEventToSecondClient()
        {
            GameState initial = CreateState(804);
            GameState host = GameState.FromJson(initial.ToJson(false));
            GameState client = GameState.FromJson(initial.ToJson(false));
            PackSyncInfo pack = Pack("hash-a");
            MockMultiplayerRoom room = new MockMultiplayerRoom(CreateRoom(pack));

            string rejectionReason;
            Assert.IsTrue(room.TryConnect(Player("p1", true), pack, out rejectionReason), rejectionReason);
            Assert.IsTrue(room.TryConnect(Player("p2", true), pack, out rejectionReason), rejectionReason);

            RulesCore.ExecuteOrThrow(host, FirstAction(RulesCore.GetLegalActions(host, 0), GameActionType.Draw));
            room.Publish("p1", host, host.event_log[0]);

            Assert.AreEqual(1, room.EventCount);
            Assert.AreEqual(1, room.SyncInto("p2", client));
            Assert.AreEqual(host.GetPlayer(0).CountZone(GameZone.Deck), client.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(host.GetPlayer(0).CountZone(GameZone.Hand), client.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void MockMultiplayerRoomRejectsMismatchedPackOnConnect()
        {
            MockMultiplayerRoom room = new MockMultiplayerRoom(CreateRoom(Pack("hash-a")));

            string rejectionReason;
            bool accepted = room.TryConnect(Player("p1", true), Pack("hash-b"), out rejectionReason);

            Assert.IsFalse(accepted);
            Assert.AreEqual("PACK_HASH_MISMATCH", rejectionReason);
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

        private static RoomPlayerInfo CommittedPlayer(string playerId)
        {
            RoomPlayerInfo player = Player(playerId, true);
            player.deck_commitment = playerId + "-commitment";
            player.deck_commitment_algorithm = DeckCommitmentService.Algorithm;
            player.deck_reveal_policy = "end_of_match";
            return player;
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

        private static GameEvent ApplyMoveEvent(GameState state, string eventId, int actorIndex, string cardInstanceId, GameZone fromZone, GameZone toZone)
        {
            GameEvent gameEvent = MoveEvent(eventId, actorIndex, cardInstanceId, fromZone, toZone);
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        private static GameEvent MoveEvent(string eventId, int actorIndex, string cardInstanceId, GameZone fromZone, GameZone toZone)
        {
            return new GameEvent
            {
                event_id = eventId,
                action_type = GameActionType.MoveCard,
                actor_index = actorIndex,
                card_instance_id = cardInstanceId,
                from_zone = fromZone,
                to_zone = toZone,
                from_index = -1,
                to_index = -1
            };
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

        private static LegalGameAction FirstPhase(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GamePhase phase)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.SetPhase && action.phase == phase)
                {
                    return action;
                }
            }

            Assert.Fail("Missing phase " + phase);
            return null;
        }
    }
}
