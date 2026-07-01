using System;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class OwnerPrivateRoomInitializationTests
    {
        [Test]
        public void CreatesLocalTrueStateWithoutOpponentDeckIdentities()
        {
            MultiplayerRoomState room = CreateCommitmentRoom();
            VanguardDeck localDeck = CreateDeck("LOCAL");
            room.players.Add(CommittedPlayer("p1", localDeck, "local-nonce", room));
            room.players.Add(CommittedOpponent("p2"));

            OwnerPrivateRoomInitializationResult result =
                OwnerPrivateRoomInitializer.Create(room, localDeck, "p1", "local-nonce");

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.NotNull(result.session);
            Assert.AreEqual(0, result.session.local_player_index);
            Assert.AreEqual(0, result.session.event_cursor);
            Assert.IsFalse(result.session.reconnect_enabled);
            Assert.AreEqual(OwnerPrivateRoomInitializer.ReconnectBlockedReason, result.session.reconnect_block_reason);

            GameState localTrueState = result.session.local_true_state;
            PlayerGameState localPlayer = localTrueState.GetPlayer(0);
            PlayerGameState opponent = localTrueState.GetPlayer(1);
            Assert.AreEqual(45, localPlayer.deck.Count);
            Assert.AreEqual(5, localPlayer.hand.Count);
            Assert.AreEqual(4, localPlayer.ride_deck.Count);
            Assert.IsTrue(ContainsCardIdPrefix(localPlayer.hand, "LOCAL-MAIN-"));
            Assert.AreEqual(45, opponent.deck.Count);
            Assert.AreEqual(5, opponent.hand.Count);
            Assert.AreEqual(4, opponent.ride_deck.Count);
            AssertAllHidden(opponent.deck, 1);
            AssertAllHidden(opponent.hand, 1);
            AssertAllHidden(opponent.ride_deck, 1);

            string sessionJson = result.session.ToJson(false);
            Assert.IsFalse(sessionJson.Contains("OPP-MAIN-"));
            Assert.IsFalse(sessionJson.Contains("OPP-RIDE-"));
            Assert.AreEqual(0, result.session.public_event_log.Count);
            Assert.AreEqual(0, result.session.local_private_event_log.Count);
            Assert.AreEqual(0, localTrueState.event_log.Count);
        }

        [Test]
        public void CreatesPlayerTwoLocalSessionWithOpponentAsPlaceholder()
        {
            MultiplayerRoomState room = CreateCommitmentRoom();
            room.host_player_id = "p1";
            room.players.Add(CommittedOpponent("p1"));
            VanguardDeck localDeck = CreateDeck("P2LOCAL");
            room.players.Add(CommittedPlayer("p2", localDeck, "p2-nonce", room));

            OwnerPrivateRoomInitializationResult result =
                OwnerPrivateRoomInitializer.Create(room, localDeck, "p2", "p2-nonce");

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.session.local_player_index);
            Assert.IsTrue(ContainsCardIdPrefix(result.session.local_true_state.GetPlayer(1).hand, "P2LOCAL-MAIN-"));
            AssertAllHidden(result.session.local_true_state.GetPlayer(0).deck, 0);
            AssertAllHidden(result.session.opponent_public_view.GetPlayer(0).hand, 0, false);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, result.session.opponent_public_view.GetPlayer(1).deck[0].card_id);
            Assert.AreNotEqual(GameStateViewFactory.HiddenCardId, result.session.opponent_public_view.GetPlayer(1).hand[0].card_id);
        }

        [Test]
        public void RejectsCommitmentMismatchWithoutMutatingInputs()
        {
            MultiplayerRoomState room = CreateCommitmentRoom();
            VanguardDeck localDeck = CreateDeck("LOCAL");
            room.players.Add(CommittedPlayer("p1", localDeck, "local-nonce", room));
            room.players.Add(CommittedOpponent("p2"));
            string roomBefore = room.ToJson(false);
            string deckBefore = localDeck.ToJson(false);

            OwnerPrivateRoomInitializationResult result =
                OwnerPrivateRoomInitializer.Create(room, localDeck, "p1", "wrong-nonce");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("LOCAL_DECK_COMMITMENT_MISMATCH", result.rejection_reason);
            Assert.IsNull(result.session);
            Assert.AreEqual(roomBefore, room.ToJson(false));
            Assert.AreEqual(deckBefore, localDeck.ToJson(false));
        }

        [Test]
        public void RejectsOpponentMissingPublicCountMetadata()
        {
            MultiplayerRoomState room = CreateCommitmentRoom();
            VanguardDeck localDeck = CreateDeck("LOCAL");
            room.players.Add(CommittedPlayer("p1", localDeck, "local-nonce", room));
            RoomPlayerInfo opponent = CommittedOpponent("p2");
            opponent.main_deck_count = 0;
            room.players.Add(opponent);

            OwnerPrivateRoomInitializationResult result =
                OwnerPrivateRoomInitializer.Create(room, localDeck, "p1", "local-nonce");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("OPPONENT_MAIN_DECK_COUNT_MISSING", result.rejection_reason);
        }

        private static MultiplayerRoomState CreateCommitmentRoom()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-OWNER-PRIVATE",
                "D",
                "p1",
                7301,
                new PackSyncInfo
                {
                    pack_id = "vanguard_th",
                    source_version = "test",
                    definition_hash = "pack-hash",
                    image_manifest_hash = "image-manifest",
                    image_content_hash = "image-content"
                });
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.room_visibility = RoomVisibilityModes.Public;
            return room;
        }

        private static RoomPlayerInfo CommittedPlayer(
            string playerId,
            VanguardDeck deck,
            string nonce,
            MultiplayerRoomState room)
        {
            return new RoomPlayerInfo
            {
                player_id = playerId,
                display_name = playerId,
                deck_commitment = DeckCommitmentService.CreateCommitment(
                    deck,
                    nonce,
                    room.room_id,
                    room.pack.definition_hash),
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                deck_reveal_policy = "end_of_match",
                main_deck_count = deck.TotalCards(DeckZone.Main),
                ride_deck_count = deck.TotalCards(DeckZone.Ride),
                g_deck_count = deck.TotalCards(DeckZone.G),
                opening_hand_count = GameStateFactory.OpeningHandSize,
                connected = true
            };
        }

        private static RoomPlayerInfo CommittedOpponent(string playerId)
        {
            return new RoomPlayerInfo
            {
                player_id = playerId,
                display_name = playerId,
                deck_commitment = playerId + "-commitment",
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                deck_reveal_policy = "end_of_match",
                main_deck_count = 50,
                ride_deck_count = 4,
                g_deck_count = 0,
                opening_hand_count = GameStateFactory.OpeningHandSize,
                connected = true
            };
        }

        private static VanguardDeck CreateDeck(string prefix)
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

        private static bool ContainsCardIdPrefix(System.Collections.Generic.IEnumerable<GameCardInstance> cards, string prefix)
        {
            foreach (GameCardInstance card in cards)
            {
                if (card != null && (card.card_id ?? "").StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertAllHidden(
            System.Collections.Generic.IEnumerable<GameCardInstance> cards,
            int ownerIndex,
            bool requireOwnerPrivateInstancePrefix = true)
        {
            foreach (GameCardInstance card in cards)
            {
                Assert.NotNull(card);
                Assert.AreEqual(GameStateViewFactory.HiddenCardId, card.card_id);
                Assert.AreEqual(ownerIndex, card.owner_index);
                Assert.IsFalse(card.face_up);
                if (requireOwnerPrivateInstancePrefix)
                {
                    Assert.IsTrue((card.instance_id ?? "").StartsWith("owner-private-hidden-p" + ownerIndex, StringComparison.Ordinal));
                }
                else
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(card.instance_id));
                }
            }
        }
    }
}
