using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class GameEventReducer
    {
        public static void Apply(GameState state, GameEvent gameEvent, bool appendToLog)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            state.EnsureLists();
            switch (gameEvent.action_type)
            {
                case GameActionType.Draw:
                case GameActionType.MoveCard:
                    ApplyMove(state, gameEvent);
                    break;
                case GameActionType.SetPhase:
                    state.phase = gameEvent.new_phase;
                    break;
                case GameActionType.AddGiftMarker:
                    state.GetPlayer(gameEvent.actor_index).AddGiftMarker(gameEvent.gift_marker_type, gameEvent.marker_delta);
                    break;
                case GameActionType.ResourceFlip:
                    ApplyResourceFlip(state, gameEvent);
                    break;
                case GameActionType.DeclareAttack:
                    ApplyDeclareAttack(state, gameEvent);
                    break;
                case GameActionType.Guard:
                    ApplyGuard(state, gameEvent);
                    break;
                case GameActionType.TriggerCheck:
                    ApplyTriggerCheck(state, gameEvent);
                    break;
                case GameActionType.MulliganCards:
                    ApplyMulligan(state, gameEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (appendToLog)
            {
                state.event_log.Add(gameEvent);
            }
        }

        public static bool UndoLast(GameState state)
        {
            state.EnsureLists();
            if (state.event_log.Count == 0)
            {
                return false;
            }

            GameEvent gameEvent = state.event_log[state.event_log.Count - 1];
            switch (gameEvent.action_type)
            {
                case GameActionType.Draw:
                case GameActionType.MoveCard:
                    UndoMove(state, gameEvent);
                    break;
                case GameActionType.SetPhase:
                    state.phase = gameEvent.previous_phase;
                    break;
                case GameActionType.AddGiftMarker:
                    state.GetPlayer(gameEvent.actor_index).AddGiftMarker(gameEvent.gift_marker_type, -gameEvent.marker_delta);
                    break;
                case GameActionType.ResourceFlip:
                    UndoResourceFlip(state, gameEvent);
                    break;
                case GameActionType.DeclareAttack:
                    UndoDeclareAttack(state, gameEvent);
                    break;
                case GameActionType.Guard:
                    UndoGuard(state, gameEvent);
                    break;
                case GameActionType.TriggerCheck:
                    UndoTriggerCheck(state, gameEvent);
                    break;
                case GameActionType.MulliganCards:
                    UndoMulligan(state, gameEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            state.event_log.RemoveAt(state.event_log.Count - 1);
            return true;
        }

        private static void ApplyDeclareAttack(GameState state, GameEvent gameEvent)
        {
            state.attacker_card_instance_id = gameEvent.card_instance_id;
            state.target_card_instance_id = gameEvent.target_card_instance_id;
        }

        private static void UndoDeclareAttack(GameState state, GameEvent gameEvent)
        {
            state.attacker_card_instance_id = null;
            state.target_card_instance_id = null;
        }

        private static void ApplyGuard(GameState state, GameEvent gameEvent)
        {
            ApplyMove(state, gameEvent);
            if (state.guardian_card_instance_ids == null)
            {
                state.guardian_card_instance_ids = new List<string>();
            }
            if (!state.guardian_card_instance_ids.Contains(gameEvent.card_instance_id))
            {
                state.guardian_card_instance_ids.Add(gameEvent.card_instance_id);
            }
        }

        private static void UndoGuard(GameState state, GameEvent gameEvent)
        {
            UndoMove(state, gameEvent);
            if (state.guardian_card_instance_ids != null)
            {
                state.guardian_card_instance_ids.Remove(gameEvent.card_instance_id);
            }
        }

        private static void ApplyTriggerCheck(GameState state, GameEvent gameEvent)
        {
            ApplyMove(state, gameEvent);
        }

        private static void UndoTriggerCheck(GameState state, GameEvent gameEvent)
        {
            UndoMove(state, gameEvent);
        }

        private static void ApplyMulligan(GameState state, GameEvent gameEvent)
        {
            PlayerGameState player = state.GetPlayer(gameEvent.actor_index);
            if (gameEvent.pre_mulligan_hand_ids == null)
            {
                gameEvent.pre_mulligan_hand_ids = new List<string>();
                foreach (GameCardInstance c in player.hand)
                {
                    gameEvent.pre_mulligan_hand_ids.Add(c.instance_id);
                }
            }
            if (gameEvent.pre_mulligan_deck_ids == null)
            {
                gameEvent.pre_mulligan_deck_ids = new List<string>();
                foreach (GameCardInstance c in player.deck)
                {
                    gameEvent.pre_mulligan_deck_ids.Add(c.instance_id);
                }
            }

            foreach (string cardId in gameEvent.card_instance_ids)
            {
                int index = FindCardIndex(player.hand, cardId);
                if (index >= 0)
                {
                    GameCardInstance card = player.hand[index];
                    player.hand.RemoveAt(index);
                    player.deck.Add(card);
                }
            }

            SeededRandomService random = new SeededRandomService(state.random_seed + state.event_log.Count);
            random.Shuffle(player.deck);

            int drawCount = gameEvent.card_instance_ids.Count;
            int actualDraw = Math.Min(drawCount, player.deck.Count);
            for (int i = 0; i < actualDraw; i++)
            {
                GameCardInstance card = player.deck[0];
                player.deck.RemoveAt(0);
                player.hand.Add(card);
            }
        }

        private static void UndoMulligan(GameState state, GameEvent gameEvent)
        {
            PlayerGameState player = state.GetPlayer(gameEvent.actor_index);
            List<GameCardInstance> pool = new List<GameCardInstance>();
            pool.AddRange(player.hand);
            pool.AddRange(player.deck);

            player.hand.Clear();
            player.deck.Clear();

            foreach (string id in gameEvent.pre_mulligan_hand_ids)
            {
                int idx = FindCardIndex(pool, id);
                if (idx >= 0)
                {
                    player.hand.Add(pool[idx]);
                    pool.RemoveAt(idx);
                }
            }

            foreach (string id in gameEvent.pre_mulligan_deck_ids)
            {
                int idx = FindCardIndex(pool, id);
                if (idx >= 0)
                {
                    player.deck.Add(pool[idx]);
                    pool.RemoveAt(idx);
                }
            }
        }

        private static void ApplyMove(GameState state, GameEvent gameEvent)
        {
            PlayerGameState player = state.GetPlayer(gameEvent.actor_index);
            List<GameCardInstance> from = player.GetZone(gameEvent.from_zone);
            List<GameCardInstance> to = player.GetZone(gameEvent.to_zone);
            int fromIndex = FindCardIndex(from, gameEvent.card_instance_id);
            if (fromIndex < 0)
            {
                throw new InvalidOperationException("Card instance is not in the expected source zone.");
            }

            GameCardInstance card = from[fromIndex];
            from.RemoveAt(fromIndex);
            if (gameEvent.to_zone == GameZone.Vanguard)
            {
                MoveDisplacedVanguardsToSoul(player, gameEvent);
            }

            int insertIndex = ClampInsertIndex(to, gameEvent.to_index);
            to.Insert(insertIndex, card);
            gameEvent.from_index = fromIndex;
            gameEvent.to_index = insertIndex;
        }

        private static void UndoMove(GameState state, GameEvent gameEvent)
        {
            PlayerGameState player = state.GetPlayer(gameEvent.actor_index);
            List<GameCardInstance> from = player.GetZone(gameEvent.from_zone);
            List<GameCardInstance> to = player.GetZone(gameEvent.to_zone);
            int currentIndex = FindCardIndex(to, gameEvent.card_instance_id);
            if (currentIndex < 0)
            {
                throw new InvalidOperationException("Cannot undo because the moved card is no longer in the destination zone.");
            }

            GameCardInstance card = to[currentIndex];
            to.RemoveAt(currentIndex);
            int insertIndex = ClampInsertIndex(from, gameEvent.from_index);
            from.Insert(insertIndex, card);
            if (gameEvent.to_zone == GameZone.Vanguard)
            {
                RestoreDisplacedVanguardsFromSoul(player, gameEvent);
            }
        }

        private static void MoveDisplacedVanguardsToSoul(PlayerGameState player, GameEvent gameEvent)
        {
            if (gameEvent.card_instance_ids == null)
            {
                gameEvent.card_instance_ids = new List<string>();
                for (int i = 0; i < player.vanguard.Count; i++)
                {
                    if (player.vanguard[i] != null && !string.IsNullOrEmpty(player.vanguard[i].instance_id))
                    {
                        gameEvent.card_instance_ids.Add(player.vanguard[i].instance_id);
                    }
                }
            }

            for (int i = 0; i < gameEvent.card_instance_ids.Count; i++)
            {
                string displacedId = gameEvent.card_instance_ids[i];
                int index = FindCardIndex(player.vanguard, displacedId);
                if (index < 0)
                {
                    continue;
                }

                GameCardInstance displaced = player.vanguard[index];
                player.vanguard.RemoveAt(index);
                player.soul.Add(displaced);
            }
        }

        private static void RestoreDisplacedVanguardsFromSoul(PlayerGameState player, GameEvent gameEvent)
        {
            if (gameEvent.card_instance_ids == null)
            {
                return;
            }

            for (int i = 0; i < gameEvent.card_instance_ids.Count; i++)
            {
                string displacedId = gameEvent.card_instance_ids[i];
                int index = FindCardIndex(player.soul, displacedId);
                if (index < 0)
                {
                    continue;
                }

                GameCardInstance displaced = player.soul[index];
                player.soul.RemoveAt(index);
                int insertIndex = ClampInsertIndex(player.vanguard, i);
                player.vanguard.Insert(insertIndex, displaced);
            }
        }

        private static int FindCardIndex(List<GameCardInstance> cards, string cardInstanceId)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].instance_id == cardInstanceId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int ClampInsertIndex(List<GameCardInstance> cards, int index)
        {
            if (index < 0 || index > cards.Count)
            {
                return cards.Count;
            }

            return index;
        }

        private static void ApplyResourceFlip(GameState state, GameEvent gameEvent)
        {
            GameCardInstance card = FindCard(
                state.GetPlayer(gameEvent.actor_index).damage,
                gameEvent.card_instance_id);
            card.face_up = gameEvent.new_face_up;
        }

        private static void UndoResourceFlip(GameState state, GameEvent gameEvent)
        {
            GameCardInstance card = FindCard(
                state.GetPlayer(gameEvent.actor_index).damage,
                gameEvent.card_instance_id);
            card.face_up = gameEvent.previous_face_up;
        }

        private static GameCardInstance FindCard(List<GameCardInstance> cards, string cardInstanceId)
        {
            int index = FindCardIndex(cards, cardInstanceId);
            if (index < 0)
            {
                throw new InvalidOperationException("Card instance is not in the expected zone.");
            }

            return cards[index];
        }
    }
}
