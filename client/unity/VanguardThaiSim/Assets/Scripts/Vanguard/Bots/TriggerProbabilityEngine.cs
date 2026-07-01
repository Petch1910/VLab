using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class TriggerHitProbability
    {
        public int TriggerHits { get; private set; }
        public double Probability { get; private set; }

        public TriggerHitProbability(int triggerHits, double probability)
        {
            TriggerHits = triggerHits;
            Probability = probability;
        }
    }

    public sealed class TriggerProbabilityResult
    {
        private readonly List<TriggerHitProbability> hitDistribution;

        public bool IsValid { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public int TotalCards { get; private set; }
        public int TriggerCards { get; private set; }
        public int NonTriggerCards { get; private set; }
        public int CheckCount { get; private set; }
        public double ProbabilityAtLeastOneTrigger { get; private set; }
        public IReadOnlyList<TriggerHitProbability> HitDistribution
        {
            get { return hitDistribution; }
        }

        private TriggerProbabilityResult(
            bool isValid,
            string errorCode,
            string errorMessage,
            int totalCards,
            int triggerCards,
            int checkCount,
            double probabilityAtLeastOneTrigger,
            List<TriggerHitProbability> hitDistribution)
        {
            IsValid = isValid;
            ErrorCode = errorCode ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
            TotalCards = totalCards;
            TriggerCards = triggerCards;
            NonTriggerCards = Math.Max(0, totalCards - triggerCards);
            CheckCount = checkCount;
            ProbabilityAtLeastOneTrigger = probabilityAtLeastOneTrigger;
            this.hitDistribution = hitDistribution ?? new List<TriggerHitProbability>();
        }

        public double GetProbabilityForHits(int triggerHits)
        {
            for (int i = 0; i < hitDistribution.Count; i++)
            {
                if (hitDistribution[i].TriggerHits == triggerHits)
                {
                    return hitDistribution[i].Probability;
                }
            }

            return 0d;
        }

        internal static TriggerProbabilityResult Valid(
            int totalCards,
            int triggerCards,
            int checkCount,
            double probabilityAtLeastOneTrigger,
            List<TriggerHitProbability> hitDistribution)
        {
            return new TriggerProbabilityResult(
                true,
                string.Empty,
                string.Empty,
                totalCards,
                triggerCards,
                checkCount,
                probabilityAtLeastOneTrigger,
                hitDistribution);
        }

        internal static TriggerProbabilityResult Invalid(
            string errorCode,
            string errorMessage,
            int totalCards,
            int triggerCards,
            int checkCount)
        {
            return new TriggerProbabilityResult(
                false,
                errorCode,
                errorMessage,
                totalCards,
                triggerCards,
                checkCount,
                0d,
                new List<TriggerHitProbability>());
        }
    }

    public static class TriggerProbabilityEngine
    {
        public static bool TryCalculate(
            int totalCards,
            int triggerCards,
            int checkCount,
            out TriggerProbabilityResult result)
        {
            if (!ValidateCounts(totalCards, triggerCards, checkCount, out result))
            {
                return false;
            }

            int nonTriggerCards = totalCards - triggerCards;
            int minimumHits = Math.Max(0, checkCount - nonTriggerCards);
            int maximumHits = Math.Min(checkCount, triggerCards);
            double denominator = LogCombination(totalCards, checkCount);
            double probabilityAtLeastOne = 0d;
            var distribution = new List<TriggerHitProbability>(checkCount + 1);

            for (int hits = 0; hits <= checkCount; hits++)
            {
                double probability = 0d;
                if (hits >= minimumHits && hits <= maximumHits)
                {
                    probability = Math.Exp(
                        LogCombination(triggerCards, hits) +
                        LogCombination(nonTriggerCards, checkCount - hits) -
                        denominator);
                }

                if (hits > 0)
                {
                    probabilityAtLeastOne += probability;
                }

                distribution.Add(new TriggerHitProbability(hits, probability));
            }

            result = TriggerProbabilityResult.Valid(
                totalCards,
                triggerCards,
                checkCount,
                probabilityAtLeastOne,
                distribution);
            return true;
        }

        public static bool TryCalculateFromCards(
            IEnumerable<GameCardInstance> cards,
            Func<GameCardInstance, bool> isTrigger,
            int checkCount,
            out TriggerProbabilityResult result)
        {
            if (cards == null)
            {
                result = TriggerProbabilityResult.Invalid(
                    "CARDS_REQUIRED",
                    "Cards are required.",
                    0,
                    0,
                    checkCount);
                return false;
            }

            if (isTrigger == null)
            {
                result = TriggerProbabilityResult.Invalid(
                    "TRIGGER_CLASSIFIER_REQUIRED",
                    "A trigger classifier is required.",
                    0,
                    0,
                    checkCount);
                return false;
            }

            int totalCards = 0;
            int triggerCards = 0;
            foreach (GameCardInstance card in cards)
            {
                totalCards++;
                if (isTrigger(card))
                {
                    triggerCards++;
                }
            }

            return TryCalculate(totalCards, triggerCards, checkCount, out result);
        }

        private static bool ValidateCounts(
            int totalCards,
            int triggerCards,
            int checkCount,
            out TriggerProbabilityResult result)
        {
            if (totalCards < 0)
            {
                result = TriggerProbabilityResult.Invalid(
                    "INVALID_TOTAL_CARDS",
                    "Total cards cannot be negative.",
                    totalCards,
                    triggerCards,
                    checkCount);
                return false;
            }

            if (triggerCards < 0)
            {
                result = TriggerProbabilityResult.Invalid(
                    "INVALID_TRIGGER_CARDS",
                    "Trigger cards cannot be negative.",
                    totalCards,
                    triggerCards,
                    checkCount);
                return false;
            }

            if (triggerCards > totalCards)
            {
                result = TriggerProbabilityResult.Invalid(
                    "TRIGGER_CARDS_EXCEED_TOTAL",
                    "Trigger cards cannot exceed total cards.",
                    totalCards,
                    triggerCards,
                    checkCount);
                return false;
            }

            if (checkCount < 0)
            {
                result = TriggerProbabilityResult.Invalid(
                    "INVALID_CHECK_COUNT",
                    "Check count cannot be negative.",
                    totalCards,
                    triggerCards,
                    checkCount);
                return false;
            }

            if (checkCount > totalCards)
            {
                result = TriggerProbabilityResult.Invalid(
                    "CHECK_COUNT_EXCEEDS_TOTAL",
                    "Check count cannot exceed total cards.",
                    totalCards,
                    triggerCards,
                    checkCount);
                return false;
            }

            result = null;
            return true;
        }

        private static double LogCombination(int totalCards, int chooseCards)
        {
            if (chooseCards < 0 || chooseCards > totalCards)
            {
                return double.NegativeInfinity;
            }

            int smallerChoose = Math.Min(chooseCards, totalCards - chooseCards);
            double value = 0d;
            for (int i = 1; i <= smallerChoose; i++)
            {
                value += Math.Log(totalCards - smallerChoose + i) - Math.Log(i);
            }

            return value;
        }
    }
}
