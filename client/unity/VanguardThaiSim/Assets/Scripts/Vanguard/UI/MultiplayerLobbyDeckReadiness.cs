using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public sealed class MultiplayerLobbyDeckReadinessResult
    {
        public bool accepted;
        public string rejection_reason;
        public int main_count;
        public int ride_count;
        public int g_count;
    }

    public static class MultiplayerLobbyDeckReadiness
    {
        public static MultiplayerLobbyDeckReadinessResult Evaluate(
            VanguardDeck deck,
            DeckValidationRules rules = null)
        {
            DeckValidationRules safeRules = rules ?? new DeckValidationRules();
            MultiplayerLobbyDeckReadinessResult result = new MultiplayerLobbyDeckReadinessResult();
            if (deck == null)
            {
                result.accepted = false;
                result.rejection_reason = "Deck missing.";
                return result;
            }

            result.main_count = deck.TotalCards(DeckZone.Main);
            result.ride_count = deck.TotalCards(DeckZone.Ride);
            result.g_count = deck.TotalCards(DeckZone.G);

            if (result.main_count != safeRules.MainDeckSize)
            {
                result.accepted = false;
                result.rejection_reason = "Main deck must be " + safeRules.MainDeckSize +
                    " cards (currently " + result.main_count + ").";
                return result;
            }

            if (result.ride_count > safeRules.RideDeckMax)
            {
                result.accepted = false;
                result.rejection_reason = "Ride deck must be at most " + safeRules.RideDeckMax +
                    " cards (currently " + result.ride_count + ").";
                return result;
            }

            if (result.g_count > safeRules.GDeckMax)
            {
                result.accepted = false;
                result.rejection_reason = "G deck must be at most " + safeRules.GDeckMax +
                    " cards (currently " + result.g_count + ").";
                return result;
            }

            result.accepted = true;
            result.rejection_reason = "";
            return result;
        }
    }
}
