using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableSetupStatusFormatter
    {
        public const string NoStateMessage = "Setup: no game loaded.";
        public const string InvalidPlayerMessage = "Setup: player state is not available.";
        public const string ChooseFirstVanguardMessage =
            "Setup: choose a Ride Deck card, then press VG for first Vanguard.";
        public const string MissingFirstVanguardMessage =
            "Setup: first Vanguard is missing. Ride Deck has no visible card to place.";
        public const string MulliganReadyMessage =
            "Setup: first Vanguard ready. Mulligan selected hand cards or press Stand.";
        public const string ReadyToStandMessage =
            "Setup: first Vanguard ready. Press Stand to begin.";
        public const string MissingVanguardAfterSetupMessage =
            "Setup warning: Vanguard is empty. Return to setup or place a Vanguard manually.";

        public static string Format(GameState state, int playerIndex)
        {
            if (state == null)
            {
                return NoStateMessage;
            }

            PlayerGameState player = GetPlayerOrNull(state, playerIndex);
            if (player == null)
            {
                return InvalidPlayerMessage;
            }

            int vanguardCount = Count(player.vanguard);
            int rideDeckCount = Count(player.ride_deck);
            int handCount = Count(player.hand);

            if (state.phase == GamePhase.Mulligan)
            {
                if (vanguardCount <= 0 && rideDeckCount > 0)
                {
                    return ChooseFirstVanguardMessage;
                }

                if (vanguardCount <= 0)
                {
                    return MissingFirstVanguardMessage;
                }

                return handCount > 0 ? MulliganReadyMessage : ReadyToStandMessage;
            }

            if (vanguardCount <= 0)
            {
                return MissingVanguardAfterSetupMessage;
            }

            return "Setup complete: " + FormatPhase(state.phase) + " phase.";
        }

        private static PlayerGameState GetPlayerOrNull(GameState state, int playerIndex)
        {
            if (state.players == null || playerIndex < 0 || playerIndex >= state.players.Count)
            {
                return null;
            }

            return state.players[playerIndex];
        }

        private static int Count(IReadOnlyCollection<GameCardInstance> cards)
        {
            return cards == null ? 0 : cards.Count;
        }

        private static string FormatPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.StandAndDraw:
                    return "Stand & Draw";
                case GamePhase.Ride:
                    return "Ride";
                case GamePhase.Main:
                    return "Main";
                case GamePhase.Battle:
                    return "Battle";
                case GamePhase.End:
                    return "End";
                case GamePhase.Mulligan:
                    return "Mulligan";
                default:
                    return phase.ToString();
            }
        }
    }
}
