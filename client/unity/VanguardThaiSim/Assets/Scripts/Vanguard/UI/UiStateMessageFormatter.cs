namespace VanguardThaiSim.UI
{
    public static class UiStateMessageFormatter
    {
        public const string CardPoolPreparing = "Preparing card pool";
        public const string FilterPreparing = "Select";
        public const string ZoneEmpty = "No cards";

        public static string FormatCardPackLoadFailure(string detail, string expectedPack)
        {
            string safeDetail = string.IsNullOrWhiteSpace(detail)
                ? "Card pack is not ready."
                : Compact(detail);
            string safePack = string.IsNullOrWhiteSpace(expectedPack)
                ? "unknown"
                : Compact(expectedPack);

            return "The runtime card pack could not be loaded.\n" +
                   safeDetail + "\n\n" +
                   "Expected pack: " + safePack + "\n" +
                   GracefulErrorMessageFormatter.RetryAction;
        }

        private static string Compact(string value)
        {
            return string.Join(" ", value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
