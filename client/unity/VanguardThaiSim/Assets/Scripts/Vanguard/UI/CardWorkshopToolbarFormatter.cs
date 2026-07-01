namespace VanguardThaiSim.UI
{
    public static class CardWorkshopToolbarFormatter
    {
        public static string FormatCompactStatus(int totalCards, int showingCards, bool hasActiveFilters)
        {
            string filters = hasActiveFilters ? "on" : "none";
            return "Cards " + System.Math.Max(0, totalCards) +
                   " | Shown " + System.Math.Max(0, showingCards) +
                   " | Filters " + filters;
        }
    }
}
