using System;
using System.Text;
using VanguardThaiSim.Settings;

namespace VanguardThaiSim.UI
{
    public static class HomeSettingsPanelFormatter
    {
        public static string Format(PlayerSettings settings)
        {
            PlayerSettings normalized = PlayerSettings.Normalize(settings);
            StringBuilder builder = new StringBuilder();
            builder.Append("Player: ");
            builder.Append(Compact(normalized.player_name, 32));
            builder.Append('\n');
            builder.Append("Default deck: ");
            builder.Append(string.IsNullOrWhiteSpace(normalized.default_deck_id)
                ? "none"
                : Compact(normalized.default_deck_id, 48));
            builder.Append('\n');
            builder.Append("Preferred format: ");
            builder.Append(normalized.preferred_format);
            builder.Append('\n');
            builder.Append("UI scale: ");
            builder.Append(normalized.ui_scale.ToString("0.00"));
            builder.Append("x");
            builder.Append('\n');
            builder.Append("Image cache: ");
            builder.Append(normalized.image_cache_mode);
            return builder.ToString();
        }

        public static string NextPreferredFormat(string current)
        {
            string normalized = string.IsNullOrWhiteSpace(current) ? PlayerSettings.DefaultPreferredFormat : current.Trim();
            if (string.Equals(normalized, "D", StringComparison.OrdinalIgnoreCase))
            {
                return "V";
            }

            if (string.Equals(normalized, "V", StringComparison.OrdinalIgnoreCase))
            {
                return "Premium";
            }

            return "D";
        }

        public static PlayerImageCacheMode NextImageCacheMode(PlayerImageCacheMode current)
        {
            switch (current)
            {
                case PlayerImageCacheMode.Balanced:
                    return PlayerImageCacheMode.MemorySaver;
                case PlayerImageCacheMode.MemorySaver:
                    return PlayerImageCacheMode.HighQuality;
                default:
                    return PlayerImageCacheMode.Balanced;
            }
        }

        private static string Compact(string value, int maxLength)
        {
            string compact = string.Join(
                " ",
                (value ?? string.Empty).Trim().Split(
                    new[] { ' ', '\r', '\n', '\t' },
                    StringSplitOptions.RemoveEmptyEntries));
            if (compact.Length <= maxLength)
            {
                return compact;
            }

            return compact.Substring(0, maxLength - 3) + "...";
        }
    }
}
