using System;

namespace VanguardThaiSim.Game
{
    public static class GameActionService
    {
        public static GameEvent Draw(GameState state, int playerIndex)
        {
            PlayerGameState player = state.GetPlayer(playerIndex);
            if (player.deck.Count == 0)
            {
                throw new InvalidOperationException("Cannot draw from an empty deck.");
            }

            GameCardInstance card = player.deck[0];
            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.Draw,
                actor_index = playerIndex,
                card_instance_id = card.instance_id,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_index = 0,
                to_index = player.hand.Count
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent MoveCard(
            GameState state,
            int playerIndex,
            string cardInstanceId,
            GameZone fromZone,
            GameZone toZone,
            int toIndex = -1)
        {
            PlayerGameState player = state.GetPlayer(playerIndex);
            if (string.IsNullOrEmpty(cardInstanceId) && fromZone == GameZone.Deck && player.deck.Count > 0)
            {
                cardInstanceId = player.deck[0].instance_id;
            }

            int fromIndex = FindCardIndex(player.GetZone(fromZone), cardInstanceId);
            if (fromIndex < 0)
            {
                throw new InvalidOperationException("Card instance is not in the expected source zone.");
            }

            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.MoveCard,
                actor_index = playerIndex,
                card_instance_id = cardInstanceId,
                from_zone = fromZone,
                to_zone = toZone,
                from_index = fromIndex,
                to_index = toIndex,
                card_instance_ids = toZone == GameZone.Vanguard ? CurrentVanguardIds(player) : null
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent SetPhase(GameState state, int actorIndex, GamePhase phase)
        {
            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.SetPhase,
                actor_index = actorIndex,
                previous_phase = state.phase,
                new_phase = phase
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent AddGiftMarker(GameState state, int playerIndex, GiftMarkerType markerType, int count = 1)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Gift marker count must be greater than zero.");
            }

            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.AddGiftMarker,
                actor_index = playerIndex,
                gift_marker_type = markerType,
                marker_delta = count
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent ResourceFlip(
            GameState state,
            int playerIndex,
            string cardInstanceId,
            GameResourceOperationType operationType)
        {
            if (operationType != GameResourceOperationType.CounterBlast &&
                operationType != GameResourceOperationType.CounterCharge)
            {
                throw new InvalidOperationException("Only CounterBlast and CounterCharge resource flips are event-sourced in the current core.");
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            int index = FindCardIndex(player.damage, cardInstanceId);
            if (index < 0)
            {
                throw new InvalidOperationException("Card instance is not in the expected damage zone.");
            }

            GameCardInstance card = player.damage[index];
            bool expectedFaceUp = operationType == GameResourceOperationType.CounterBlast;
            bool newFaceUp = operationType == GameResourceOperationType.CounterCharge;
            if (card.face_up != expectedFaceUp)
            {
                throw new InvalidOperationException("Damage card face-up state is not legal for " + operationType + ".");
            }

            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.ResourceFlip,
                actor_index = playerIndex,
                card_instance_id = cardInstanceId,
                from_zone = GameZone.Damage,
                to_zone = GameZone.Damage,
                from_index = index,
                to_index = index,
                resource_operation_type = operationType,
                resource_delta = operationType == GameResourceOperationType.CounterBlast ? -1 : 1,
                previous_face_up = card.face_up,
                new_face_up = newFaceUp
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent DeclareAttack(GameState state, int actorIndex, string attackerId, string targetId)
        {
            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.DeclareAttack,
                actor_index = actorIndex,
                card_instance_id = attackerId,
                target_card_instance_id = targetId
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent Guard(GameState state, int actorIndex, string cardId)
        {
            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.Guard,
                actor_index = actorIndex,
                card_instance_id = cardId,
                from_zone = GameZone.Hand,
                to_zone = GameZone.Guardian
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent TriggerCheck(
            GameState state,
            int actorIndex,
            GameZone fromZone,
            GameZone toZone,
            TriggerCheckSource checkSource = TriggerCheckSource.Manual)
        {
            PlayerGameState player = state.GetPlayer(actorIndex);
            if (player.GetZone(fromZone).Count == 0)
            {
                throw new InvalidOperationException("Cannot perform trigger check from empty zone.");
            }
            GameCardInstance card = player.GetZone(fromZone)[0];
            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.TriggerCheck,
                actor_index = actorIndex,
                card_instance_id = card.instance_id,
                from_zone = fromZone,
                to_zone = toZone,
                trigger_check_source = checkSource
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static GameEvent MulliganCards(GameState state, int actorIndex, System.Collections.Generic.List<string> cardIds)
        {
            GameEvent gameEvent = new GameEvent
            {
                event_id = NextEventId(state),
                action_type = GameActionType.MulliganCards,
                actor_index = actorIndex,
                card_instance_ids = cardIds ?? new System.Collections.Generic.List<string>()
            };
            GameEventReducer.Apply(state, gameEvent, true);
            return gameEvent;
        }

        public static bool UndoLast(GameState state)
        {
            return GameEventReducer.UndoLast(state);
        }

        private static int FindCardIndex(System.Collections.Generic.List<GameCardInstance> cards, string cardInstanceId)
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

        private static System.Collections.Generic.List<string> CurrentVanguardIds(PlayerGameState player)
        {
            var ids = new System.Collections.Generic.List<string>();
            for (int i = 0; i < player.vanguard.Count; i++)
            {
                if (player.vanguard[i] != null && !string.IsNullOrEmpty(player.vanguard[i].instance_id))
                {
                    ids.Add(player.vanguard[i].instance_id);
                }
            }

            return ids;
        }

        private static string NextEventId(GameState state)
        {
            state.EnsureLists();
            return "event-" + (state.event_log.Count + 1).ToString("D6");
        }
    }
}
