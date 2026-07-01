using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableSetupStatusFormatterTests
    {
        [Test]
        public void NullStateFormatsNoGameLoaded()
        {
            Assert.AreEqual(
                PlayTableSetupStatusFormatter.NoStateMessage,
                PlayTableSetupStatusFormatter.Format(null, 0));
        }

        [Test]
        public void InvalidPlayerFormatsUnavailable()
        {
            var state = new GameState { phase = GamePhase.Mulligan };

            Assert.AreEqual(
                PlayTableSetupStatusFormatter.InvalidPlayerMessage,
                PlayTableSetupStatusFormatter.Format(state, 0));
        }

        [Test]
        public void MulliganWithoutVanguardPromptsRideDeckSelection()
        {
            GameState state = CreateState(GamePhase.Mulligan, includeVanguard: false, includeRideDeck: true, includeHand: true);

            Assert.AreEqual(
                PlayTableSetupStatusFormatter.ChooseFirstVanguardMessage,
                PlayTableSetupStatusFormatter.Format(state, 0));
        }

        [Test]
        public void MulliganWithoutVanguardAndRideDeckShowsMissingWarning()
        {
            GameState state = CreateState(GamePhase.Mulligan, includeVanguard: false, includeRideDeck: false, includeHand: true);

            Assert.AreEqual(
                PlayTableSetupStatusFormatter.MissingFirstVanguardMessage,
                PlayTableSetupStatusFormatter.Format(state, 0));
        }

        [Test]
        public void MulliganWithVanguardAndHandPromptsMulliganOrStandWithoutIds()
        {
            GameState state = CreateState(GamePhase.Mulligan, includeVanguard: true, includeRideDeck: true, includeHand: true);

            string formatted = PlayTableSetupStatusFormatter.Format(state, 0);

            Assert.AreEqual(PlayTableSetupStatusFormatter.MulliganReadyMessage, formatted);
            Assert.IsFalse(formatted.Contains("secret-vg-instance"));
            Assert.IsFalse(formatted.Contains("secret-hand-instance"));
        }

        [Test]
        public void MulliganWithNoHandPromptsStand()
        {
            GameState state = CreateState(GamePhase.Mulligan, includeVanguard: true, includeRideDeck: true, includeHand: false);

            Assert.AreEqual(
                PlayTableSetupStatusFormatter.ReadyToStandMessage,
                PlayTableSetupStatusFormatter.Format(state, 0));
        }

        [Test]
        public void StartedGameReportsSetupCompletePhase()
        {
            GameState state = CreateState(GamePhase.Ride, includeVanguard: true, includeRideDeck: true, includeHand: true);

            Assert.AreEqual(
                "Setup complete: Ride phase.",
                PlayTableSetupStatusFormatter.Format(state, 0));
        }

        [Test]
        public void StartedGameWithoutVanguardReportsWarning()
        {
            GameState state = CreateState(GamePhase.Main, includeVanguard: false, includeRideDeck: true, includeHand: true);

            Assert.AreEqual(
                PlayTableSetupStatusFormatter.MissingVanguardAfterSetupMessage,
                PlayTableSetupStatusFormatter.Format(state, 0));
        }

        private static GameState CreateState(
            GamePhase phase,
            bool includeVanguard,
            bool includeRideDeck,
            bool includeHand)
        {
            var state = new GameState { phase = phase, turn_number = 1, turn_player_index = 0 };
            var player = new PlayerGameState { player_id = "p1" };
            if (includeVanguard)
            {
                player.vanguard.Add(new GameCardInstance("secret-vg-instance", "CARD-VG", 0, true));
            }

            if (includeRideDeck)
            {
                player.ride_deck.Add(new GameCardInstance("secret-ride-instance", "CARD-RIDE", 0, true));
            }

            if (includeHand)
            {
                player.hand.Add(new GameCardInstance("secret-hand-instance", "CARD-HAND", 1, true));
            }

            state.players.Add(player);
            return state;
        }
    }
}
