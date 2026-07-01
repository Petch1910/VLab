using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class GameState
    {
        public string game_id;
        public string format;
        public int random_seed;
        public int turn_number;
        public int turn_player_index;
        public GamePhase phase;
        public List<PlayerGameState> players = new List<PlayerGameState>();
        public List<GameEvent> event_log = new List<GameEvent>();
        public PendingAutoAbilityQueue pending_auto_abilities = new PendingAutoAbilityQueue();
        public string attacker_card_instance_id;
        public string target_card_instance_id;
        public List<string> guardian_card_instance_ids = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static GameState FromJson(string json)
        {
            GameState state = JsonUtility.FromJson<GameState>(json);
            if (state == null)
            {
                throw new ArgumentException("Game state JSON could not be parsed.", nameof(json));
            }

            state.EnsureLists();
            return state;
        }

        public PlayerGameState GetPlayer(int playerIndex)
        {
            EnsureLists();
            if (playerIndex < 0 || playerIndex >= players.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(playerIndex), playerIndex, null);
            }

            PlayerGameState player = players[playerIndex];
            player.EnsureLists();
            return player;
        }

        public void EnsureLists()
        {
            if (players == null)
            {
                players = new List<PlayerGameState>();
            }

            if (event_log == null)
            {
                event_log = new List<GameEvent>();
            }

            if (pending_auto_abilities == null)
            {
                pending_auto_abilities = new PendingAutoAbilityQueue();
            }

            pending_auto_abilities.EnsureLists();

            if (guardian_card_instance_ids == null)
            {
                guardian_card_instance_ids = new List<string>();
            }

            foreach (PlayerGameState player in players)
            {
                player?.EnsureLists();
            }
        }
    }
}
