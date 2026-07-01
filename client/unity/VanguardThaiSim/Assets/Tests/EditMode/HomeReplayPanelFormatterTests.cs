using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class HomeReplayPanelFormatterTests
    {
        [Test]
        public void EmptyLibraryTextExplainsReplayEntryState()
        {
            string formatted = HomeReplayPanelFormatter.FormatEmptyLibrary();

            Assert.IsTrue(formatted.Contains("Replay Library"));
            Assert.IsTrue(formatted.Contains("no local replay selected"));
            Assert.IsTrue(formatted.Contains("GameReplay JSON"));
            Assert.IsFalse(formatted.Contains("scheduled in M16"));
        }

        [Test]
        public void LoadedReplayTextShowsReplayIdEventCountAndSource()
        {
            string formatted = HomeReplayPanelFormatter.FormatLoadedReplay("replay-1", 3, "C:\\temp\\replay.json");

            Assert.IsTrue(formatted.Contains("Status: replay loaded."));
            Assert.IsTrue(formatted.Contains("Replay id: replay-1"));
            Assert.IsTrue(formatted.Contains("Events: 3"));
            Assert.IsTrue(formatted.Contains("C:\\temp\\replay.json"));
        }

        [Test]
        public void ErrorTextShowsPlayerFacingReason()
        {
            string formatted = HomeReplayPanelFormatter.FormatError("Replay file not found.");

            Assert.IsTrue(formatted.Contains("Status: replay not loaded."));
            Assert.IsTrue(formatted.Contains("Replay file not found."));
        }

        [Test]
        public void PreviewTextShowsReplayCursorAndState()
        {
            string formatted = HomeReplayPanelFormatter.FormatPreview(1, 3, 2, "Battle", false);

            Assert.IsTrue(formatted.Contains("Replay Preview"));
            Assert.IsTrue(formatted.Contains("Position: 1 / 3"));
            Assert.IsTrue(formatted.Contains("Turn: 2"));
            Assert.IsTrue(formatted.Contains("Phase: Battle"));
            Assert.IsTrue(formatted.Contains("Status: ready"));
        }
    }
}
