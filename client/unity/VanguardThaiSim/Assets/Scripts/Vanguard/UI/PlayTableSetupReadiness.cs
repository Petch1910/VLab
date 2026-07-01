using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public sealed class PlayTableSetupReadinessResult
    {
        public bool can_start;
        public string status_message;
        public string rejection_reason;
        public int main_count;
        public int ride_count;
        public int g_count;
        public int opening_hand_count;
    }

    public static class PlayTableSetupReadiness
    {
        public const string MissingDeckReason = "select a deck before starting";
        public const string MissingValidationReason = "deck validation is not ready";
        public const string NotPlayableReason = "deck is not playable";

        public static PlayTableSetupReadinessResult Evaluate(
            VanguardDeck deck,
            DeckValidationResult validation,
            DeckValidationRules rules = null)
        {
            DeckValidationRules safeRules = rules ?? new DeckValidationRules();

            if (deck == null)
            {
                return Reject(MissingDeckReason, "Setup blocked: select a deck before starting.", 0, 0, 0);
            }

            if (validation == null)
            {
                return Reject(
                    MissingValidationReason,
                    "Setup blocked: deck validation is not ready.",
                    deck.TotalCards(DeckZone.Main),
                    deck.TotalCards(DeckZone.Ride),
                    deck.TotalCards(DeckZone.G));
            }

            if (!validation.IsPlayable)
            {
                string message =
                    "Setup blocked: deck is not playable. " +
                    FormatCounts(validation, safeRules) +
                    ", Issues " + validation.ErrorCount + " errors / " + validation.WarningCount + " warnings.";
                return Reject(
                    NotPlayableReason,
                    message,
                    validation.MainCount,
                    validation.RideCount,
                    validation.GCount);
            }

            return new PlayTableSetupReadinessResult
            {
                can_start = true,
                rejection_reason = string.Empty,
                status_message =
                    "Setup ready: " + FormatDeckName(deck) +
                    " | " + FormatCounts(validation, safeRules) +
                    " | opening hand " + GameStateFactory.OpeningHandSize +
                    " | phase Mulligan.",
                main_count = validation.MainCount,
                ride_count = validation.RideCount,
                g_count = validation.GCount,
                opening_hand_count = GameStateFactory.OpeningHandSize
            };
        }

        private static PlayTableSetupReadinessResult Reject(
            string reason,
            string message,
            int mainCount,
            int rideCount,
            int gCount)
        {
            return new PlayTableSetupReadinessResult
            {
                can_start = false,
                rejection_reason = reason,
                status_message = message,
                main_count = mainCount,
                ride_count = rideCount,
                g_count = gCount,
                opening_hand_count = GameStateFactory.OpeningHandSize
            };
        }

        private static string FormatCounts(DeckValidationResult validation, DeckValidationRules rules)
        {
            return
                "Main " + validation.MainCount + "/" + rules.MainDeckSize +
                ", Ride " + validation.RideCount + "/" + rules.RideDeckMax +
                ", G " + validation.GCount + "/" + rules.GDeckMax;
        }

        private static string FormatDeckName(VanguardDeck deck)
        {
            return string.IsNullOrWhiteSpace(deck.name) ? "Unnamed Deck" : deck.name.Trim();
        }
    }
}
