using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class BattleSequenceSearchOptions
    {
        public int OpponentVanguardPower = 13000;
        public int MaxCandidates = 4;
        public double BoardScoreWeight = 0.05d;
        public double AttackPowerPer10000Weight = 1.0d;
        public double GuardBreakPer5000Weight = 1.0d;
        public double VanguardTriggerPressureWeight = 2.0d;

        public static BattleSequenceSearchOptions CreateDefault()
        {
            return new BattleSequenceSearchOptions();
        }
    }

    public sealed class BattleAttackCandidate
    {
        public string CardInstanceId { get; private set; }
        public string CardId { get; private set; }
        public GameZone Zone { get; private set; }
        public int ZoneIndex { get; private set; }
        public int EstimatedPower { get; private set; }

        public BattleAttackCandidate(
            string cardInstanceId,
            string cardId,
            GameZone zone,
            int zoneIndex,
            int estimatedPower)
        {
            CardInstanceId = cardInstanceId ?? string.Empty;
            CardId = cardId ?? string.Empty;
            Zone = zone;
            ZoneIndex = zoneIndex;
            EstimatedPower = estimatedPower;
        }
    }

    public sealed class BattleSequenceCandidate
    {
        private readonly List<BattleAttackCandidate> attackers;

        public string CandidateId { get; private set; }
        public double TotalScore { get; private set; }
        public double BoardScoreContribution { get; private set; }
        public double TriggerPressureContribution { get; private set; }
        public string Explanation { get; private set; }
        public IReadOnlyList<BattleAttackCandidate> Attackers
        {
            get { return attackers; }
        }

        public BattleSequenceCandidate(
            string candidateId,
            double totalScore,
            double boardScoreContribution,
            double triggerPressureContribution,
            string explanation,
            List<BattleAttackCandidate> attackers)
        {
            CandidateId = candidateId ?? string.Empty;
            TotalScore = totalScore;
            BoardScoreContribution = boardScoreContribution;
            TriggerPressureContribution = triggerPressureContribution;
            Explanation = explanation ?? string.Empty;
            this.attackers = attackers ?? new List<BattleAttackCandidate>();
        }
    }

    public static class BattleSequenceSearch
    {
        public static IReadOnlyList<BattleSequenceCandidate> Search(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository,
            BoardResourceEvaluation boardEvaluation = null,
            TriggerProbabilityResult triggerProbability = null,
            BattleSequenceSearchOptions options = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            BattleSequenceSearchOptions safeOptions = options ?? BattleSequenceSearchOptions.CreateDefault();
            if (safeOptions.MaxCandidates <= 0)
            {
                return new List<BattleSequenceCandidate>();
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            List<BattleAttackCandidate> attackers = CollectVisibleAttackers(player, cardRepository);
            if (attackers.Count == 0)
            {
                return new List<BattleSequenceCandidate>();
            }

            BoardResourceEvaluation safeBoardEvaluation = boardEvaluation ??
                BoardResourceEvaluator.Evaluate(state, playerIndex, cardRepository);

            var candidates = new List<BattleSequenceCandidate>();
            var seenOrderKeys = new HashSet<string>();

            AddCandidate(candidates, seenOrderKeys, "high_power_first", SortByPower(attackers, true), safeBoardEvaluation, triggerProbability, safeOptions);
            AddCandidate(candidates, seenOrderKeys, "low_power_first", SortByPower(attackers, false), safeBoardEvaluation, triggerProbability, safeOptions);
            AddCandidate(candidates, seenOrderKeys, "vanguard_first", OrderByZone(attackers, true), safeBoardEvaluation, triggerProbability, safeOptions);
            AddCandidate(candidates, seenOrderKeys, "rear_guards_first", OrderByZone(attackers, false), safeBoardEvaluation, triggerProbability, safeOptions);

            candidates.Sort(CompareCandidates);
            if (candidates.Count <= safeOptions.MaxCandidates)
            {
                return candidates;
            }

            return candidates.GetRange(0, safeOptions.MaxCandidates);
        }

        private static List<BattleAttackCandidate> CollectVisibleAttackers(
            PlayerGameState player,
            ICardRepository cardRepository)
        {
            var attackers = new List<BattleAttackCandidate>();
            AddZoneAttackers(attackers, player.vanguard, GameZone.Vanguard, cardRepository);
            AddZoneAttackers(attackers, player.rear_guard, GameZone.RearGuard, cardRepository);
            return attackers;
        }

        private static void AddZoneAttackers(
            List<BattleAttackCandidate> attackers,
            IList<GameCardInstance> cards,
            GameZone zone,
            ICardRepository cardRepository)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (!IsVisibleKnownCard(card))
                {
                    continue;
                }

                attackers.Add(new BattleAttackCandidate(
                    card.instance_id,
                    card.card_id,
                    zone,
                    i,
                    EstimatePower(card, cardRepository)));
            }
        }

        private static int EstimatePower(GameCardInstance card, ICardRepository cardRepository)
        {
            int basePower = 0;
            if (cardRepository != null)
            {
                CardDetail detail = cardRepository.GetCard(card.card_id);
                if (detail != null && detail.Power.HasValue)
                {
                    basePower = detail.Power.Value;
                }
            }

            return basePower + card.power_delta;
        }

        private static List<BattleAttackCandidate> SortByPower(
            List<BattleAttackCandidate> attackers,
            bool descending)
        {
            var ordered = new List<BattleAttackCandidate>(attackers);
            ordered.Sort(delegate(BattleAttackCandidate left, BattleAttackCandidate right)
            {
                int powerCompare = descending
                    ? right.EstimatedPower.CompareTo(left.EstimatedPower)
                    : left.EstimatedPower.CompareTo(right.EstimatedPower);
                if (powerCompare != 0)
                {
                    return powerCompare;
                }

                int zoneCompare = ZoneRank(left.Zone).CompareTo(ZoneRank(right.Zone));
                if (zoneCompare != 0)
                {
                    return zoneCompare;
                }

                return left.ZoneIndex.CompareTo(right.ZoneIndex);
            });
            return ordered;
        }

        private static List<BattleAttackCandidate> OrderByZone(
            List<BattleAttackCandidate> attackers,
            bool vanguardFirst)
        {
            var ordered = new List<BattleAttackCandidate>(attackers);
            ordered.Sort(delegate(BattleAttackCandidate left, BattleAttackCandidate right)
            {
                int leftRank = ZoneOrderRank(left.Zone, vanguardFirst);
                int rightRank = ZoneOrderRank(right.Zone, vanguardFirst);
                int zoneCompare = leftRank.CompareTo(rightRank);
                if (zoneCompare != 0)
                {
                    return zoneCompare;
                }

                int powerCompare = right.EstimatedPower.CompareTo(left.EstimatedPower);
                if (powerCompare != 0)
                {
                    return powerCompare;
                }

                return left.ZoneIndex.CompareTo(right.ZoneIndex);
            });
            return ordered;
        }

        private static void AddCandidate(
            List<BattleSequenceCandidate> candidates,
            HashSet<string> seenOrderKeys,
            string candidateId,
            List<BattleAttackCandidate> orderedAttackers,
            BoardResourceEvaluation boardEvaluation,
            TriggerProbabilityResult triggerProbability,
            BattleSequenceSearchOptions options)
        {
            string orderKey = BuildOrderKey(orderedAttackers);
            if (!seenOrderKeys.Add(orderKey))
            {
                return;
            }

            double boardScoreContribution = boardEvaluation.TotalScore * options.BoardScoreWeight;
            double triggerContribution = 0d;
            double totalScore = boardScoreContribution;
            for (int i = 0; i < orderedAttackers.Count; i++)
            {
                BattleAttackCandidate attacker = orderedAttackers[i];
                double positionMultiplier = Math.Max(0.5d, 1d - (i * 0.05d));
                double powerPressure = (attacker.EstimatedPower / 10000d) *
                    options.AttackPowerPer10000Weight *
                    positionMultiplier;
                double guardBreakPressure = Math.Max(0, attacker.EstimatedPower - options.OpponentVanguardPower) /
                    5000d *
                    options.GuardBreakPer5000Weight *
                    positionMultiplier;
                double attackerTriggerPressure = 0d;
                if (attacker.Zone == GameZone.Vanguard &&
                    triggerProbability != null &&
                    triggerProbability.IsValid)
                {
                    attackerTriggerPressure =
                        triggerProbability.ProbabilityAtLeastOneTrigger *
                        options.VanguardTriggerPressureWeight *
                        positionMultiplier;
                    triggerContribution += attackerTriggerPressure;
                }

                totalScore += powerPressure + guardBreakPressure + attackerTriggerPressure;
            }

            string explanation =
                candidateId +
                ": attackers=" + orderedAttackers.Count +
                ", board=" + boardScoreContribution.ToString("0.###") +
                ", trigger=" + triggerContribution.ToString("0.###") +
                ", total=" + totalScore.ToString("0.###");

            candidates.Add(new BattleSequenceCandidate(
                candidateId,
                totalScore,
                boardScoreContribution,
                triggerContribution,
                explanation,
                orderedAttackers));
        }

        private static string BuildOrderKey(List<BattleAttackCandidate> attackers)
        {
            var parts = new List<string>(attackers.Count);
            for (int i = 0; i < attackers.Count; i++)
            {
                parts.Add(attackers[i].CardInstanceId);
            }

            return string.Join("|", parts.ToArray());
        }

        private static int CompareCandidates(BattleSequenceCandidate left, BattleSequenceCandidate right)
        {
            int scoreCompare = right.TotalScore.CompareTo(left.TotalScore);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            return string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }

        private static int ZoneOrderRank(GameZone zone, bool vanguardFirst)
        {
            if (zone == GameZone.Vanguard)
            {
                return vanguardFirst ? 0 : 1;
            }

            return vanguardFirst ? 1 : 0;
        }

        private static int ZoneRank(GameZone zone)
        {
            return zone == GameZone.Vanguard ? 0 : 1;
        }

        private static bool IsVisibleKnownCard(GameCardInstance card)
        {
            return card != null &&
                   card.face_up &&
                   !string.IsNullOrEmpty(card.card_id) &&
                   card.card_id != GameStateViewFactory.HiddenCardId;
        }
    }
}
