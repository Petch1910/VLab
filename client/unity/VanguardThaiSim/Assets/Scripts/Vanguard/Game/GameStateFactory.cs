using System;
using System.Collections.Generic;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Game
{
    public static class GameStateFactory
    {
        public const int OpeningHandSize = 5;

        public static GameState CreateTwoPlayerGame(
            VanguardDeck playerOneDeck,
            VanguardDeck playerTwoDeck,
            int randomSeed,
            string playerOneId = "player-1",
            string playerTwoId = "player-2")
        {
            if (playerOneDeck == null)
            {
                throw new ArgumentNullException(nameof(playerOneDeck));
            }

            if (playerTwoDeck == null)
            {
                throw new ArgumentNullException(nameof(playerTwoDeck));
            }

            SeededRandomService random = new SeededRandomService(randomSeed);
            GameState state = new GameState
            {
                game_id = Guid.NewGuid().ToString("N"),
                format = string.IsNullOrWhiteSpace(playerOneDeck.format) ? playerTwoDeck.format : playerOneDeck.format,
                random_seed = randomSeed,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan
            };

            state.players.Add(CreatePlayerState(playerOneId, playerOneDeck, 0, random));
            state.players.Add(CreatePlayerState(playerTwoId, playerTwoDeck, 1, random));
            return state;
        }

        private static PlayerGameState CreatePlayerState(string playerId, VanguardDeck deck, int ownerIndex, SeededRandomService random)
        {
            PlayerGameState player = new PlayerGameState
            {
                player_id = playerId
            };

            AddDeckEntries(player.deck, deck.GetEntries(DeckZone.Main), ownerIndex, "main");
            AddDeckEntries(player.ride_deck, deck.GetEntries(DeckZone.Ride), ownerIndex, "ride");
            AddDeckEntries(player.g_zone, deck.GetEntries(DeckZone.G), ownerIndex, "g");
            random.Shuffle(player.deck);
            DrawOpeningHand(player);
            return player;
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
                    string instanceId = ownerIndex + "-" + zonePrefix + "-" + entry.card_id + "-" + copyIndex;
                    target.Add(new GameCardInstance(instanceId, entry.card_id, ownerIndex, true));
                    copyIndex++;
                }
            }
        }

        private static void DrawOpeningHand(PlayerGameState player)
        {
            int drawCount = Math.Min(OpeningHandSize, player.deck.Count);
            for (int i = 0; i < drawCount; i++)
            {
                GameCardInstance card = player.deck[0];
                player.deck.RemoveAt(0);
                player.hand.Add(card);
            }
        }

    }
}
