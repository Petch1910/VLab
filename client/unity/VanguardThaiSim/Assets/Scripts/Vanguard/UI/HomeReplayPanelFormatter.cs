namespace VanguardThaiSim.UI
{
    public static class HomeReplayPanelFormatter
    {
        public const string EmptyLibraryMessage =
            "Replay Library\n" +
            "Status: no local replay selected.\n" +
            "Supported: local GameReplay JSON.\n" +
            "Next: choose or export a replay file, then open it here.";
        public const string EmptyPreviewMessage =
            "Replay Preview\nNo replay loaded.";

        public static string FormatEmptyLibrary()
        {
            return EmptyLibraryMessage;
        }

        public static string FormatLoadedReplay(string replayId, int eventCount, string sourcePath)
        {
            return "Replay Library\n" +
                   "Status: replay loaded.\n" +
                   "Replay id: " + (string.IsNullOrWhiteSpace(replayId) ? "unknown" : replayId) + "\n" +
                   "Events: " + (eventCount < 0 ? 0 : eventCount) + "\n" +
                   "Source: " + (string.IsNullOrWhiteSpace(sourcePath) ? "local path" : sourcePath);
        }

        public static string FormatError(string reason)
        {
            return "Replay Library\n" +
                   "Status: replay not loaded.\n" +
                   "Error: " + (string.IsNullOrWhiteSpace(reason) ? "unknown replay error" : reason);
        }

        public static string FormatPreview(int currentIndex, int eventCount, int turnNumber, string phase, bool atEnd)
        {
            return "Replay Preview\n" +
                   "Position: " + ClampNonNegative(currentIndex) + " / " + ClampNonNegative(eventCount) + "\n" +
                   "Turn: " + ClampNonNegative(turnNumber) + "\n" +
                   "Phase: " + (string.IsNullOrWhiteSpace(phase) ? "unknown" : phase) + "\n" +
                   "Status: " + (atEnd ? "end" : "ready");
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
