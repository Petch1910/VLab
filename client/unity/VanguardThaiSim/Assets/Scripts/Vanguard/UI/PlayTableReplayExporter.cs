using System;
using System.IO;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    [Serializable]
    public sealed class PlayTableReplayExportResult
    {
        public bool accepted;
        public string output_path;
        public int event_count;
        public string rejection_reason;
    }

    public static class PlayTableReplayExporter
    {
        public static PlayTableReplayExportResult Export(GameState initialState, GameState liveState, string outputPath)
        {
            if (initialState == null)
            {
                return Reject("Initial replay state is missing.");
            }

            if (liveState == null)
            {
                return Reject("Live state is missing.");
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return Reject("Replay output path is missing.");
            }

            try
            {
                string fullPath = Path.GetFullPath(outputPath);
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                GameReplay replay = GameReplay.Create(initialState, liveState.event_log);
                File.WriteAllText(fullPath, replay.ToJson(true));
                return new PlayTableReplayExportResult
                {
                    accepted = true,
                    output_path = fullPath,
                    event_count = replay.events == null ? 0 : replay.events.Count,
                    rejection_reason = string.Empty
                };
            }
            catch (Exception exception)
            {
                return Reject(exception.Message);
            }
        }

        private static PlayTableReplayExportResult Reject(string reason)
        {
            return new PlayTableReplayExportResult
            {
                accepted = false,
                output_path = string.Empty,
                event_count = 0,
                rejection_reason = string.IsNullOrWhiteSpace(reason) ? "Replay export failed." : reason
            };
        }
    }
}
