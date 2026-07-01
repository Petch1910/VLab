using System.IO;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableReplayExporterTests
    {
        [Test]
        public void ExportWritesReplayJsonWithoutMutatingSourceStates()
        {
            string outputPath = Path.Combine(Path.GetTempPath(), "vanguard_thai_sim_playtable_replay_export_test.json");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            GameState initial = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 3105);
            GameState live = GameState.FromJson(initial.ToJson(false));
            GameActionService.Draw(live, 0);
            string initialBefore = initial.ToJson(false);
            string liveBefore = live.ToJson(false);

            try
            {
                PlayTableReplayExportResult result = PlayTableReplayExporter.Export(initial, live, outputPath);

                Assert.IsTrue(result.accepted, result.rejection_reason);
                Assert.AreEqual(Path.GetFullPath(outputPath), result.output_path);
                Assert.AreEqual(1, result.event_count);
                Assert.IsTrue(File.Exists(outputPath));
                Assert.AreEqual(initialBefore, initial.ToJson(false));
                Assert.AreEqual(liveBefore, live.ToJson(false));

                GameReplay replay = GameReplay.FromJson(File.ReadAllText(outputPath));
                GameState replayInitial = replay.CreateInitialState();
                GameReplayPlayer player = new GameReplayPlayer(replay);
                player.JumpToEnd();

                Assert.AreEqual(1, replay.events.Count);
                Assert.AreEqual(0, replayInitial.event_log.Count);
                Assert.AreEqual(initial.GetPlayer(0).CountZone(GameZone.Deck), replayInitial.GetPlayer(0).CountZone(GameZone.Deck));
                Assert.AreEqual(live.GetPlayer(0).CountZone(GameZone.Deck), player.CurrentState.GetPlayer(0).CountZone(GameZone.Deck));
                Assert.AreEqual(live.event_log.Count, player.CurrentState.event_log.Count);
            }
            finally
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }

        [Test]
        public void ExportRejectsMissingInputsWithoutWritingFile()
        {
            string outputPath = Path.Combine(Path.GetTempPath(), "vanguard_thai_sim_playtable_replay_export_reject_test.json");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 3106);

            PlayTableReplayExportResult missingInitial = PlayTableReplayExporter.Export(null, state, outputPath);
            PlayTableReplayExportResult missingLive = PlayTableReplayExporter.Export(state, null, outputPath);
            PlayTableReplayExportResult missingPath = PlayTableReplayExporter.Export(state, state, string.Empty);

            Assert.IsFalse(missingInitial.accepted);
            Assert.IsTrue(missingInitial.rejection_reason.Contains("Initial"));
            Assert.IsFalse(missingLive.accepted);
            Assert.IsTrue(missingLive.rejection_reason.Contains("Live"));
            Assert.IsFalse(missingPath.accepted);
            Assert.IsTrue(missingPath.rejection_reason.Contains("path"));
            Assert.IsFalse(File.Exists(outputPath));
        }

        private static VanguardDeck CreateDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " Replay Export Deck", "D", "vanguard_th", "m30-05");
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
