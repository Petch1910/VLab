using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableGuidedNextActionFormatter
    {
        public const string NoStateMessage = "Next: load a game.";
        public const string InvalidSeatMessage = "Next: choose a valid player seat.";

        public static string Format(GameState state, int playerIndex, bool canSwitchLocalSeat)
        {
            if (state == null)
            {
                return NoStateMessage;
            }

            PlayerGameState player = GetPlayerOrNull(state, playerIndex);
            if (player == null)
            {
                return InvalidSeatMessage;
            }

            if (state.phase == GamePhase.Battle)
            {
                string battleHint = FormatBattleHint(state, playerIndex, canSwitchLocalSeat);
                if (!string.IsNullOrEmpty(battleHint))
                {
                    return battleHint;
                }
            }

            if (state.phase != GamePhase.Mulligan &&
                state.turn_player_index >= 0 &&
                state.turn_player_index < CountPlayers(state) &&
                state.turn_player_index != playerIndex)
            {
                return canSwitchLocalSeat
                    ? "Next: press Seat P" + (state.turn_player_index + 1) + " to continue the turn."
                    : "Next: wait for P" + (state.turn_player_index + 1) + " to act.";
            }

            switch (state.phase)
            {
                case GamePhase.Mulligan:
                    return FormatMulliganHint(player);
                case GamePhase.StandAndDraw:
                    return player.deck.Count > 0
                        ? "Next: press Draw, then press Ride."
                        : "Next: press Ride; deck is empty for draw.";
                case GamePhase.Ride:
                    return player.hand.Count > 0
                        ? "Next: select a hand card, then press VG to ride, or press Main."
                        : "Next: press Main; no hand card is available to ride.";
                case GamePhase.Main:
                    return player.hand.Count > 0
                        ? "Next: select a hand card, then press Rear to call, or press Battle."
                        : "Next: press Battle, or use manual zone actions if needed.";
                case GamePhase.Battle:
                    return FormatReadyToAttackHint(state, playerIndex);
                case GamePhase.End:
                    return canSwitchLocalSeat
                        ? "Next: resolve end cleanup manually, then switch seats for the next turn."
                        : "Next: resolve end cleanup manually and wait for the next turn.";
                default:
                    return "Next: use legal action buttons for the current phase.";
            }
        }

        private static string FormatMulliganHint(PlayerGameState player)
        {
            if (player.vanguard.Count <= 0 && player.ride_deck.Count > 0)
            {
                return "Next: choose a Ride Deck card, then press VG for first Vanguard.";
            }

            if (player.vanguard.Count <= 0)
            {
                return "Next: place a Vanguard manually before starting.";
            }

            return player.hand.Count > 0
                ? "Next: mulligan selected hand cards, or press Stand to keep hand."
                : "Next: press Stand to begin.";
        }

        private static string FormatBattleHint(GameState state, int playerIndex, bool canSwitchLocalSeat)
        {
            GameEvent latest = LatestEvent(state.event_log);
            if (latest == null)
            {
                return FormatReadyToAttackHint(state, playerIndex);
            }

            if (latest.action_type == GameActionType.DeclareAttack)
            {
                int defenderIndex = OpponentOf(latest.actor_index, state);
                if (playerIndex == defenderIndex)
                {
                    return "Next: select a hand card and press Guard, or press Damage Check if taking the hit.";
                }

                if (playerIndex == latest.actor_index)
                {
                    return canSwitchLocalSeat && defenderIndex >= 0
                        ? "Next: switch to P" + (defenderIndex + 1) + " for guard or damage check."
                        : "Next: wait for opponent guard or damage check.";
                }
            }

            if (latest.action_type == GameActionType.Guard)
            {
                int attackerIndex = OpponentOf(latest.actor_index, state);
                if (playerIndex == attackerIndex)
                {
                    return "Next: press Drive, then resolve damage manually if the attack hits.";
                }

                return canSwitchLocalSeat && attackerIndex >= 0
                    ? "Next: switch to P" + (attackerIndex + 1) + " for Drive Check."
                    : "Next: wait for attacker Drive Check.";
            }

            if (latest.action_type == GameActionType.TriggerCheck)
            {
                if (latest.trigger_check_source == TriggerCheckSource.Drive)
                {
                    int defenderIndex = OpponentOf(latest.actor_index, state);
                    if (playerIndex == defenderIndex)
                    {
                        return "Next: press Damage Check if the attack hits, or resolve battle manually.";
                    }

                    return canSwitchLocalSeat && defenderIndex >= 0
                        ? "Next: switch to P" + (defenderIndex + 1) + " for Damage Check if needed."
                        : "Next: wait for damage check or manual battle resolution.";
                }

                if (latest.trigger_check_source == TriggerCheckSource.Damage)
                {
                    return "Next: resolve trigger effects manually, then press End when battle is finished.";
                }

                return "Next: resolve trigger effects manually.";
            }

            return FormatReadyToAttackHint(state, playerIndex);
        }

        private static string FormatReadyToAttackHint(GameState state, int playerIndex)
        {
            PlayerGameState player = GetPlayerOrNull(state, playerIndex);
            int opponentIndex = OpponentOf(playerIndex, state);
            PlayerGameState opponent = GetPlayerOrNull(state, opponentIndex);
            bool hasAttacker = HasCards(player == null ? null : player.vanguard) ||
                HasCards(player == null ? null : player.rear_guard);
            bool hasTarget = HasCards(opponent == null ? null : opponent.vanguard) ||
                HasCards(opponent == null ? null : opponent.rear_guard);

            if (hasAttacker && hasTarget)
            {
                return "Next: select attacker, then press Atk VG or choose an opponent target.";
            }

            return "Next: resolve checks manually or press End when battle is finished.";
        }

        private static PlayerGameState GetPlayerOrNull(GameState state, int playerIndex)
        {
            if (state == null || state.players == null || playerIndex < 0 || playerIndex >= state.players.Count)
            {
                return null;
            }

            PlayerGameState player = state.players[playerIndex];
            player?.EnsureLists();
            return player;
        }

        private static int CountPlayers(GameState state)
        {
            return state == null || state.players == null ? 0 : state.players.Count;
        }

        private static int OpponentOf(int playerIndex, GameState state)
        {
            if (playerIndex < 0 || CountPlayers(state) < 2)
            {
                return -1;
            }

            return playerIndex == 0 ? 1 : 0;
        }

        private static bool HasCards(IReadOnlyCollection<GameCardInstance> cards)
        {
            return cards != null && cards.Count > 0;
        }

        private static GameEvent LatestEvent(IReadOnlyList<GameEvent> events)
        {
            if (events == null || events.Count == 0)
            {
                return null;
            }

            return events[events.Count - 1];
        }
    }
}
