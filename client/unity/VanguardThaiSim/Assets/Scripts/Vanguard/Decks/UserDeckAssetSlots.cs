using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Decks
{
    public static class UserDeckAssetSlots
    {
        public const string Sleeve = "sleeve";
        public const string CardBack = "card_back";
        public const string Playmat = "playmat";
        public const string Crest = "crest";
        public const string PersonaShield = "persona_shield";
        public const string GiftMarker = "gift_marker";
        public const string QuickShield = "quick_shield";

        private static readonly HashSet<string> KnownSlots =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Sleeve,
                CardBack,
                Playmat,
                Crest,
                PersonaShield,
                GiftMarker,
                QuickShield
            };

        public static bool IsKnown(string slot)
        {
            return !string.IsNullOrWhiteSpace(slot) && KnownSlots.Contains(slot.Trim());
        }
    }
}
