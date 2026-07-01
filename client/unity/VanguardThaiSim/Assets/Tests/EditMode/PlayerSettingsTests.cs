using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Settings;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayerSettingsTests
    {
        [Test]
        public void DefaultSettingsUseWindowsLocalPreferences()
        {
            PlayerSettings settings = PlayerSettings.CreateDefault();

            Assert.AreEqual(PlayerSettings.DefaultPlayerName, settings.player_name);
            Assert.AreEqual(string.Empty, settings.default_deck_id);
            Assert.AreEqual(PlayerSettings.DefaultPreferredFormat, settings.preferred_format);
            Assert.AreEqual(PlayerSettings.DefaultUiScale, settings.ui_scale);
            Assert.AreEqual(PlayerImageCacheMode.Balanced, settings.image_cache_mode);
        }

        [Test]
        public void JsonRoundTripPreservesValidSettings()
        {
            var settings = new PlayerSettings
            {
                player_name = "Phet",
                default_deck_id = "deck-royal-paladin",
                preferred_format = "Premium",
                ui_scale = 1.25f,
                image_cache_mode = PlayerImageCacheMode.HighQuality
            };

            PlayerSettings roundTrip = PlayerSettings.FromJson(settings.ToJson(false));

            Assert.AreEqual("Phet", roundTrip.player_name);
            Assert.AreEqual("deck-royal-paladin", roundTrip.default_deck_id);
            Assert.AreEqual("Premium", roundTrip.preferred_format);
            Assert.AreEqual(1.25f, roundTrip.ui_scale);
            Assert.AreEqual(PlayerImageCacheMode.HighQuality, roundTrip.image_cache_mode);
        }

        [Test]
        public void NormalizeTrimsTextClampsScaleAndFallbacksInvalidMode()
        {
            var settings = new PlayerSettings
            {
                player_name = "  Local Fighter  ",
                default_deck_id = "  deck-001  ",
                preferred_format = "  D  ",
                ui_scale = 9.0f,
                image_cache_mode = (PlayerImageCacheMode)999
            };

            PlayerSettings normalized = PlayerSettings.Normalize(settings);

            Assert.AreEqual("Local Fighter", normalized.player_name);
            Assert.AreEqual("deck-001", normalized.default_deck_id);
            Assert.AreEqual("D", normalized.preferred_format);
            Assert.AreEqual(PlayerSettings.MaximumUiScale, normalized.ui_scale);
            Assert.AreEqual(PlayerImageCacheMode.Balanced, normalized.image_cache_mode);
        }

        [Test]
        public void NormalizeFallsBackBlankTextAndInvalidScale()
        {
            var settings = new PlayerSettings
            {
                player_name = " ",
                default_deck_id = null,
                preferred_format = "",
                ui_scale = float.NaN,
                image_cache_mode = PlayerImageCacheMode.MemorySaver
            };

            PlayerSettings normalized = PlayerSettings.Normalize(settings);

            Assert.AreEqual(PlayerSettings.DefaultPlayerName, normalized.player_name);
            Assert.AreEqual(string.Empty, normalized.default_deck_id);
            Assert.AreEqual(PlayerSettings.DefaultPreferredFormat, normalized.preferred_format);
            Assert.AreEqual(PlayerSettings.DefaultUiScale, normalized.ui_scale);
            Assert.AreEqual(PlayerImageCacheMode.MemorySaver, normalized.image_cache_mode);
        }

        [Test]
        public void NormalizeDoesNotMutateSource()
        {
            var settings = new PlayerSettings
            {
                player_name = "  Phet  ",
                default_deck_id = "  deck  ",
                preferred_format = "  V  ",
                ui_scale = 0.1f,
                image_cache_mode = PlayerImageCacheMode.Balanced
            };
            string before = JsonUtility.ToJson(settings, false);

            PlayerSettings.Normalize(settings);

            Assert.AreEqual(before, JsonUtility.ToJson(settings, false));
        }

        [Test]
        public void FromJsonFallsBackForEmptyOrInvalidJson()
        {
            PlayerSettings empty = PlayerSettings.FromJson("");
            PlayerSettings invalid = PlayerSettings.FromJson("{not valid json");

            Assert.AreEqual(PlayerSettings.DefaultPlayerName, empty.player_name);
            Assert.AreEqual(PlayerSettings.DefaultPlayerName, invalid.player_name);
            Assert.AreEqual(PlayerSettings.DefaultUiScale, invalid.ui_scale);
        }
    }
}
