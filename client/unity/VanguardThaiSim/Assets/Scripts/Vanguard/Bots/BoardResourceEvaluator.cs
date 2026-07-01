using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class BoardEvaluationWeights
    {
        public double HandCardScore = 1.0d;
        public double HandShieldPer10000Score = 0.5d;
        public double VanguardCardScore = 1.0d;
        public double RearGuardCardScore = 0.8d;
        public double AvailableCounterBlastScore = 0.7d;
        public double EstimatedSoulScore = 0.25d;
        public double DamagePenalty = -0.75d;
        public double FiveDamageExtraPenalty = -2.0d;
        public double LethalDamagePenalty = -100.0d;
        public double DeckCardScore = 0.02d;

        public static BoardEvaluationWeights CreateDefault()
        {
            return new BoardEvaluationWeights();
        }
    }

    public sealed class BoardEvaluationTerm
    {
        public string Key { get; private set; }
        public double RawValue { get; private set; }
        public double Weight { get; private set; }
        public double Score { get; private set; }
        public string Explanation { get; private set; }

        public BoardEvaluationTerm(
            string key,
            double rawValue,
            double weight,
            double score,
            string explanation)
        {
            Key = key ?? string.Empty;
            RawValue = rawValue;
            Weight = weight;
            Score = score;
            Explanation = explanation ?? string.Empty;
        }
    }

    public sealed class BoardResourceEvaluation
    {
        private readonly List<BoardEvaluationTerm> terms;

        public int PlayerIndex { get; private set; }
        public double TotalScore { get; private set; }
        public int HandCount { get; private set; }
        public int VisibleHandShield { get; private set; }
        public int UnknownHandStatCount { get; private set; }
        public int VanguardCount { get; private set; }
        public int RearGuardCount { get; private set; }
        public int DamageCount { get; private set; }
        public int AvailableCounterBlastCount { get; private set; }
        public int EstimatedSoulCount { get; private set; }
        public int DeckCount { get; private set; }
        public IReadOnlyList<BoardEvaluationTerm> Terms
        {
            get { return terms; }
        }

        public BoardResourceEvaluation(
            int playerIndex,
            double totalScore,
            int handCount,
            int visibleHandShield,
            int unknownHandStatCount,
            int vanguardCount,
            int rearGuardCount,
            int damageCount,
            int availableCounterBlastCount,
            int estimatedSoulCount,
            int deckCount,
            List<BoardEvaluationTerm> terms)
        {
            PlayerIndex = playerIndex;
            TotalScore = totalScore;
            HandCount = handCount;
            VisibleHandShield = visibleHandShield;
            UnknownHandStatCount = unknownHandStatCount;
            VanguardCount = vanguardCount;
            RearGuardCount = rearGuardCount;
            DamageCount = damageCount;
            AvailableCounterBlastCount = availableCounterBlastCount;
            EstimatedSoulCount = estimatedSoulCount;
            DeckCount = deckCount;
            this.terms = terms ?? new List<BoardEvaluationTerm>();
        }

        public BoardEvaluationTerm FindTerm(string key)
        {
            for (int i = 0; i < terms.Count; i++)
            {
                if (terms[i].Key == key)
                {
                    return terms[i];
                }
            }

            return null;
        }
    }

    public static class BoardResourceEvaluator
    {
        public static BoardResourceEvaluation Evaluate(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository,
            BoardEvaluationWeights weights = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            BoardEvaluationWeights safeWeights = weights ?? BoardEvaluationWeights.CreateDefault();
            PlayerGameState player = state.GetPlayer(playerIndex);
            var terms = new List<BoardEvaluationTerm>();

            int handCount = player.hand.Count;
            int visibleHandShield = CalculateVisibleHandShield(player.hand, cardRepository, out int unknownHandStatCount);
            int vanguardCount = player.vanguard.Count;
            int rearGuardCount = player.rear_guard.Count;
            int damageCount = player.damage.Count;
            int availableCounterBlast = CountFaceUp(player.damage);
            int estimatedSoul = Math.Max(0, vanguardCount - 1);
            int deckCount = player.deck.Count;

            AddWeightedTerm(
                terms,
                "hand_count",
                handCount,
                safeWeights.HandCardScore,
                "Cards in hand are flexible resources and future guard options.");
            AddWeightedTerm(
                terms,
                "visible_hand_shield",
                visibleHandShield / 10000d,
                safeWeights.HandShieldPer10000Score,
                "Visible shield in hand improves defensive stability.");
            AddWeightedTerm(
                terms,
                "vanguard_count",
                vanguardCount,
                safeWeights.VanguardCardScore,
                "Vanguard stack presence is required for pressure and estimated soul.");
            AddWeightedTerm(
                terms,
                "rear_guard_count",
                rearGuardCount,
                safeWeights.RearGuardCardScore,
                "Rear-guards increase attack and boost options.");
            AddWeightedTerm(
                terms,
                "available_counter_blast",
                availableCounterBlast,
                safeWeights.AvailableCounterBlastScore,
                "Face-up damage can pay Counter-Blast costs.");
            AddWeightedTerm(
                terms,
                "estimated_soul",
                estimatedSoul,
                safeWeights.EstimatedSoulScore,
                "Stacked vanguard cards approximate soul until a dedicated Soul zone exists.");
            AddWeightedTerm(
                terms,
                "damage_count",
                damageCount,
                safeWeights.DamagePenalty,
                "Damage is both resource and loss pressure; high damage is risky.");
            if (damageCount >= 5)
            {
                AddFlatTerm(
                    terms,
                    "five_damage_pressure",
                    damageCount,
                    safeWeights.FiveDamageExtraPenalty,
                    "At five damage, the player is in lethal range and should value defense higher.");
            }

            if (damageCount >= 6)
            {
                AddFlatTerm(
                    terms,
                    "lethal_damage",
                    damageCount,
                    safeWeights.LethalDamagePenalty,
                    "Six damage is losing territory in normal Vanguard rules.");
            }

            AddWeightedTerm(
                terms,
                "deck_count",
                deckCount,
                safeWeights.DeckCardScore,
                "Remaining deck count is a small endurance signal.");

            double totalScore = 0d;
            for (int i = 0; i < terms.Count; i++)
            {
                totalScore += terms[i].Score;
            }

            return new BoardResourceEvaluation(
                playerIndex,
                totalScore,
                handCount,
                visibleHandShield,
                unknownHandStatCount,
                vanguardCount,
                rearGuardCount,
                damageCount,
                availableCounterBlast,
                estimatedSoul,
                deckCount,
                terms);
        }

        private static int CalculateVisibleHandShield(
            IList<GameCardInstance> hand,
            ICardRepository cardRepository,
            out int unknownCount)
        {
            int shield = 0;
            unknownCount = 0;
            if (hand == null)
            {
                return shield;
            }

            for (int i = 0; i < hand.Count; i++)
            {
                GameCardInstance card = hand[i];
                if (!IsVisibleKnownCard(card) || cardRepository == null)
                {
                    unknownCount++;
                    continue;
                }

                CardDetail detail = cardRepository.GetCard(card.card_id);
                if (detail == null || !detail.Shield.HasValue)
                {
                    unknownCount++;
                    continue;
                }

                shield += Math.Max(0, detail.Shield.Value);
            }

            return shield;
        }

        private static int CountFaceUp(IList<GameCardInstance> cards)
        {
            if (cards == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null && cards[i].face_up)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsVisibleKnownCard(GameCardInstance card)
        {
            return card != null &&
                   card.face_up &&
                   !string.IsNullOrEmpty(card.card_id) &&
                   card.card_id != GameStateViewFactory.HiddenCardId;
        }

        private static void AddWeightedTerm(
            List<BoardEvaluationTerm> terms,
            string key,
            double rawValue,
            double weight,
            string explanation)
        {
            terms.Add(new BoardEvaluationTerm(
                key,
                rawValue,
                weight,
                rawValue * weight,
                explanation));
        }

        private static void AddFlatTerm(
            List<BoardEvaluationTerm> terms,
            string key,
            double rawValue,
            double score,
            string explanation)
        {
            terms.Add(new BoardEvaluationTerm(
                key,
                rawValue,
                1d,
                score,
                explanation));
        }
    }
}
