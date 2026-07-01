using System;
using System.Collections.Generic;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class OwnerPrivateRoomInitializer
    {
        public const string ReconnectBlockedReason =
            "OWNER_PRIVATE_RECONNECT_REQUIRES_PUBLIC_EVENT_REPLAY_AND_LOCAL_SNAPSHOT";

        public static OwnerPrivateRoomInitializationResult Create(
            MultiplayerRoomState room,
            VanguardDeck localDeck,
            string localPlayerId,
            string localRevealNonce)
        {
            if (room == null)
            {
                return OwnerPrivateRoomInitializationResult.Rejected("ROOM_MISSING");
            }

            if (localDeck == null)
            {
                return OwnerPrivateRoomInitializationResult.Rejected("LOCAL_DECK_MISSING");
            }

            room.EnsureLists();
            string privacyMode = string.IsNullOrWhiteSpace(room.deck_privacy_mode)
                ? DeckPrivacyModes.SharedDeckCode
                : room.deck_privacy_mode.Trim();
            if (!string.Equals(privacyMode, DeckPrivacyModes.DeckCommitment, StringComparison.Ordinal))
            {
                return OwnerPrivateRoomInitializationResult.Rejected("DECK_COMMITMENT_PRIVACY_REQUIRED");
            }

            if (string.IsNullOrWhiteSpace(room.room_id))
            {
                return OwnerPrivateRoomInitializationResult.Rejected("ROOM_ID_MISSING");
            }

            if (room.pack == null || string.IsNullOrWhiteSpace(room.pack.definition_hash))
            {
                return OwnerPrivateRoomInitializationResult.Rejected("PACK_DEFINITION_HASH_MISSING");
            }

            if (room.random_seed == 0)
            {
                return OwnerPrivateRoomInitializationResult.Rejected("RANDOM_SEED_MISSING");
            }

            int localIndex = FindPlayerIndex(room.players, localPlayerId);
            if (localIndex < 0)
            {
                return OwnerPrivateRoomInitializationResult.Rejected("LOCAL_PLAYER_MISSING");
            }

            int opponentIndex = FindOpponentIndex(room.players, localIndex);
            if (opponentIndex < 0)
            {
                return OwnerPrivateRoomInitializationResult.Rejected("OPPONENT_PLAYER_MISSING");
            }

            RoomPlayerInfo localPlayer = room.players[localIndex];
            RoomPlayerInfo opponent = room.players[opponentIndex];
            string rejectionReason;
            if (!ValidateCommittedPlayer(localPlayer, "LOCAL", out rejectionReason))
            {
                return OwnerPrivateRoomInitializationResult.Rejected(rejectionReason);
            }

            if (!ValidateCommittedPlayer(opponent, "OPPONENT", out rejectionReason))
            {
                return OwnerPrivateRoomInitializationResult.Rejected(rejectionReason);
            }

            if (!ValidateOpponentCounts(opponent, out rejectionReason))
            {
                return OwnerPrivateRoomInitializationResult.Rejected(rejectionReason);
            }

            if (!DeckCommitmentService.VerifyCommitment(
                    localDeck,
                    localRevealNonce,
                    room.room_id,
                    room.pack.definition_hash,
                    localPlayer.deck_commitment))
            {
                return OwnerPrivateRoomInitializationResult.Rejected("LOCAL_DECK_COMMITMENT_MISMATCH");
            }

            GameState localTrueState = CreateLocalTrueState(room, localDeck, localIndex);
            GameState publicView = GameStateViewFactory.CreatePlayerView(localTrueState, localIndex);
            publicView.event_log.Clear();

            LocalOwnerPrivateSession session = new LocalOwnerPrivateSession
            {
                room_id = room.room_id,
                local_player_id = localPlayer.player_id,
                local_player_index = localIndex,
                event_cursor = 0,
                reconnect_enabled = false,
                reconnect_block_reason = ReconnectBlockedReason,
                local_true_state = localTrueState,
                opponent_public_view = publicView
            };
            session.EnsureLists();
            return OwnerPrivateRoomInitializationResult.Accepted(session);
        }

        private static bool ValidateCommittedPlayer(RoomPlayerInfo player, string prefix, out string rejectionReason)
        {
            rejectionReason = null;
            if (player == null || string.IsNullOrWhiteSpace(player.player_id))
            {
                rejectionReason = prefix + "_PLAYER_MISSING";
                return false;
            }

            if (!player.connected)
            {
                rejectionReason = prefix + "_PLAYER_NOT_CONNECTED";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(player.deck_code))
            {
                rejectionReason = prefix + "_DECK_CODE_FORBIDDEN_FOR_COMMITMENT";
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.deck_commitment))
            {
                rejectionReason = prefix + "_DECK_COMMITMENT_MISSING";
                return false;
            }

            if (!string.Equals(player.deck_commitment_algorithm ?? "", DeckCommitmentService.Algorithm, StringComparison.Ordinal))
            {
                rejectionReason = prefix + "_DECK_COMMITMENT_ALGORITHM_MISMATCH";
                return false;
            }

            return true;
        }

        private static bool ValidateOpponentCounts(RoomPlayerInfo opponent, out string rejectionReason)
        {
            rejectionReason = null;
            if (opponent.main_deck_count <= 0)
            {
                rejectionReason = "OPPONENT_MAIN_DECK_COUNT_MISSING";
                return false;
            }

            int openingHandCount = NormalizeOpeningHandCount(opponent.opening_hand_count, opponent.main_deck_count);
            if (openingHandCount < 0 || openingHandCount > opponent.main_deck_count)
            {
                rejectionReason = "OPPONENT_OPENING_HAND_COUNT_INVALID";
                return false;
            }

            if (opponent.ride_deck_count < 0 || opponent.g_deck_count < 0)
            {
                rejectionReason = "OPPONENT_DECK_COUNT_INVALID";
                return false;
            }

            return true;
        }

        private static GameState CreateLocalTrueState(MultiplayerRoomState room, VanguardDeck localDeck, int localIndex)
        {
            GameState state = new GameState
            {
                game_id = room.room_id,
                format = string.IsNullOrWhiteSpace(room.format) ? localDeck.format : room.format,
                random_seed = room.random_seed,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan
            };

            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (i == localIndex)
                {
                    state.players.Add(CreateLocalPlayerState(player.player_id, localDeck, i, room.random_seed));
                }
                else
                {
                    state.players.Add(CreateOpponentPlaceholderState(player, i));
                }
            }

            state.EnsureLists();
            return state;
        }

        private static PlayerGameState CreateLocalPlayerState(
            string playerId,
            VanguardDeck deck,
            int ownerIndex,
            int roomSeed)
        {
            PlayerGameState player = new PlayerGameState
            {
                player_id = playerId
            };

            AddDeckEntries(player.deck, deck.GetEntries(DeckZone.Main), ownerIndex, "main");
            AddDeckEntries(player.ride_deck, deck.GetEntries(DeckZone.Ride), ownerIndex, "ride");
            SeededRandomService random = new SeededRandomService(DerivePlayerSeed(roomSeed, ownerIndex));
            random.Shuffle(player.deck);
            DrawOpeningHand(player);
            return player;
        }

        private static PlayerGameState CreateOpponentPlaceholderState(RoomPlayerInfo playerInfo, int ownerIndex)
        {
            PlayerGameState player = new PlayerGameState
            {
                player_id = playerInfo == null ? "" : playerInfo.player_id
            };

            int mainDeckCount = Math.Max(0, playerInfo == null ? 0 : playerInfo.main_deck_count);
            int openingHandCount = NormalizeOpeningHandCount(
                playerInfo == null ? 0 : playerInfo.opening_hand_count,
                mainDeckCount);
            int deckCountAfterOpeningHand = Math.Max(0, mainDeckCount - openingHandCount);
            AddHiddenCards(player.deck, ownerIndex, GameZone.Deck, deckCountAfterOpeningHand);
            AddHiddenCards(player.hand, ownerIndex, GameZone.Hand, openingHandCount);
            AddHiddenCards(player.ride_deck, ownerIndex, GameZone.RideDeck, Math.Max(0, playerInfo == null ? 0 : playerInfo.ride_deck_count));
            return player;
        }

        private static int FindPlayerIndex(IReadOnlyList<RoomPlayerInfo> players, string playerId)
        {
            if (players == null || string.IsNullOrWhiteSpace(playerId))
            {
                return -1;
            }

            for (int i = 0; i < players.Count; i++)
            {
                RoomPlayerInfo player = players[i];
                if (player != null && string.Equals(player.player_id ?? "", playerId ?? "", StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindOpponentIndex(IReadOnlyList<RoomPlayerInfo> players, int localIndex)
        {
            if (players == null)
            {
                return -1;
            }

            for (int i = 0; i < players.Count; i++)
            {
                if (i != localIndex && players[i] != null && players[i].connected)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void AddDeckEntries(
            List<GameCardInstance> target,
            IReadOnlyList<DeckCardEntry> entries,
            int ownerIndex,
            string zonePrefix)
        {
            int copyIndex = 0;
            foreach (DeckCardEntry entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.card_id) || entry.quantity <= 0)
                {
                    continue;
                }

                for (int i = 0; i < entry.quantity; i++)
                {
                    string instanceId = "owner-private-p" + ownerIndex + "-" + zonePrefix + "-" + entry.card_id + "-" + copyIndex;
                    target.Add(new GameCardInstance(instanceId, entry.card_id, ownerIndex, true));
                    copyIndex++;
                }
            }
        }

        private static void AddHiddenCards(List<GameCardInstance> target, int ownerIndex, GameZone zone, int count)
        {
            for (int i = 0; i < count; i++)
            {
                string instanceId = "owner-private-hidden-p" +
                    ownerIndex +
                    "-" +
                    zone.ToString().ToLowerInvariant() +
                    "-" +
                    i.ToString("D4");
                target.Add(new GameCardInstance(instanceId, GameStateViewFactory.HiddenCardId, ownerIndex, false));
            }
        }

        private static void DrawOpeningHand(PlayerGameState player)
        {
            int drawCount = Math.Min(GameStateFactory.OpeningHandSize, player.deck.Count);
            for (int i = 0; i < drawCount; i++)
            {
                GameCardInstance card = player.deck[0];
                player.deck.RemoveAt(0);
                player.hand.Add(card);
            }
        }

        private static int NormalizeOpeningHandCount(int configuredOpeningHandCount, int mainDeckCount)
        {
            if (configuredOpeningHandCount > 0)
            {
                return configuredOpeningHandCount;
            }

            return Math.Min(GameStateFactory.OpeningHandSize, Math.Max(0, mainDeckCount));
        }

        private static int DerivePlayerSeed(int roomSeed, int playerIndex)
        {
            unchecked
            {
                return (roomSeed * 397) ^ ((playerIndex + 1) * 7919);
            }
        }
    }
}
