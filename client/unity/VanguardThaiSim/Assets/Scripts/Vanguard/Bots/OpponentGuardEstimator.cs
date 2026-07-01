using System;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class OpponentGuardEstimatorOptions
    {
        public int AverageUnknownShield = 10000;
        public int MaximumUnknownShield = 15000;
        public double VisibleCardConfidence = 1.0d;
        public double UnknownCardConfidence = 0.35d;

        public static OpponentGuardEstimatorOptions CreateDefault()
        {
            return new OpponentGuardEstimatorOptions();
        }
    }

    public sealed class OpponentGuardEstimate
    {
        public int PlayerIndex { get; private set; }
        public int HandCount { get; private set; }
        public int VisibleKnownShield { get; private set; }
        public int UnknownHandCardCount { get; private set; }
        public int ConservativeShieldEstimate { get; private set; }
        public int ExpectedShieldEstimate { get; private set; }
        public int MaximumShieldEstimate { get; private set; }
        public double Confidence { get; private set; }
        public string Explanation { get; private set; }

        public OpponentGuardEstimate(
            int playerIndex,
            int handCount,
            int visibleKnownShield,
            int unknownHandCardCount,
            int conservativeShieldEstimate,
            int expectedShieldEstimate,
            int maximumShieldEstimate,
            double confidence,
            string explanation)
        {
            PlayerIndex = playerIndex;
            HandCount = handCount;
            VisibleKnownShield = visibleKnownShield;
            UnknownHandCardCount = unknownHandCardCount;
            ConservativeShieldEstimate = conservativeShieldEstimate;
            ExpectedShieldEstimate = expectedShieldEstimate;
            MaximumShieldEstimate = maximumShieldEstimate;
            Confidence = confidence;
            Explanation = explanation ?? string.Empty;
        }
    }

    public static class OpponentGuardEstimator
    {
        public static OpponentGuardEstimate Estimate(
            GameState state,
            int opponentPlayerIndex,
            ICardRepository cardRepository,
            OpponentGuardEstimatorOptions options = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            OpponentGuardEstimatorOptions safeOptions = options ?? OpponentGuardEstimatorOptions.CreateDefault();
            PlayerGameState opponent = state.GetPlayer(opponentPlayerIndex);
            int handCount = opponent.hand.Count;
            int visibleKnownShield = 0;
            int visibleKnownCount = 0;
            int unknownCount = 0;

            for (int i = 0; i < opponent.hand.Count; i++)
            {
                GameCardInstance card = opponent.hand[i];
                if (!TryReadVisibleShield(card, cardRepository, out int shield))
                {
                    unknownCount++;
                    continue;
                }

                visibleKnownShield += shield;
                visibleKnownCount++;
            }

            int averageUnknownShield = Math.Max(0, safeOptions.AverageUnknownShield);
            int maximumUnknownShield = Math.Max(averageUnknownShield, safeOptions.MaximumUnknownShield);
            int conservativeShield = visibleKnownShield;
            int expectedShield = visibleKnownShield + (unknownCount * averageUnknownShield);
            int maximumShield = visibleKnownShield + (unknownCount * maximumUnknownShield);
            double confidence = handCount == 0
                ? 1d
                : ((visibleKnownCount * safeOptions.VisibleCardConfidence) +
                   (unknownCount * safeOptions.UnknownCardConfidence)) / handCount;
            confidence = Math.Max(0d, Math.Min(1d, confidence));

            string explanation =
                "hand=" + handCount +
                ", known_shield=" + visibleKnownShield +
                ", unknown=" + unknownCount +
                ", expected=" + expectedShield +
                ", confidence=" + confidence.ToString("0.###");

            return new OpponentGuardEstimate(
                opponentPlayerIndex,
                handCount,
                visibleKnownShield,
                unknownCount,
                conservativeShield,
                expectedShield,
                maximumShield,
                confidence,
                explanation);
        }

        private static bool TryReadVisibleShield(
            GameCardInstance card,
            ICardRepository cardRepository,
            out int shield)
        {
            shield = 0;
            if (card == null ||
                !card.face_up ||
                string.IsNullOrEmpty(card.card_id) ||
                card.card_id == GameStateViewFactory.HiddenCardId ||
                cardRepository == null)
            {
                return false;
            }

            CardDetail detail = cardRepository.GetCard(card.card_id);
            if (detail == null || !detail.Shield.HasValue)
            {
                return false;
            }

            shield = Math.Max(0, detail.Shield.Value);
            return true;
        }
    }
}
