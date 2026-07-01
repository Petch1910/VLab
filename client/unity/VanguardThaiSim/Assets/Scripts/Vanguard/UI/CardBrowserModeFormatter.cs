namespace VanguardThaiSim.UI
{
    public static class CardBrowserModeFormatter
    {
        public static string FormatTitle(CardBrowserScreenMode mode)
        {
            return mode == CardBrowserScreenMode.Browser ? "Card Browser" : "Deck Builder";
        }

        public static string FormatLayoutSummary(CardBrowserScreenMode mode)
        {
            if (mode == CardBrowserScreenMode.Browser)
            {
                return "Card Browser: search/filter grid with a read-only detail preview.";
            }

            return "Deck Builder: left preview, center card grid, right deck list and counters.";
        }
    }
}
