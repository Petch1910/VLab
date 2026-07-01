using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class GameStateViewTests
    {
        [Test]
        public void PlayerViewKeepsOwnHandButMasksOpponentHandAndAllDecks()
        {
            GameState state = CreateState(610);
            string ownHandCardId = state.GetPlayer(0).hand[0].card_id;
            string ownDeckInstanceId = state.GetPlayer(0).deck[0].instance_id;
            string opponentHandInstanceId = state.GetPlayer(1).hand[0].instance_id;

            GameState view = GameStateViewFactory.CreatePlayerView(state, 0);

            Assert.AreEqual(ownHandCardId, view.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(0).deck[0].card_id);
            Assert.AreNotEqual(ownDeckInstanceId, view.GetPlayer(0).deck[0].instance_id);
            Assert.IsFalse(view.GetPlayer(0).deck[0].face_up);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(1).hand[0].card_id);
            Assert.AreNotEqual(opponentHandInstanceId, view.GetPlayer(1).hand[0].instance_id);
            Assert.IsFalse(view.GetPlayer(1).hand[0].face_up);

            Assert.AreEqual(5, view.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(45, view.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(5, view.GetPlayer(1).CountZone(GameZone.Hand));
            Assert.AreEqual(opponentHandInstanceId, state.GetPlayer(1).hand[0].instance_id);
        }

        [Test]
        public void SpectatorViewMasksBothHandsAndDecksButKeepsFaceUpPublicCards()
        {
            GameState state = CreateState(611);
            string vanguardInstanceId = state.GetPlayer(0).hand[0].instance_id;
            string vanguardCardId = state.GetPlayer(0).hand[0].card_id;
            GameActionService.MoveCard(state, 0, vanguardInstanceId, GameZone.Hand, GameZone.Vanguard);

            GameState view = GameStateViewFactory.CreateSpectatorView(state);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(1).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(0).deck[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(1).deck[0].card_id);
            Assert.AreEqual(vanguardCardId, view.GetPlayer(0).vanguard[0].card_id);
            Assert.AreEqual(vanguardInstanceId, view.GetPlayer(0).vanguard[0].instance_id);
            Assert.IsTrue(view.GetPlayer(0).vanguard[0].face_up);
        }

        [Test]
        public void PlayerViewMasksFaceDownCardsInPublicZones()
        {
            GameState state = CreateState(612);
            GameCardInstance hiddenBind = state.GetPlayer(1).hand[0];
            state.GetPlayer(1).hand.RemoveAt(0);
            hiddenBind.face_up = false;
            state.GetPlayer(1).bind.Add(hiddenBind);

            GameState view = GameStateViewFactory.CreatePlayerView(state, 0);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(1).bind[0].card_id);
            Assert.AreNotEqual(hiddenBind.instance_id, view.GetPlayer(1).bind[0].instance_id);
            Assert.IsFalse(view.GetPlayer(1).bind[0].face_up);
        }

        [Test]
        public void PlayerViewMasksOpponentPrivateEventCardIds()
        {
            GameState state = CreateState(613);
            string opponentDrawInstanceId = state.GetPlayer(1).deck[0].instance_id;

            GameActionService.Draw(state, 1);
            GameState view = GameStateViewFactory.CreatePlayerView(state, 0);

            Assert.AreEqual(opponentDrawInstanceId, state.event_log[0].card_instance_id);
            Assert.AreNotEqual(opponentDrawInstanceId, view.event_log[0].card_instance_id);
            Assert.IsTrue(view.event_log[0].card_instance_id.StartsWith("hidden-"));
        }

        [Test]
        public void ReplayPlayerReturnsMaskedCurrentStateView()
        {
            GameState initial = CreateState(614);
            GameState live = GameState.FromJson(initial.ToJson(false));
            GameActionService.Draw(live, 1);

            GameReplay replay = GameReplay.Create(initial, live.event_log);
            GameReplayPlayer player = new GameReplayPlayer(replay);
            player.JumpToEnd();

            GameState view = player.CreateCurrentStateView(GameStateViewPerspective.Spectator);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(1).hand[0].card_id);
            Assert.IsTrue(view.event_log[0].card_instance_id.StartsWith("hidden-"));
        }

        private static GameState CreateState(int seed)
        {
            return GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), seed);
        }

        private static VanguardDeck CreateSampleDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "test");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            return deck;
        }
    }
}
