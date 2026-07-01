using System.IO;
using System.Reflection;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class HomeLobbyBootstrapTests
    {
        [Test]
        public void SettingsButtonOpensSessionSettingsPanel()
        {
            GameObject host = new GameObject("Home Lobby Bootstrap Test");
            try
            {
                HomeLobbyBootstrap bootstrap = host.AddComponent<HomeLobbyBootstrap>();
                typeof(HomeLobbyBootstrap)
                    .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(bootstrap, null);

                GameObject settingsScreen = FindChild(host.transform, "Settings Screen");
                Assert.NotNull(settingsScreen);
                Assert.IsFalse(settingsScreen.activeSelf);

                Button settingsButton = FindChild(host.transform, "Settings Button").GetComponent<Button>();
                settingsButton.onClick.Invoke();

                Assert.IsTrue(settingsScreen.activeSelf);
                Text summary = FindChild(settingsScreen.transform, "Multiline Label").GetComponent<Text>();
                Assert.IsTrue(summary.text.Contains("Preferred format: D"));
                Assert.IsTrue(summary.text.Contains("Image cache: Balanced"));

                Button formatButton = FindChild(settingsScreen.transform, "Format Button").GetComponent<Button>();
                formatButton.onClick.Invoke();
                Assert.IsTrue(summary.text.Contains("Preferred format: V"));

                Button closeButton = FindChild(settingsScreen.transform, "Close Button").GetComponent<Button>();
                closeButton.onClick.Invoke();
                Assert.IsFalse(settingsScreen.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(host);
                EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        [Test]
        public void SoloPlayButtonOpensSetupPanelBeforeStartingTable()
        {
            GameObject host = new GameObject("Home Lobby Solo Setup Test");
            try
            {
                HomeLobbyBootstrap bootstrap = host.AddComponent<HomeLobbyBootstrap>();
                typeof(HomeLobbyBootstrap)
                    .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(bootstrap, null);

                GameObject soloSetupScreen = FindChild(host.transform, "Solo Play Setup Screen");
                Assert.NotNull(soloSetupScreen);
                Assert.IsFalse(soloSetupScreen.activeSelf);

                Button soloButton = FindChild(host.transform, "Solo Play Button").GetComponent<Button>();
                soloButton.onClick.Invoke();

                Assert.IsTrue(soloSetupScreen.activeSelf);
                Text difficulty = FindTextContaining(soloSetupScreen.transform, "Difficulty:");
                Text opponent = FindTextContaining(soloSetupScreen.transform, "Bot deck:");
                Assert.NotNull(difficulty);
                Assert.NotNull(opponent);
                Assert.IsTrue(difficulty.text.Contains("Easy"));
                Assert.IsTrue(opponent.text.Contains("Mirror player deck"));

                Button difficultyButton = FindChild(soloSetupScreen.transform, "Difficulty Button").GetComponent<Button>();
                difficultyButton.onClick.Invoke();
                Assert.IsTrue(difficulty.text.Contains("Normal"));

                Button closeButton = FindChild(soloSetupScreen.transform, "Close Button").GetComponent<Button>();
                closeButton.onClick.Invoke();
                Assert.IsFalse(soloSetupScreen.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(host);
                EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        [Test]
        public void ReplayButtonOpensReplayScreenInsteadOfLockedPlaceholder()
        {
            GameObject host = new GameObject("Home Lobby Replay Test");
            try
            {
                HomeLobbyBootstrap bootstrap = host.AddComponent<HomeLobbyBootstrap>();
                typeof(HomeLobbyBootstrap)
                    .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(bootstrap, null);

                GameObject replayScreen = FindChild(host.transform, "Replay Screen");
                Assert.NotNull(replayScreen);
                Assert.IsFalse(replayScreen.activeSelf);

                Button replayButton = FindChild(host.transform, "Replay Button").GetComponent<Button>();
                replayButton.onClick.Invoke();

                Assert.IsTrue(replayScreen.activeSelf);
                Text summary = FindChild(replayScreen.transform, "Replay Summary Text").GetComponent<Text>();
                Assert.NotNull(summary);
                Assert.IsTrue(summary.text.Contains("Replay Library"));
                Assert.IsTrue(summary.text.Contains("no local replay selected"));
                Assert.IsFalse(summary.text.Contains("scheduled in M16"));

                Button closeButton = FindChild(replayScreen.transform, "Close Button").GetComponent<Button>();
                closeButton.onClick.Invoke();
                Assert.IsFalse(replayScreen.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(host);
                EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        [Test]
        public void ReplayScreenLoadsLocalReplayJsonPath()
        {
            string replayPath = Path.Combine(Path.GetTempPath(), "vanguard_thai_sim_replay_test.json");
            GameState initial = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 3003);
            GameState live = GameState.FromJson(initial.ToJson(false));
            GameActionService.Draw(live, 0);
            GameReplay replay = GameReplay.Create(initial, live.event_log);
            File.WriteAllText(replayPath, replay.ToJson(false));

            GameObject host = new GameObject("Home Lobby Replay Load Test");
            try
            {
                HomeLobbyBootstrap bootstrap = host.AddComponent<HomeLobbyBootstrap>();
                typeof(HomeLobbyBootstrap)
                    .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(bootstrap, null);

                Button replayButton = FindChild(host.transform, "Replay Button").GetComponent<Button>();
                replayButton.onClick.Invoke();
                GameObject replayScreen = FindChild(host.transform, "Replay Screen");
                InputField pathInput = FindChild(replayScreen.transform, "Replay JSON path Input").GetComponent<InputField>();
                Button loadButton = FindChild(replayScreen.transform, "Load Path Button").GetComponent<Button>();

                pathInput.text = replayPath;
                loadButton.onClick.Invoke();

                Text summary = FindChild(replayScreen.transform, "Replay Summary Text").GetComponent<Text>();
                Assert.IsTrue(summary.text.Contains("Status: replay loaded."));
                Assert.IsTrue(summary.text.Contains("Events: 1"));
                Assert.IsTrue(summary.text.Contains(replayPath));

                Text preview = FindChild(replayScreen.transform, "Replay Preview Text").GetComponent<Text>();
                Assert.IsTrue(preview.text.Contains("Position: 0 / 1"));

                Button stepButton = FindChild(replayScreen.transform, "Step Replay Button").GetComponent<Button>();
                stepButton.onClick.Invoke();
                Assert.IsTrue(preview.text.Contains("Position: 1 / 1"));
                Assert.IsTrue(preview.text.Contains("Status: end"));

                Button startButton = FindChild(replayScreen.transform, "Start Preview Button").GetComponent<Button>();
                startButton.onClick.Invoke();
                Assert.IsTrue(preview.text.Contains("Position: 0 / 1"));

                Button endButton = FindChild(replayScreen.transform, "End Replay Button").GetComponent<Button>();
                endButton.onClick.Invoke();
                Assert.IsTrue(preview.text.Contains("Position: 1 / 1"));
            }
            finally
            {
                if (File.Exists(replayPath))
                {
                    File.Delete(replayPath);
                }

                Object.DestroyImmediate(host);
                EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        [Test]
        public void ReplayScreenInvalidPathShowsError()
        {
            GameObject host = new GameObject("Home Lobby Replay Error Test");
            try
            {
                HomeLobbyBootstrap bootstrap = host.AddComponent<HomeLobbyBootstrap>();
                typeof(HomeLobbyBootstrap)
                    .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(bootstrap, null);

                Button replayButton = FindChild(host.transform, "Replay Button").GetComponent<Button>();
                replayButton.onClick.Invoke();
                GameObject replayScreen = FindChild(host.transform, "Replay Screen");
                InputField pathInput = FindChild(replayScreen.transform, "Replay JSON path Input").GetComponent<InputField>();
                Button loadButton = FindChild(replayScreen.transform, "Load Path Button").GetComponent<Button>();

                pathInput.text = Path.Combine(Path.GetTempPath(), "missing-vanguard-replay.json");
                loadButton.onClick.Invoke();

                Text summary = FindChild(replayScreen.transform, "Replay Summary Text").GetComponent<Text>();
                Assert.IsTrue(summary.text.Contains("Status: replay not loaded."));
                Assert.IsTrue(summary.text.Contains("Replay file not found."));
            }
            finally
            {
                Object.DestroyImmediate(host);
                EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
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

        private static Text FindTextContaining(Transform root, string text)
        {
            if (root == null)
            {
                return null;
            }

            Text component = root.GetComponent<Text>();
            if (component != null && component.text.Contains(text))
            {
                return component;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Text found = FindTextContaining(root.GetChild(i), text);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static VanguardDeck CreateDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " Replay Deck", "D", "vanguard_th", "m30-03");
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
