using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableMatchLogDensityTests
    {
        [TearDown]
        public void Cleanup()
        {
            PlayTableBootstrap[] tables =
                Object.FindObjectsByType<PlayTableBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (PlayTableBootstrap table in tables)
            {
                Object.DestroyImmediate(table.gameObject);
            }
        }

        [Test]
        public void PlayTableKeepsMatchLogsAdvancedOnlyForDeDashboardField()
        {
            PlayTableBootstrap.Show(CreateDeck("p1"), CreateDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject advancedDrawer = GameObject.Find("Advanced Drawer");
            GameObject compactLog = GameObject.Find("PlayTable Compact Match Log");
            GameObject fullLog = GameObject.Find("PlayTable Full Match Log");

            Assert.NotNull(table);
            Assert.NotNull(advancedDrawer);
            Assert.NotNull(compactLog);
            Assert.NotNull(fullLog);
            Assert.IsTrue(compactLog.transform.IsChildOf(advancedDrawer.transform));
            Assert.IsTrue(fullLog.transform.IsChildOf(advancedDrawer.transform));

            Text compactText = compactLog.GetComponent<Text>();
            Text fullText = fullLog.GetComponent<Text>();
            LayoutElement compactLayout = compactLog.GetComponent<LayoutElement>();
            LayoutElement fullLayout = fullLog.GetComponent<LayoutElement>();

            Assert.NotNull(compactText);
            Assert.NotNull(fullText);
            Assert.NotNull(compactLayout);
            Assert.NotNull(fullLayout);
            Assert.AreEqual(table.CreateCompactEventReplayPanel(), compactText.text);
            Assert.AreEqual(table.CreateEventReplayPanel(), fullText.text);
            Assert.LessOrEqual(compactLayout.preferredHeight, 140f);
            Assert.Greater(fullLayout.preferredHeight, compactLayout.preferredHeight);
        }

        private static VanguardDeck CreateDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " Match Log Deck", "D", "vanguard_th", "m28-10");
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
