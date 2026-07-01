using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableCommonActionAvailabilityTests
    {
        [Test]
        public void StandAndDrawPhaseEnablesDrawButNotGiftMarkers()
        {
            GameState state = CreateState(GamePhase.StandAndDraw);

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, string.Empty, GameZone.Hand, false);

            Assert.IsTrue(availability.can_draw);
            Assert.IsFalse(availability.can_add_force);
            Assert.IsFalse(availability.can_add_accel);
            Assert.IsFalse(availability.can_add_protect);
            Assert.IsTrue(availability.can_set_main);
        }

        [Test]
        public void MulliganPhaseEnablesStandAndRidePhaseButtons()
        {
            GameState state = CreateState(GamePhase.Mulligan);

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, string.Empty, GameZone.Hand, false);

            Assert.IsTrue(availability.can_set_stand_and_draw);
            Assert.IsTrue(availability.can_set_ride);
            Assert.IsTrue(availability.can_set_main);
        }

        [Test]
        public void MainPhaseSelectedHandCardEnablesMoveAndGiftButNotDraw()
        {
            GameState state = CreateState(GamePhase.Main);
            string selected = state.GetPlayer(0).hand[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.Hand, false);

            Assert.IsFalse(availability.can_draw);
            Assert.IsTrue(availability.can_move_to_vanguard);
            Assert.IsTrue(availability.can_move_to_rear_guard);
            Assert.IsTrue(availability.can_move_to_drop);
            Assert.IsTrue(availability.can_move_to_damage);
            Assert.IsTrue(availability.can_add_force);
            Assert.IsTrue(availability.can_add_accel);
            Assert.IsTrue(availability.can_add_protect);
        }

        [Test]
        public void RidePhaseSelectedHandCardEnablesOnlyVanguardMove()
        {
            GameState state = CreateState(GamePhase.Ride);
            string selected = state.GetPlayer(0).hand[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.Hand, false);

            Assert.IsTrue(availability.can_move_to_vanguard);
            Assert.IsFalse(availability.can_move_to_rear_guard);
            Assert.IsFalse(availability.can_move_to_drop);
            Assert.IsFalse(availability.can_move_to_damage);
            Assert.IsFalse(availability.can_add_force);
        }

        [Test]
        public void MulliganSelectedRideDeckCardEnablesFirstVanguardMove()
        {
            GameState state = CreateState(GamePhase.Mulligan);
            state.GetPlayer(0).vanguard.Clear();
            string selected = state.GetPlayer(0).ride_deck[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.RideDeck, false);

            Assert.IsTrue(availability.can_move_to_vanguard);
            Assert.IsFalse(availability.can_move_to_rear_guard);
            Assert.IsFalse(availability.can_move_to_drop);
            Assert.IsFalse(availability.can_move_to_damage);
        }

        [Test]
        public void MulliganSelectedHandCardDoesNotEnableVanguardMove()
        {
            GameState state = CreateState(GamePhase.Mulligan);
            state.GetPlayer(0).vanguard.Clear();
            string selected = state.GetPlayer(0).hand[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.Hand, false);

            Assert.IsFalse(availability.can_move_to_vanguard);
        }

        [Test]
        public void MulliganSelectedHandCardEnablesMulliganOnly()
        {
            GameState state = CreateState(GamePhase.Mulligan);
            string selected = state.GetPlayer(0).hand[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.Hand, false);

            Assert.IsTrue(availability.can_mulligan_selected);
            Assert.IsFalse(availability.can_guard_selected);
        }

        [Test]
        public void MulliganSelectedRideDeckCardDoesNotEnableMulligan()
        {
            GameState state = CreateState(GamePhase.Mulligan);
            state.GetPlayer(0).vanguard.Clear();
            string selected = state.GetPlayer(0).ride_deck[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.RideDeck, false);

            Assert.IsFalse(availability.can_mulligan_selected);
        }

        [Test]
        public void NoSelectionDisablesMoveButtons()
        {
            GameState state = CreateState(GamePhase.Main);

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, string.Empty, GameZone.Hand, false);

            Assert.IsFalse(availability.can_move_to_vanguard);
            Assert.IsFalse(availability.can_move_to_rear_guard);
            Assert.IsFalse(availability.can_move_to_drop);
            Assert.IsFalse(availability.can_move_to_damage);
        }

        [Test]
        public void OnlineRoomDisablesUndoEvenWhenEventLogExists()
        {
            GameState state = CreateState(GamePhase.Main);
            state.event_log.Add(new GameEvent { action_type = GameActionType.Draw });

            PlayTableCommonActionAvailability local =
                PlayTableCommonActionAvailability.FromState(state, 0, string.Empty, GameZone.Hand, false);
            PlayTableCommonActionAvailability online =
                PlayTableCommonActionAvailability.FromState(state, 0, string.Empty, GameZone.Hand, true);

            Assert.IsTrue(local.can_undo);
            Assert.IsFalse(online.can_undo);
        }

        [Test]
        public void BattlePhaseEnablesTriggerCheckAndSelectedGuard()
        {
            GameState state = CreateState(GamePhase.Battle);
            string selected = state.GetPlayer(0).hand[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.Hand, false);

            Assert.IsTrue(availability.can_trigger_check);
            Assert.IsTrue(availability.can_drive_check);
            Assert.IsTrue(availability.can_damage_check);
            Assert.IsTrue(availability.can_guard_selected);
            Assert.IsFalse(availability.can_attack_vanguard_selected);
            Assert.IsFalse(availability.can_move_to_vanguard);
            Assert.IsFalse(availability.can_add_force);
        }

        [Test]
        public void BattlePhaseSelectedRearGuardEnablesAttackVanguard()
        {
            GameState state = CreateState(GamePhase.Battle);
            string selected = state.GetPlayer(0).rear_guard[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(state, 0, selected, GameZone.RearGuard, false);

            Assert.IsTrue(availability.can_attack_vanguard_selected);
            Assert.IsFalse(availability.can_attack_selected_target);
            Assert.IsFalse(availability.can_guard_selected);
        }

        [Test]
        public void BattlePhaseSelectedRearGuardAndOpponentTargetEnablesAttackTarget()
        {
            GameState state = CreateState(GamePhase.Battle);
            string selected = state.GetPlayer(0).rear_guard[0].instance_id;
            string target = state.GetPlayer(1).rear_guard[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(
                    state,
                    0,
                    selected,
                    GameZone.RearGuard,
                    target,
                    false);

            Assert.IsTrue(availability.can_attack_selected_target);
            Assert.IsTrue(availability.can_attack_vanguard_selected);
        }

        [Test]
        public void BattlePhaseSelectedRearGuardWithInvalidTargetDisablesAttackTarget()
        {
            GameState state = CreateState(GamePhase.Battle);
            string selected = state.GetPlayer(0).rear_guard[0].instance_id;

            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(
                    state,
                    0,
                    selected,
                    GameZone.RearGuard,
                    "missing-target",
                    false);

            Assert.IsFalse(availability.can_attack_selected_target);
        }

        private static GameState CreateState(GamePhase phase)
        {
            var state = new GameState
            {
                phase = phase,
                turn_number = 1,
                turn_player_index = 0
            };
            state.players.Add(CreatePlayer("p1"));
            state.players.Add(CreatePlayer("p2"));
            return state;
        }

        private static PlayerGameState CreatePlayer(string playerId)
        {
            var player = new PlayerGameState { player_id = playerId };
            player.deck.Add(new GameCardInstance(playerId + "-deck-1", "CARD-001", 0, false));
            player.hand.Add(new GameCardInstance(playerId + "-hand-1", "CARD-002", 1, true));
            player.ride_deck.Add(new GameCardInstance(playerId + "-ride-1", "CARD-000", 0, true));
            player.vanguard.Add(new GameCardInstance(playerId + "-vg-1", "CARD-003", 2, true));
            player.rear_guard.Add(new GameCardInstance(playerId + "-rg-1", "CARD-004", 3, true));
            return player;
        }
    }
}
