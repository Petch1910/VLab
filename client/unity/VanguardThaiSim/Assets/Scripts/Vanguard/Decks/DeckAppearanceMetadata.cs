using System;
using UnityEngine;

namespace VanguardThaiSim.Decks
{
    [Serializable]
    public sealed class DeckAppearanceMetadata
    {
        public const string DefaultKey = "default";
        public const int MaximumKeyLength = 64;

        public string sleeve_key = DefaultKey;
        public string card_back_key = DefaultKey;
        public string playmat_key = DefaultKey;
        public string crest_key = DefaultKey;
        public string persona_shield_key = DefaultKey;
        public string gift_marker_key = DefaultKey;
        public string quick_shield_key = DefaultKey;

        public static DeckAppearanceMetadata CreateDefault()
        {
            return new DeckAppearanceMetadata();
        }

        public static DeckAppearanceMetadata Normalize(DeckAppearanceMetadata source)
        {
            if (source == null)
            {
                return CreateDefault();
            }

            return new DeckAppearanceMetadata
            {
                sleeve_key = NormalizeKey(source.sleeve_key),
                card_back_key = NormalizeKey(source.card_back_key),
                playmat_key = NormalizeKey(source.playmat_key),
                crest_key = NormalizeKey(source.crest_key),
                persona_shield_key = NormalizeKey(source.persona_shield_key),
                gift_marker_key = NormalizeKey(source.gift_marker_key),
                quick_shield_key = NormalizeKey(source.quick_shield_key)
            };
        }

        public static DeckAppearanceMetadata FromLegacyCosmetics(DeckCosmetics cosmetics)
        {
            if (cosmetics == null)
            {
                return CreateDefault();
            }

            return Normalize(new DeckAppearanceMetadata
            {
                sleeve_key = cosmetics.sleeve,
                card_back_key = cosmetics.sleeve,
                playmat_key = cosmetics.playmat,
                crest_key = cosmetics.crest,
                persona_shield_key = cosmetics.persona_shield
            });
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(Normalize(this), prettyPrint);
        }

        public static DeckAppearanceMetadata FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefault();
            }

            try
            {
                DeckAppearanceMetadata metadata = JsonUtility.FromJson<DeckAppearanceMetadata>(json);
                return Normalize(metadata);
            }
            catch (Exception)
            {
                return CreateDefault();
            }
        }

        private static string NormalizeKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultKey;
            }

            string trimmed = value.Trim();
            if (trimmed.Length == 0 ||
                trimmed.Length > MaximumKeyLength ||
                trimmed.Contains("..") ||
                trimmed.Contains("/") ||
                trimmed.Contains("\\"))
            {
                return DefaultKey;
            }

            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                bool valid =
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_' ||
                    c == '-' ||
                    c == '.';

                if (!valid)
                {
                    return DefaultKey;
                }
            }

            return trimmed;
        }
    }
}
