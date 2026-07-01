using System;
using UnityEngine;

namespace VanguardThaiSim.Settings
{
    public enum PlayerImageCacheMode
    {
        Balanced,
        MemorySaver,
        HighQuality
    }

    [Serializable]
    public sealed class PlayerSettings
    {
        public const string DefaultPlayerName = "Player";
        public const string DefaultPreferredFormat = "D";
        public const float DefaultUiScale = 1.0f;
        public const float MinimumUiScale = 0.75f;
        public const float MaximumUiScale = 1.5f;
        public const int MaximumPlayerNameLength = 32;
        public const int MaximumDeckIdLength = 128;
        public const int MaximumFormatLength = 32;

        public string player_name = DefaultPlayerName;
        public string default_deck_id = string.Empty;
        public string preferred_format = DefaultPreferredFormat;
        public float ui_scale = DefaultUiScale;
        public PlayerImageCacheMode image_cache_mode = PlayerImageCacheMode.Balanced;

        public static PlayerSettings CreateDefault()
        {
            return new PlayerSettings();
        }

        public static PlayerSettings Normalize(PlayerSettings source)
        {
            if (source == null)
            {
                return CreateDefault();
            }

            return new PlayerSettings
            {
                player_name = NormalizeText(
                    source.player_name,
                    DefaultPlayerName,
                    MaximumPlayerNameLength),
                default_deck_id = NormalizeText(
                    source.default_deck_id,
                    string.Empty,
                    MaximumDeckIdLength),
                preferred_format = NormalizeText(
                    source.preferred_format,
                    DefaultPreferredFormat,
                    MaximumFormatLength),
                ui_scale = ClampScale(source.ui_scale),
                image_cache_mode = IsKnownMode(source.image_cache_mode)
                    ? source.image_cache_mode
                    : PlayerImageCacheMode.Balanced
            };
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(Normalize(this), prettyPrint);
        }

        public static PlayerSettings FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefault();
            }

            try
            {
                PlayerSettings settings = JsonUtility.FromJson<PlayerSettings>(json);
                return Normalize(settings);
            }
            catch (Exception)
            {
                return CreateDefault();
            }
        }

        private static string NormalizeText(string value, string fallback, int maxLength)
        {
            string normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized.Substring(0, maxLength);
        }

        private static float ClampScale(float value)
        {
            if (float.IsNaN(value) || value <= 0f)
            {
                return DefaultUiScale;
            }

            return Math.Min(MaximumUiScale, Math.Max(MinimumUiScale, value));
        }

        private static bool IsKnownMode(PlayerImageCacheMode mode)
        {
            return Enum.IsDefined(typeof(PlayerImageCacheMode), mode);
        }
    }
}
