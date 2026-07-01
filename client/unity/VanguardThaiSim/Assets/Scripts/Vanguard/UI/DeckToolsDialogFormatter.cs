using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public static class DeckToolsDialogFormatter
    {
        public static string FormatDialogStatus(VanguardDeck deck)
        {
            if (deck == null)
            {
                return "Current deck: none";
            }

            return "Current deck: " + (string.IsNullOrWhiteSpace(deck.name) ? "Untitled Deck" : deck.name) +
                   " / Main " + deck.TotalCards(DeckZone.Main) +
                   " / Ride " + deck.TotalCards(DeckZone.Ride) +
                   " / G " + deck.TotalCards(DeckZone.G);
        }

        public static string FormatOperationResult(string operation, bool accepted, string detail)
        {
            string prefix = accepted ? "OK" : "Rejected";
            string compactDetail = string.IsNullOrWhiteSpace(detail)
                ? "No detail."
                : string.Join(" ", detail.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries));
            return prefix + " [" + operation + "]: " + compactDetail;
        }

        public static string FormatOperationResultWithTip(string operation, bool accepted, string detail, string tipContext)
        {
            return LoadingTipCatalog.AppendTip(
                FormatOperationResult(operation, accepted, detail),
                tipContext);
        }
    }
}
