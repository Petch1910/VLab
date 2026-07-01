using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableGuidedNextActionFormatterTests
    {
        [TearDown]
        public void TearDown()
        {
            PlayTableBootstrap[] tables = Object.FindObjectsByType<PlayTableBootstrap>(FindObjectsInactive.Include);
            for (int i = 0; i < tables.Length; i++)
            {
                if (tables[i] != null)
                {
                    Object.DestroyImmediate(tables[i].gameObject);
                }
            }
        }

        [Test]
        public void FormatGuidesFirstVanguardWithoutHiddenLeak()
        {
            GameState state = CreateState();
            GameState view = GameStateViewFactory.CreatePlayerView(state, 0);

            string hint = PlayTableGuidedNextActionFormatter.Format(view, 0, true);

            Assert.AreEqual("Next: choose a Ride Deck card, then press VG for first Vanguard.", hint);
            Assert.IsFalse(hint.Contains("p2-MAIN"));
            Assert.IsFalse(hint.Contains("p2-RIDE"));
        }

        [Test]
        public void FormatGuidesMainPhaseCallAndBattle()
        {
            GameState state = CreateState();
            SetFirstVanguard(state, 0);
            GameActionService.SetPhase(state, 0, GamePhase.Main);

            string hint = PlayTableGuidedNextActionFormatter.Format(
                GameStateViewFactory.CreatePlayerView(state, 0),
                0,
                true);

            Assert.AreEqual("Next: select a hand card, then press Rear to call, or press Battle.", hint);
        }

        [Test]
        public void FormatGuidesBattleAttackAndLocalSeatHandoff()
        {
            GameState state = CreateState();
            SetFirstVanguard(state, 0);
            SetFirstVanguard(state, 1);
            GameActionService.SetPhase(state, 0, GamePhase.Battle);

            string ready = PlayTableGuidedNextActionFormatter.Format(
                GameStateViewFactory.CreatePlayerView(state, 0),
                0,
                true);
            Assert.AreEqual("Next: select attacker, then press Atk VG or choose an opponent target.", ready);

            GameActionService.DeclareAttack(
                state,
                0,
                state.GetPlayer(0).vanguard[0].instance_id,
                state.GetPlayer(1).vanguard[0].instance_id);

            string attackerHint = PlayTableGuidedNextActionFormatter.Format(
                GameStateViewFactory.CreatePlayerView(state, 0),
                0,
                true);
            string defenderHint = PlayTableGuidedNextActionFormatter.Format(
                GameStateViewFactory.CreatePlayerView(state, 1),
                1,
                true);

            Assert.AreEqual("Next: switch to P2 for guard or damage check.", attackerHint);
            Assert.AreEqual(
                "Next: select a hand card and press Guard, or press Damage Check if taking the hit.",
                defenderHint);
        }

        [Test]
        public void FormatGuidesGuardAndCheckSequence()
        {
            GameState state = CreateBattleAfterAttack();
            GameActionService.Guard(state, 1, state.GetPlayer(1).hand[0].instance_id);

            string defenderAfterGuard = PlayTableGuidedNextActionFormatter.Format(
                GameStateViewFactory.CreatePlayerView(state, 1),
                1,
                true);
            Assert.AreEqual("Next: switch to P1 for Drive Check.", defenderAfterGuard);

            GameActionService.TriggerCheck(state, 0, GameZone.Deck, GameZone.Trigger, TriggerCheckSource.Drive);

            string defenderAfterDrive = PlayTableGuidedNextActionFormatter.Format(
                GameStateViewFactory.CreatePlayerView(state, 1),
                1,
                true);
            Assert.AreEqual(
                "Next: press Damage Check if the attack hits, or resolve battle manually.",
                defenderAfterDrive);
        }

        [Test]
        public void PlayTableShowsGuidedNextActionSurface()
        {
            PlayTableBootstrap.Show(CreateDeck("p1"), CreateDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject guideObject = GameObject.Find("PlayTable Guided Next Action");

            Assert.NotNull(table);
            Assert.NotNull(guideObject);
            Text guideText = guideObject.GetComponent<Text>();
            Assert.NotNull(guideText);
            Assert.AreEqual(table.CreateGuidedNextAction(), guideText.text);
            Assert.IsTrue(guideText.text.Contains("Ride Deck"));
        }

        [Test]
        public void PlayTableShowsActionGroupLegendSurface()
        {
            PlayTableBootstrap.Show(CreateDeck("p1"), CreateDeck("p2"));
            GameObject legendObject = GameObject.Find("PlayTable Action Group Legend");

            Assert.NotNull(legendObject);
            Text legendText = legendObject.GetComponent<Text>();
            Assert.NotNull(legendText);
            Assert.AreEqual(PlayTableActionGroupLegendFormatter.Format(), legendText.text);
            Assert.IsTrue(legendText.text.Contains("Turn:"));
            Assert.IsTrue(legendText.text.Contains("Card:"));
            Assert.IsTrue(legendText.text.Contains("Battle:"));
        }

        [Test]
        public void PlayTableKeepsBotPlanInsideAdvancedDrawer()
        {
            PlayTableBootstrap.Show(CreateDeck("p1"), CreateDeck("p2"));
            GameObject drawerObject = GameObject.Find("Advanced Drawer");
            GameObject botObject = GameObject.Find("PlayTable Bot Explanation");

            Assert.NotNull(drawerObject);
            Assert.NotNull(botObject);
            Assert.AreEqual(drawerObject.transform, botObject.transform.parent);
        }

        private static GameState CreateBattleAfterAttack()
        {
            GameState state = CreateState();
            SetFirstVanguard(state, 0);
            SetFirstVanguard(state, 1);
            GameActionService.SetPhase(state, 0, GamePhase.Battle);
            GameActionService.DeclareAttack(
                state,
                0,
                state.GetPlayer(0).vanguard[0].instance_id,
                state.GetPlayer(1).vanguard[0].instance_id);
            return state;
        }

        private static void SetFirstVanguard(GameState state, int playerIndex)
        {
            PlayerGameState player = state.GetPlayer(playerIndex);
            GameActionService.MoveCard(
                state,
                playerIndex,
                player.ride_deck[0].instance_id,
                GameZone.RideDeck,
                GameZone.Vanguard);
        }

        private static GameState CreateState()
        {
            return GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 2805);
        }

        private static VanguardDeck CreateDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " Guided Deck", "D", "vanguard_th", "m28-05");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i.ToString("00"), 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i.ToString("00"), 1);
            }

            return deck;
        }
    }
}
