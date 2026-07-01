using System;

namespace VanguardThaiSim.Game
{
    public static class LegalActionExecutor
    {
        public static GameEvent Execute(GameState state, LegalGameAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            switch (action.action_type)
            {
                case GameActionType.Draw:
                    return GameActionService.Draw(state, action.actor_index);
                case GameActionType.MoveCard:
                    return GameActionService.MoveCard(
                        state,
                        action.actor_index,
                        action.card_instance_id,
                        action.from_zone,
                        action.to_zone);
                case GameActionType.SetPhase:
                    return GameActionService.SetPhase(state, action.actor_index, action.phase);
                case GameActionType.AddGiftMarker:
                    return GameActionService.AddGiftMarker(
                        state,
                        action.actor_index,
                        action.gift_marker_type,
                        action.marker_delta <= 0 ? 1 : action.marker_delta);
                case GameActionType.ResourceFlip:
                    return GameActionService.ResourceFlip(
                        state,
                        action.actor_index,
                        action.card_instance_id,
                        action.resource_operation_type);
                case GameActionType.DeclareAttack:
                    return GameActionService.DeclareAttack(
                        state,
                        action.actor_index,
                        action.card_instance_id,
                        action.target_card_instance_id);
                case GameActionType.Guard:
                    return GameActionService.Guard(
                        state,
                        action.actor_index,
                        action.card_instance_id);
                case GameActionType.TriggerCheck:
                    return GameActionService.TriggerCheck(
                        state,
                        action.actor_index,
                        action.from_zone,
                        action.to_zone,
                        action.trigger_check_source);
                case GameActionType.MulliganCards:
                    return GameActionService.MulliganCards(
                        state,
                        action.actor_index,
                        action.card_instance_ids);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
