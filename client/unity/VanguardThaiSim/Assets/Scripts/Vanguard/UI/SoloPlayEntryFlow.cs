using System;
using System.Collections.Generic;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public enum SoloPlayOpponentDeckMode
    {
        MirrorPlayerDeck,
        RandomSavedDeck,
        SavedDeck
    }

    public sealed class SoloPlayEntryFlowOptions
    {
        public BotDifficulty difficulty = BotDifficulty.Easy;
        public SoloPlayOpponentDeckMode opponent_deck_mode = SoloPlayOpponentDeckMode.MirrorPlayerDeck;
        public string opponent_deck_id;
        public int random_seed;
    }

    public sealed class SoloPlayEntryFlowStartResult
    {
        public bool can_start;
        public string status_message;
        public string rejection_reason;
        public string difficulty_label;
        public string opponent_deck_label;
        public string playtable_mode_detail;
        public VanguardDeck player_deck;
        public VanguardDeck opponent_deck;
    }

    public static class SoloPlayEntryFlow
    {
        public const string MissingPlayerDeckReason = "player deck is not ready";
        public const string MissingOpponentDeckReason = "opponent deck is not ready";
        public const string OpponentDeckNotPlayableReason = "opponent deck is not playable";

        private static readonly BotDifficulty[] DifficultyOrder =
        {
            BotDifficulty.Easy,
            BotDifficulty.Normal,
            BotDifficulty.Hard
        };

        public static BotDifficulty DifficultyFromIndex(int index)
        {
            int safeIndex = NormalizeDifficultyIndex(index);
            return DifficultyOrder[safeIndex];
        }

        public static int NormalizeDifficultyIndex(int index)
        {
            if (index < 0)
            {
                return 0;
            }

            if (index >= DifficultyOrder.Length)
            {
                return DifficultyOrder.Length - 1;
            }

            return index;
        }

        public static int NextDifficultyIndex(int currentIndex)
        {
            int safeIndex = NormalizeDifficultyIndex(currentIndex);
            return (safeIndex + 1) % DifficultyOrder.Length;
        }

        public static string FormatDifficultyLabel(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.Normal:
                    return "Normal - balanced profile";
                case BotDifficulty.Hard:
                    return "Hard - strongest available heuristic";
                case BotDifficulty.Easy:
                default:
                    return "Easy - guided legal actions";
            }
        }

        public static int NormalizeOpponentChoiceIndex(
            int requestedIndex,
            IReadOnlyList<VanguardDeck> savedDecks)
        {
            int maxIndex = CountSavedDecks(savedDecks) <= 0 ? 0 : CountSavedDecks(savedDecks) + 1;
            if (requestedIndex < 0)
            {
                return 0;
            }

            if (requestedIndex > maxIndex)
            {
                return maxIndex;
            }

            return requestedIndex;
        }

        public static int NextOpponentChoiceIndex(
            int currentIndex,
            IReadOnlyList<VanguardDeck> savedDecks)
        {
            int safeIndex = NormalizeOpponentChoiceIndex(currentIndex, savedDecks);
            int maxIndex = CountSavedDecks(savedDecks) <= 0 ? 0 : CountSavedDecks(savedDecks) + 1;
            return safeIndex >= maxIndex ? 0 : safeIndex + 1;
        }

        public static string FormatOpponentChoiceLabel(
            int choiceIndex,
            IReadOnlyList<VanguardDeck> savedDecks)
        {
            int safeIndex = NormalizeOpponentChoiceIndex(choiceIndex, savedDecks);
            if (safeIndex == 0)
            {
                return "Mirror player deck";
            }

            if (safeIndex == 1)
            {
                return "Random saved deck";
            }

            VanguardDeck deck = GetSavedDeckByChoiceIndex(savedDecks, safeIndex);
            return "Saved deck: " + FormatDeckName(deck);
        }

        public static SoloPlayEntryFlowOptions CreateOptionsFromUi(
            int difficultyIndex,
            int opponentChoiceIndex,
            IReadOnlyList<VanguardDeck> savedDecks,
            int randomSeed)
        {
            int safeOpponentIndex = NormalizeOpponentChoiceIndex(opponentChoiceIndex, savedDecks);
            SoloPlayEntryFlowOptions options = new SoloPlayEntryFlowOptions
            {
                difficulty = DifficultyFromIndex(difficultyIndex),
                random_seed = randomSeed
            };

            if (safeOpponentIndex == 0)
            {
                options.opponent_deck_mode = SoloPlayOpponentDeckMode.MirrorPlayerDeck;
            }
            else if (safeOpponentIndex == 1)
            {
                options.opponent_deck_mode = SoloPlayOpponentDeckMode.RandomSavedDeck;
            }
            else
            {
                VanguardDeck deck = GetSavedDeckByChoiceIndex(savedDecks, safeOpponentIndex);
                options.opponent_deck_mode = SoloPlayOpponentDeckMode.SavedDeck;
                options.opponent_deck_id = deck == null ? string.Empty : deck.deck_id;
            }

            return options;
        }

        public static SoloPlayEntryFlowStartResult CreateStartRequest(
            VanguardDeck playerDeck,
            DeckValidationResult playerValidation,
            DeckValidator validator,
            IReadOnlyList<VanguardDeck> savedDecks,
            SoloPlayEntryFlowOptions options)
        {
            SoloPlayEntryFlowOptions safeOptions = options ?? new SoloPlayEntryFlowOptions();
            PlayTableSetupReadinessResult playerReadiness =
                PlayTableSetupReadiness.Evaluate(playerDeck, playerValidation);
            if (!playerReadiness.can_start)
            {
                return Reject(
                    MissingPlayerDeckReason,
                    playerReadiness.status_message,
                    safeOptions,
                    string.Empty);
            }

            string opponentLabel;
            VanguardDeck opponentSource = SelectOpponentDeck(
                playerDeck,
                validator,
                savedDecks,
                safeOptions,
                out opponentLabel);
            if (opponentSource == null)
            {
                return Reject(
                    MissingOpponentDeckReason,
                    "Solo setup blocked: choose a playable bot deck or use Mirror player deck.",
                    safeOptions,
                    opponentLabel);
            }

            DeckValidationResult opponentValidation =
                ReferenceEquals(opponentSource, playerDeck)
                    ? playerValidation
                    : validator == null ? null : validator.Validate(opponentSource);
            PlayTableSetupReadinessResult opponentReadiness =
                PlayTableSetupReadiness.Evaluate(opponentSource, opponentValidation);
            if (!opponentReadiness.can_start)
            {
                return Reject(
                    OpponentDeckNotPlayableReason,
                    "Solo setup blocked: bot deck is not playable. " + opponentReadiness.status_message,
                    safeOptions,
                    opponentLabel);
            }

            string difficultyLabel = FormatDifficultyLabel(safeOptions.difficulty);
            string status =
                "Solo setup ready: " +
                FormatDeckName(playerDeck) +
                " vs " +
                opponentLabel +
                " | " +
                difficultyLabel +
                " | manual table starts first; CPU automation remains gated.";
            return new SoloPlayEntryFlowStartResult
            {
                can_start = true,
                rejection_reason = string.Empty,
                status_message = status,
                difficulty_label = difficultyLabel,
                opponent_deck_label = opponentLabel,
                playtable_mode_detail =
                    "Solo Practice | " +
                    difficultyLabel +
                    " | Bot deck: " +
                    opponentLabel,
                player_deck = CloneDeck(playerDeck),
                opponent_deck = CloneDeck(opponentSource)
            };
        }

        private static VanguardDeck SelectOpponentDeck(
            VanguardDeck playerDeck,
            DeckValidator validator,
            IReadOnlyList<VanguardDeck> savedDecks,
            SoloPlayEntryFlowOptions options,
            out string opponentLabel)
        {
            if (options.opponent_deck_mode == SoloPlayOpponentDeckMode.MirrorPlayerDeck)
            {
                opponentLabel = "Mirror player deck";
                return playerDeck;
            }

            if (options.opponent_deck_mode == SoloPlayOpponentDeckMode.SavedDeck)
            {
                VanguardDeck selected = FindSavedDeck(savedDecks, options.opponent_deck_id);
                opponentLabel = selected == null ? "Saved deck" : "Saved deck: " + FormatDeckName(selected);
                return selected;
            }

            List<VanguardDeck> playableDecks = new List<VanguardDeck>();
            int count = CountSavedDecks(savedDecks);
            for (int i = 0; i < count; i++)
            {
                VanguardDeck deck = savedDecks[i];
                if (IsPlayable(deck, validator))
                {
                    playableDecks.Add(deck);
                }
            }

            if (playableDecks.Count == 0)
            {
                opponentLabel = "Random saved deck";
                return null;
            }

            int selectedIndex = PositiveModulo(options.random_seed, playableDecks.Count);
            VanguardDeck randomDeck = playableDecks[selectedIndex];
            opponentLabel = "Random saved deck: " + FormatDeckName(randomDeck);
            return randomDeck;
        }

        private static bool IsPlayable(VanguardDeck deck, DeckValidator validator)
        {
            if (deck == null || validator == null)
            {
                return false;
            }

            return validator.Validate(deck).IsPlayable;
        }

        private static VanguardDeck FindSavedDeck(
            IReadOnlyList<VanguardDeck> savedDecks,
            string deckId)
        {
            int count = CountSavedDecks(savedDecks);
            for (int i = 0; i < count; i++)
            {
                VanguardDeck deck = savedDecks[i];
                if (deck != null &&
                    string.Equals(deck.deck_id, deckId, StringComparison.OrdinalIgnoreCase))
                {
                    return deck;
                }
            }

            return null;
        }

        private static VanguardDeck GetSavedDeckByChoiceIndex(
            IReadOnlyList<VanguardDeck> savedDecks,
            int choiceIndex)
        {
            int savedIndex = choiceIndex - 2;
            if (savedDecks == null || savedIndex < 0 || savedIndex >= savedDecks.Count)
            {
                return null;
            }

            return savedDecks[savedIndex];
        }

        private static int CountSavedDecks(IReadOnlyList<VanguardDeck> savedDecks)
        {
            return savedDecks == null ? 0 : savedDecks.Count;
        }

        private static SoloPlayEntryFlowStartResult Reject(
            string reason,
            string message,
            SoloPlayEntryFlowOptions options,
            string opponentLabel)
        {
            return new SoloPlayEntryFlowStartResult
            {
                can_start = false,
                rejection_reason = reason,
                status_message = message,
                difficulty_label = FormatDifficultyLabel((options ?? new SoloPlayEntryFlowOptions()).difficulty),
                opponent_deck_label = string.IsNullOrWhiteSpace(opponentLabel) ? "not selected" : opponentLabel,
                playtable_mode_detail = string.Empty
            };
        }

        private static VanguardDeck CloneDeck(VanguardDeck deck)
        {
            return deck == null ? null : VanguardDeck.FromJson(deck.ToJson(false));
        }

        private static string FormatDeckName(VanguardDeck deck)
        {
            return deck == null || string.IsNullOrWhiteSpace(deck.name)
                ? "Unnamed Deck"
                : deck.name.Trim();
        }

        private static int PositiveModulo(int value, int modulo)
        {
            if (modulo <= 0)
            {
                return 0;
            }

            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }
    }
}
