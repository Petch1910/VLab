using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ManualScreenOverlayTests
    {
        [Test]
        public void CreateBuildsScrollableManualOverlayAndCloseCallback()
        {
            GameObject host = new GameObject("Manual Overlay Test Host");
            host.AddComponent<RectTransform>();
            bool closed = false;

            try
            {
                GameObject overlay = ManualScreenOverlay.Create(
                    host.transform,
                    Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                    delegate { closed = true; });

                Assert.NotNull(overlay);
                Assert.AreEqual("Manual Screen", overlay.name);

                Text body = FindChild(overlay.transform, "Manual Body").GetComponent<Text>();
                StringAssert.Contains("App Guide", body.text);
                StringAssert.Contains("Vanguard Rules Basics", body.text);

                Button close = FindChild(overlay.transform, "Manual Close Button").GetComponent<Button>();
                close.onClick.Invoke();
                Assert.IsTrue(closed);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void ManualOverlaySearchAndCategoryControlsFilterBody()
        {
            GameObject host = new GameObject("Manual Filter Overlay Test Host");
            host.AddComponent<RectTransform>();

            try
            {
                GameObject overlay = ManualScreenOverlay.Create(
                    host.transform,
                    Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                    delegate { });
                Text body = FindChild(overlay.transform, "Manual Body").GetComponent<Text>();
                InputField searchInput = FindChild(overlay.transform, "Manual Search Input").GetComponent<InputField>();
                Button categoryButton = FindChild(overlay.transform, "Manual Category Button").GetComponent<Button>();

                searchInput.text = "trigger";
                searchInput.onValueChanged.Invoke(searchInput.text);
                StringAssert.Contains("Triggers", body.text);
                Assert.IsFalse(body.text.Contains("Home is the starting point"));

                searchInput.text = string.Empty;
                searchInput.onValueChanged.Invoke(searchInput.text);
                categoryButton.onClick.Invoke();
                Assert.IsTrue(body.text.Contains("App Guide") || body.text.Contains("Vanguard Rules Basics"));
                Assert.IsFalse(body.text.Contains(ManualContentFilter.EmptyResultMessage));

                searchInput.text = "no-such-manual-topic";
                searchInput.onValueChanged.Invoke(searchInput.text);
                Assert.AreEqual(ManualContentFilter.EmptyResultMessage, body.text);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void HomeManualButtonOpensManualScreen()
        {
            GameObject host = new GameObject("Home Manual Bootstrap Test");
            try
            {
                HomeLobbyBootstrap bootstrap = host.AddComponent<HomeLobbyBootstrap>();
                typeof(HomeLobbyBootstrap)
                    .GetMethod("Start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(bootstrap, null);

                Button manualButton = FindChild(host.transform, "Manual Button").GetComponent<Button>();
                manualButton.onClick.Invoke();

                GameObject manualScreen = FindChild(host.transform, "Manual Screen");
                Assert.NotNull(manualScreen);
                Assert.IsTrue(manualScreen.activeSelf);
                StringAssert.Contains(
                    "PlayTable",
                    FindChild(manualScreen.transform, "Manual Body").GetComponent<Text>().text);

                Button closeButton = FindChild(manualScreen.transform, "Manual Close Button").GetComponent<Button>();
                closeButton.onClick.Invoke();
                Assert.IsFalse(manualScreen.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(host);
                DestroyEventSystem();
            }
        }

        [Test]
        public void PlayTableManualButtonOpensManualScreen()
        {
            VanguardDeck deck = CreateDeck("Manual UI Deck");
            PlayTableBootstrap.Show(deck, VanguardDeck.FromJson(deck.ToJson(false)));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            try
            {
                Assert.NotNull(table);
                Button manualButton = FindChild(table.transform, "Manual Button").GetComponent<Button>();
                manualButton.onClick.Invoke();

                GameObject manualScreen = FindChild(table.transform, "Manual Screen");
                Assert.NotNull(manualScreen);
                Assert.IsTrue(manualScreen.activeSelf);
                StringAssert.Contains(
                    "Vanguard Rules Basics",
                    FindChild(manualScreen.transform, "Manual Body").GetComponent<Text>().text);

                Button closeButton = FindChild(manualScreen.transform, "Manual Close Button").GetComponent<Button>();
                closeButton.onClick.Invoke();
                Assert.IsFalse(manualScreen.activeSelf);
            }
            finally
            {
                if (table != null)
                {
                    Object.DestroyImmediate(table.gameObject);
                }

                DestroyEventSystem();
            }
        }

        private static VanguardDeck CreateDeck(string name)
        {
            VanguardDeck deck = VanguardDeck.Create(name, "D", "pack", "version");
            for (int i = 1; i <= 10; i++)
            {
                deck.AddCard(DeckZone.Main, "CARD-" + i.ToString("D3"), 1);
            }

            deck.AddCard(DeckZone.Ride, "RIDE-001", 1);
            return deck;
        }

        private static void DestroyEventSystem()
        {
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }
        }

        private static GameObject FindChild(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject found = FindChild(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
