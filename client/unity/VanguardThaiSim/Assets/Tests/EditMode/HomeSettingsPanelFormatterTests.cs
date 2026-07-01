using NUnit.Framework;
using VanguardThaiSim.Settings;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class HomeSettingsPanelFormatterTests
    {
        [Test]
        public void FormatUsesDefaultsForNullSettings()
        {
            string formatted = HomeSettingsPanelFormatter.Format(null);

            Assert.IsTrue(formatted.Contains("Player: Player"));
            Assert.IsTrue(formatted.Contains("Default deck: none"));
            Assert.IsTrue(formatted.Contains("Preferred format: D"));
            Assert.IsTrue(formatted.Contains("UI scale: 1.00x"));
            Assert.IsTrue(formatted.Contains("Image cache: Balanced"));
        }

        [Test]
        public void FormatUsesNormalizedCustomSettings()
        {
            var settings = new PlayerSettings
            {
                player_name = "  Local Fighter  ",
                default_deck_id = "  deck-001  ",
                preferred_format = "  Premium  ",
                ui_scale = 1.25f,
                image_cache_mode = PlayerImageCacheMode.HighQuality
            };

            string formatted = HomeSettingsPanelFormatter.Format(settings);

            Assert.IsTrue(formatted.Contains("Player: Local Fighter"));
            Assert.IsTrue(formatted.Contains("Default deck: deck-001"));
            Assert.IsTrue(formatted.Contains("Preferred format: Premium"));
            Assert.IsTrue(formatted.Contains("UI scale: 1.25x"));
            Assert.IsTrue(formatted.Contains("Image cache: HighQuality"));
        }

        [Test]
        public void NextPreferredFormatCyclesKnownFormats()
        {
            Assert.AreEqual("V", HomeSettingsPanelFormatter.NextPreferredFormat("D"));
            Assert.AreEqual("Premium", HomeSettingsPanelFormatter.NextPreferredFormat("V"));
            Assert.AreEqual("D", HomeSettingsPanelFormatter.NextPreferredFormat("Premium"));
            Assert.AreEqual("D", HomeSettingsPanelFormatter.NextPreferredFormat("unknown"));
        }

        [Test]
        public void NextImageCacheModeCyclesKnownModes()
        {
            Assert.AreEqual(
                PlayerImageCacheMode.MemorySaver,
                HomeSettingsPanelFormatter.NextImageCacheMode(PlayerImageCacheMode.Balanced));
            Assert.AreEqual(
                PlayerImageCacheMode.HighQuality,
                HomeSettingsPanelFormatter.NextImageCacheMode(PlayerImageCacheMode.MemorySaver));
            Assert.AreEqual(
                PlayerImageCacheMode.Balanced,
                HomeSettingsPanelFormatter.NextImageCacheMode(PlayerImageCacheMode.HighQuality));
            Assert.AreEqual(
                PlayerImageCacheMode.Balanced,
                HomeSettingsPanelFormatter.NextImageCacheMode((PlayerImageCacheMode)999));
        }
    }
}
