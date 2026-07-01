using System.Collections;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace VanguardThaiSim.PlayModeTests
{
    public sealed class WindowsPlayableLoopPlayModeTests
    {
        [TearDown]
        public void TearDown()
        {
            DestroyAll<HomeLobbyBootstrap>();
            DestroyAll<CardBrowserBootstrap>();
            DestroyAll<PlayTableBootstrap>();
        }

        [UnityTest]
        public IEnumerator HomeToPlayTableDrawRideEndFlowHasNoRuntimeExceptions()
        {
            TearDown();
            yield return null;

            HomeLobbyBootstrap.Show();
            yield return null;
            Assert.NotNull(Object.FindAnyObjectByType<HomeLobbyBootstrap>());

            PlayTableBootstrap.Show(
                CreateSmokeDeck("p1"),
                CreateSmokeDeck("p2"),
                "M27-06 PlayMode integration");
            yield return null;

            Assert.NotNull(Object.FindAnyObjectByType<PlayTableBootstrap>());
            AssertTextContains("Local manual table");

            ClickButton("Stand");
            yield return null;
            AssertTextContains("Phase StandAndDraw");

            ClickButton("Draw");
            yield return null;
            AssertTextContains("drew 1 card");

            ClickButton("Ride");
            yield return null;
            AssertTextContains("Phase Ride");

            ClickButton("End");
            yield return null;
            AssertTextContains("Phase End");
        }

        [UnityTest]
        public IEnumerator LocalTwoSeatMatchSmokeUsesRuntimeUiControls()
        {
            TearDown();
            yield return null;

            PlayTableBootstrap.Show(
                CreateSmokeDeck("p1"),
                CreateSmokeDeck("p2"),
                "M28-03 two-seat UI smoke");
            yield return null;

            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            Assert.NotNull(table);
            Assert.AreEqual(0, table.CurrentPlayerIndex);

            ClickVisibleCard(table, 0, GameZone.RideDeck);
            yield return null;
            ClickButton("VG");
            yield return null;
            Assert.AreEqual(1, table.CreateDisplayStateView().GetPlayer(0).CountZone(GameZone.Vanguard));

            ClickButton("Seat P2");
            yield return null;
            Assert.AreEqual(1, table.CurrentPlayerIndex);
            ClickVisibleCard(table, 1, GameZone.RideDeck);
            yield return null;
            ClickButton("VG");
            yield return null;
            Assert.AreEqual(1, table.CreateDisplayStateView().GetPlayer(1).CountZone(GameZone.Vanguard));

            ClickButton("Seat P1");
            yield return null;
            Assert.AreEqual(0, table.CurrentPlayerIndex);

            ClickButton("Stand");
            yield return null;
            ClickButton("Draw");
            yield return null;
            ClickButton("Ride");
            yield return null;
            ClickVisibleCard(table, 0, GameZone.Hand);
            yield return null;
            ClickButton("VG");
            yield return null;
            Assert.GreaterOrEqual(table.CreateDisplayStateView().GetPlayer(0).CountZone(GameZone.Soul), 1);

            ClickButton("Main");
            yield return null;
            ClickVisibleCard(table, 0, GameZone.Hand);
            yield return null;
            ClickButton("Rear");
            yield return null;
            Assert.GreaterOrEqual(table.CreateDisplayStateView().GetPlayer(0).CountZone(GameZone.RearGuard), 1);

            ClickButton("Battle");
            yield return null;
            ClickVisibleCard(table, 0, GameZone.Vanguard);
            yield return null;
            ClickButton("Atk VG");
            yield return null;
            AssertTextContains("declared an attack");

            ClickButton("Seat P2");
            yield return null;
            Assert.AreEqual(1, table.CurrentPlayerIndex);
            ClickVisibleCard(table, 1, GameZone.Hand);
            yield return null;
            ClickButton("Guard");
            yield return null;
            Assert.GreaterOrEqual(table.CreateDisplayStateView().GetPlayer(1).CountZone(GameZone.Guardian), 1);
            ClickButton("Damage Check");
            yield return null;
            Assert.GreaterOrEqual(table.CreateDisplayStateView().GetPlayer(1).CountZone(GameZone.Trigger), 1);

            ClickButton("Seat P1");
            yield return null;
            Assert.AreEqual(0, table.CurrentPlayerIndex);
            ClickButton("Drive");
            yield return null;
            Assert.GreaterOrEqual(table.CreateDisplayStateView().GetPlayer(0).CountZone(GameZone.Trigger), 1);
            ClickButton("End");
            yield return null;

            AssertTextContains("Phase End");
            Assert.GreaterOrEqual(table.CreateDisplayStateView().event_log.Count, 12);
        }

        private static VanguardDeck CreateSmokeDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(
                prefix + " PlayMode Deck",
                "D",
                "vanguard_th",
                "m27-06");

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

        private static void ClickButton(string label)
        {
            Button button = FindButton(label);
            Assert.NotNull(button, "Button not found: " + label);
            Assert.IsTrue(button.interactable, "Button is not interactable: " + label);
            button.onClick.Invoke();
        }

        private static void ClickVisibleCard(PlayTableBootstrap table, int playerIndex, GameZone zone)
        {
            GameState view = table.CreateDisplayStateView();
            Assert.NotNull(view);
            PlayerGameState player = view.GetPlayer(playerIndex);
            Assert.Greater(player.GetZone(zone).Count, 0, "No visible cards in " + zone + " for P" + (playerIndex + 1));
            string cardId = player.GetZone(zone)[0].card_id;
            Assert.AreNotEqual(GameStateViewFactory.HiddenCardId, cardId, "Card is hidden in " + zone + " for P" + (playerIndex + 1));

            Button button = FindCardButton(cardId);
            Assert.NotNull(button, "Card button not found for " + cardId + " in " + zone);
            Assert.IsTrue(button.interactable, "Card button is not interactable for " + cardId);
            button.onClick.Invoke();
        }

        private static Button FindCardButton(string cardId)
        {
            GameObject namedButton = GameObject.Find(cardId + " Button");
            if (namedButton != null)
            {
                Button named = namedButton.GetComponent<Button>();
                if (named != null)
                {
                    return named;
                }
            }

            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            for (int i = 0; i < buttons.Length; i++)
            {
                Text[] texts = buttons[i].GetComponentsInChildren<Text>(true);
                for (int j = 0; j < texts.Length; j++)
                {
                    if (texts[j] != null &&
                        !string.IsNullOrEmpty(texts[j].text) &&
                        texts[j].text.Contains(cardId))
                    {
                        return buttons[i];
                    }
                }
            }

            return null;
        }

        private static Button FindButton(string label)
        {
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            for (int i = 0; i < buttons.Length; i++)
            {
                Text text = buttons[i].GetComponentInChildren<Text>(true);
                if (text != null && text.text == label)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private static void AssertTextContains(string expected)
        {
            Text[] texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && !string.IsNullOrEmpty(texts[i].text) && texts[i].text.Contains(expected))
                {
                    return;
                }
            }

            Assert.Fail("Text not found: " + expected);
        }

        private static void DestroyAll<T>() where T : Object
        {
            T[] objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    Object.Destroy(objects[i] is Component component ? component.gameObject : objects[i]);
                }
            }
        }
    }
}
