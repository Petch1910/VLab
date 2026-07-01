using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;

namespace VanguardThaiSim.Decks
{
    public sealed class DeckValidator
    {
        private readonly ICardRepository repository;
        private readonly DeckValidationRules rules;

        public DeckValidator(ICardRepository repository, DeckValidationRules rules = null)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.rules = rules ?? new DeckValidationRules();
        }

        public DeckValidationResult Validate(VanguardDeck deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            DeckValidationResult result = new DeckValidationResult
            {
                MainCount = deck.TotalCards(DeckZone.Main),
                RideCount = deck.TotalCards(DeckZone.Ride),
                GCount = deck.TotalCards(DeckZone.G)
            };

            ValidateZone(deck, DeckZone.Main, result);
            ValidateZone(deck, DeckZone.Ride, result);
            ValidateZone(deck, DeckZone.G, result);
            ValidateCounts(result);
            ValidateCopyLimits(deck, result);

            result.IsComplete =
                result.MainCount == rules.MainDeckSize &&
                result.RideCount <= rules.RideDeckMax &&
                result.GCount <= rules.GDeckMax &&
                !result.HasErrors;

            return result;
        }

        private void ValidateZone(VanguardDeck deck, DeckZone zone, DeckValidationResult result)
        {
            foreach (DeckCardEntry entry in deck.GetEntries(zone))
            {
                if (entry == null)
                {
                    result.Add(new DeckValidationIssue(
                        DeckValidationSeverity.Error,
                        "NULL_ENTRY",
                        "Deck contains an empty card entry.",
                        null,
                        zone));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.card_id))
                {
                    result.Add(new DeckValidationIssue(
                        DeckValidationSeverity.Error,
                        "MISSING_CARD_ID",
                        "Deck contains an entry without a card id.",
                        null,
                        zone));
                    continue;
                }

                if (entry.quantity <= 0)
                {
                    result.Add(new DeckValidationIssue(
                        DeckValidationSeverity.Error,
                        "BAD_QUANTITY",
                        "Card quantity must be greater than zero.",
                        entry.card_id,
                        zone));
                }

                CardDetail card = repository.GetCard(entry.card_id);
                if (card == null)
                {
                    result.Add(new DeckValidationIssue(
                        DeckValidationSeverity.Error,
                        "UNKNOWN_CARD",
                        "Card id does not exist in the active pack.",
                        entry.card_id,
                        zone));
                }
            }
        }

        private void ValidateCounts(DeckValidationResult result)
        {
            if (result.MainCount == 0)
            {
                result.Add(new DeckValidationIssue(
                    DeckValidationSeverity.Warning,
                    "EMPTY_MAIN_DECK",
                    "Main deck is empty."));
            }
            else if (result.MainCount < rules.MainDeckSize)
            {
                result.Add(new DeckValidationIssue(
                    DeckValidationSeverity.Warning,
                    "MAIN_DECK_INCOMPLETE",
                    "Main deck has fewer cards than the target size."));
            }
            else if (result.MainCount > rules.MainDeckSize)
            {
                result.Add(new DeckValidationIssue(
                    DeckValidationSeverity.Error,
                    "MAIN_DECK_TOO_LARGE",
                    "Main deck has more cards than the target size."));
            }

            if (result.RideCount > rules.RideDeckMax)
            {
                result.Add(new DeckValidationIssue(
                    DeckValidationSeverity.Error,
                    "RIDE_DECK_TOO_LARGE",
                    "Ride deck has more cards than allowed."));
            }

            if (result.GCount > rules.GDeckMax)
            {
                result.Add(new DeckValidationIssue(
                    DeckValidationSeverity.Error,
                    "G_DECK_TOO_LARGE",
                    "G deck has more cards than allowed."));
            }
        }

        private void ValidateCopyLimits(VanguardDeck deck, DeckValidationResult result)
        {
            Dictionary<string, int> copies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            AddCopies(deck, DeckZone.Main, copies);
            AddCopies(deck, DeckZone.Ride, copies);
            AddCopies(deck, DeckZone.G, copies);

            foreach (KeyValuePair<string, int> pair in copies)
            {
                CardDetail card = repository.GetCard(pair.Key);
                if (card == null)
                {
                    continue;
                }

                int limit = card.DeckLimit > 0 ? card.DeckLimit : rules.DefaultCopyLimit;
                if (pair.Value > limit)
                {
                    result.Add(new DeckValidationIssue(
                        DeckValidationSeverity.Error,
                        "COPY_LIMIT_EXCEEDED",
                        "Card exceeds its copy limit.",
                        pair.Key));
                }
            }
        }

        private static void AddCopies(VanguardDeck deck, DeckZone zone, Dictionary<string, int> copies)
        {
            foreach (DeckCardEntry entry in deck.GetEntries(zone))
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.card_id) || entry.quantity <= 0)
                {
                    continue;
                }

                int current;
                copies.TryGetValue(entry.card_id, out current);
                copies[entry.card_id] = current + entry.quantity;
            }
        }
    }
}
