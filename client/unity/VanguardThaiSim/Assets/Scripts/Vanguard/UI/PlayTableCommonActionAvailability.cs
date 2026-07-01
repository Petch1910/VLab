using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public sealed class PlayTableCommonActionAvailability
    {
        public bool can_draw;
        public bool can_set_stand_and_draw;
        public bool can_set_ride;
        public bool can_set_main;
        public bool can_set_battle;
        public bool can_set_end;
        public bool can_move_to_vanguard;
        public bool can_move_to_rear_guard;
        public bool can_move_to_drop;
        public bool can_move_to_damage;
        public bool can_drive_check;
        public bool can_damage_check;
        public bool can_trigger_check;
        public bool can_guard_selected;
        public bool can_mulligan_selected;
        public bool can_attack_vanguard_selected;
        public bool can_attack_selected_target;
        public bool can_add_force;
        public bool can_add_accel;
        public bool can_add_protect;
        public bool can_undo;

        public static PlayTableCommonActionAvailability FromState(
            GameState state,
            int playerIndex,
            string selectedCardInstanceId,
            GameZone selectedZone,
            bool onlineRoom)
        {
            return FromState(
                state,
                playerIndex,
                selectedCardInstanceId,
                selectedZone,
                string.Empty,
                onlineRoom);
        }

        public static PlayTableCommonActionAvailability FromState(
            GameState state,
            int playerIndex,
            string selectedCardInstanceId,
            GameZone selectedZone,
            string selectedTargetCardInstanceId,
            bool onlineRoom)
        {
            var availability = new PlayTableCommonActionAvailability();
            if (state == null)
            {
                return availability;
            }

            IReadOnlyList<LegalGameAction> legalActions = RulesCore.GetLegalActions(state, playerIndex);
            availability.can_draw =
                IsMatrixLegal(state.phase, PhaseTimingMatrixCommandIds.Draw, TimingWindow.OnDraw) &&
                HasAction(legalActions, GameActionType.Draw);

            availability.can_set_stand_and_draw = HasPhaseAction(legalActions, GamePhase.StandAndDraw);
            availability.can_set_ride = HasPhaseAction(legalActions, GamePhase.Ride);
            availability.can_set_main = HasPhaseAction(legalActions, GamePhase.Main);
            availability.can_set_battle = HasPhaseAction(legalActions, GamePhase.Battle);
            availability.can_set_end = HasPhaseAction(legalActions, GamePhase.End);

            availability.can_move_to_vanguard = CanMoveToTargetInPhase(state.phase, selectedZone, GameZone.Vanguard) &&
                HasMoveAction(legalActions, selectedCardInstanceId, selectedZone, GameZone.Vanguard);
            availability.can_move_to_rear_guard = CanMoveToTargetInPhase(state.phase, selectedZone, GameZone.RearGuard) &&
                HasMoveAction(legalActions, selectedCardInstanceId, selectedZone, GameZone.RearGuard);
            availability.can_move_to_drop = CanMoveToTargetInPhase(state.phase, selectedZone, GameZone.Drop) &&
                HasMoveAction(legalActions, selectedCardInstanceId, selectedZone, GameZone.Drop);
            availability.can_move_to_damage = CanMoveToTargetInPhase(state.phase, selectedZone, GameZone.Damage) &&
                HasMoveAction(legalActions, selectedCardInstanceId, selectedZone, GameZone.Damage);

            availability.can_drive_check =
                IsMatrixLegal(state.phase, PhaseTimingMatrixCommandIds.TriggerCheckDrive, TimingWindow.DriveCheck) &&
                HasTriggerCheckAction(legalActions, TriggerCheckSource.Drive);
            availability.can_damage_check =
                IsMatrixLegal(state.phase, PhaseTimingMatrixCommandIds.TriggerCheckDamage, TimingWindow.DamageCheck) &&
                HasTriggerCheckAction(legalActions, TriggerCheckSource.Damage);
            availability.can_trigger_check = availability.can_drive_check || availability.can_damage_check;
            availability.can_guard_selected =
                state.phase == GamePhase.Battle &&
                HasCardAction(legalActions, GameActionType.Guard, selectedCardInstanceId);
            availability.can_mulligan_selected =
                state.phase == GamePhase.Mulligan &&
                selectedZone == GameZone.Hand &&
                HasMulliganAction(legalActions, selectedCardInstanceId);
            availability.can_attack_vanguard_selected =
                state.phase == GamePhase.Battle &&
                HasAttackToOpponentVanguard(state, playerIndex, legalActions, selectedCardInstanceId);
            availability.can_attack_selected_target =
                state.phase == GamePhase.Battle &&
                HasAttackToTarget(legalActions, selectedCardInstanceId, selectedTargetCardInstanceId);

            bool canAddGiftInPhase =
                IsMatrixLegal(state.phase, PhaseTimingMatrixCommandIds.AddGiftMarker, TimingWindow.OnAddGiftMarker);
            availability.can_add_force = canAddGiftInPhase && HasGiftAction(legalActions, GiftMarkerType.Force);
            availability.can_add_accel = canAddGiftInPhase && HasGiftAction(legalActions, GiftMarkerType.Accel);
            availability.can_add_protect = canAddGiftInPhase && HasGiftAction(legalActions, GiftMarkerType.Protect);

            availability.can_undo = !onlineRoom && state.event_log != null && state.event_log.Count > 0;
            return availability;
        }

        private static bool IsMatrixLegal(GamePhase phase, string commandId, TimingWindow timingWindow)
        {
            return PhaseTimingMatrix.IsLegal(
                PhaseTimingMatrix.CreateCurrentMatrix(),
                commandId,
                phase,
                timingWindow);
        }

        private static bool CanMoveToTargetInPhase(GamePhase phase, GameZone sourceZone, GameZone targetZone)
        {
            if (!IsMatrixLegal(phase, PhaseTimingMatrixCommandIds.MoveCard, TimingWindow.OnMoveCard))
            {
                return false;
            }

            if (phase == GamePhase.Mulligan)
            {
                return sourceZone == GameZone.RideDeck && targetZone == GameZone.Vanguard;
            }

            if (phase == GamePhase.Ride)
            {
                return targetZone == GameZone.Vanguard;
            }

            return phase == GamePhase.Main;
        }

        private static bool HasAction(IReadOnlyList<LegalGameAction> legalActions, GameActionType actionType)
        {
            if (legalActions == null)
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null && action.action_type == actionType)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasPhaseAction(IReadOnlyList<LegalGameAction> legalActions, GamePhase phase)
        {
            if (legalActions == null)
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null &&
                    action.action_type == GameActionType.SetPhase &&
                    action.phase == phase)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMoveAction(
            IReadOnlyList<LegalGameAction> legalActions,
            string selectedCardInstanceId,
            GameZone selectedZone,
            GameZone targetZone)
        {
            if (legalActions == null || string.IsNullOrWhiteSpace(selectedCardInstanceId))
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null &&
                    action.action_type == GameActionType.MoveCard &&
                    action.card_instance_id == selectedCardInstanceId &&
                    action.from_zone == selectedZone &&
                    action.to_zone == targetZone)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCardAction(
            IReadOnlyList<LegalGameAction> legalActions,
            GameActionType actionType,
            string selectedCardInstanceId)
        {
            if (legalActions == null || string.IsNullOrWhiteSpace(selectedCardInstanceId))
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null &&
                    action.action_type == actionType &&
                    action.card_instance_id == selectedCardInstanceId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasTriggerCheckAction(
            IReadOnlyList<LegalGameAction> legalActions,
            TriggerCheckSource checkSource)
        {
            if (legalActions == null)
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null &&
                    action.action_type == GameActionType.TriggerCheck &&
                    action.trigger_check_source == checkSource)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMulliganAction(
            IReadOnlyList<LegalGameAction> legalActions,
            string selectedCardInstanceId)
        {
            if (legalActions == null || string.IsNullOrWhiteSpace(selectedCardInstanceId))
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action == null ||
                    action.action_type != GameActionType.MulliganCards ||
                    action.card_instance_ids == null ||
                    action.card_instance_ids.Count != 1)
                {
                    continue;
                }

                if (action.card_instance_ids[0] == selectedCardInstanceId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttackToOpponentVanguard(
            GameState state,
            int playerIndex,
            IReadOnlyList<LegalGameAction> legalActions,
            string selectedCardInstanceId)
        {
            if (state == null ||
                legalActions == null ||
                string.IsNullOrWhiteSpace(selectedCardInstanceId))
            {
                return false;
            }

            int opponentIndex = 1 - playerIndex;
            if (opponentIndex < 0 || opponentIndex >= state.players.Count)
            {
                return false;
            }

            PlayerGameState opponent = state.GetPlayer(opponentIndex);
            foreach (LegalGameAction action in legalActions)
            {
                if (action == null ||
                    action.action_type != GameActionType.DeclareAttack ||
                    action.card_instance_id != selectedCardInstanceId)
                {
                    continue;
                }

                if (ContainsCard(opponent.vanguard, action.target_card_instance_id))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttackToTarget(
            IReadOnlyList<LegalGameAction> legalActions,
            string selectedCardInstanceId,
            string selectedTargetCardInstanceId)
        {
            if (legalActions == null ||
                string.IsNullOrWhiteSpace(selectedCardInstanceId) ||
                string.IsNullOrWhiteSpace(selectedTargetCardInstanceId))
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null &&
                    action.action_type == GameActionType.DeclareAttack &&
                    action.card_instance_id == selectedCardInstanceId &&
                    action.target_card_instance_id == selectedTargetCardInstanceId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCard(
            IReadOnlyList<GameCardInstance> cards,
            string cardInstanceId)
        {
            if (cards == null || string.IsNullOrWhiteSpace(cardInstanceId))
            {
                return false;
            }

            foreach (GameCardInstance card in cards)
            {
                if (card != null && card.instance_id == cardInstanceId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasGiftAction(IReadOnlyList<LegalGameAction> legalActions, GiftMarkerType markerType)
        {
            if (legalActions == null)
            {
                return false;
            }

            foreach (LegalGameAction action in legalActions)
            {
                if (action != null &&
                    action.action_type == GameActionType.AddGiftMarker &&
                    action.gift_marker_type == markerType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
