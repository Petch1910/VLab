using System;

namespace VanguardThaiSim.UI
{
    public static class LoadingTipCatalog
    {
        public const string DataReload = "data_reload";
        public const string CardImages = "card_images";
        public const string DeckLoad = "deck_load";

        public static string Get(string context)
        {
            switch (Normalize(context))
            {
                case DataReload:
                    return "Reload refreshes the pack, latest deck, and validation status.";
                case CardImages:
                    return "Missing card images use fallback art so browsing can continue.";
                case DeckLoad:
                    return "Loaded decks keep cosmetics separate from legality checks.";
                default:
                    return "Use Manual when a screen or table action is unclear.";
            }
        }

        public static string Format(string context)
        {
            return "Tip: " + Get(context);
        }

        public static string[] AllTips()
        {
            return new[]
            {
                Get(DataReload),
                Get(CardImages),
                Get(DeckLoad)
            };
        }

        public static string AppendTip(string message, string context)
        {
            string compactMessage = string.IsNullOrWhiteSpace(message)
                ? string.Empty
                : string.Join(" ", message.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            string tip = Format(context);
            return string.IsNullOrEmpty(compactMessage)
                ? tip
                : compactMessage + "\n" + tip;
        }

        private static string Normalize(string context)
        {
            return string.IsNullOrWhiteSpace(context)
                ? string.Empty
                : context.Trim().ToLowerInvariant();
        }
    }
}
