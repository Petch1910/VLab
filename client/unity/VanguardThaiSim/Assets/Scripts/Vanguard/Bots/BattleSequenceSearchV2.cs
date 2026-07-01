using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class BattleSequenceSearchV2Options
    {
        public BattleSequenceSearchOptions BattleOptions = BattleSequenceSearchOptions.CreateDefault();
        public int ShieldStep = 5000;
        public double GuardDrainPer5000Weight = 0.25d;
        public double GuardBreakPer5000Weight = 0.75d;
        public double EarlyAttackPressureDecay = 0.15d;

        public static BattleSequenceSearchV2Options CreateDefault()
        {
            return new BattleSequenceSearchV2Options();
        }
    }

    public sealed class BattleSequenceV2Candidate
    {
        private readonly List<BattleAttackCandidate> attackers;

        public string CandidateId { get; private set; }
        public double BaseScore { get; private set; }
        public double GuardPressureContribution { get; private set; }
        public double TriggerPressureContribution { get; private set; }
        public double TriggerRisk { get; private set; }
        public double TotalScore { get; private set; }
        public string Explanation { get; private set; }
        public IReadOnlyList<BattleAttackCandidate> Attackers
        {
            get { return attackers; }
        }

        public BattleSequenceV2Candidate(
            string candidateId,
            double baseScore,
            double guardPressureContribution,
            double triggerPressureContribution,
            double triggerRisk,
            double totalScore,
            string explanation,
            List<BattleAttackCandidate> attackers)
        {
            CandidateId = candidateId ?? string.Empty;
            BaseScore = baseScore;
            GuardPressureContribution = guardPressureContribution;
            TriggerPressureContribution = triggerPressureContribution;
            TriggerRisk = triggerRisk;
            TotalScore = totalScore;
            Explanation = explanation ?? string.Empty;
            this.attackers = attackers ?? new List<BattleAttackCandidate>();
        }
    }

    public static class BattleSequenceSearchV2
    {
        public static IReadOnlyList<BattleSequenceV2Candidate> Search(
            GameState visibleState,
            int playerIndex,
            ICardRepository cardRepository,
            OpponentGuardEstimate guardEstimate = null,
            TriggerProbabilityResult triggerProbability = null,
            BattleSequenceSearchV2Options options = null)
        {
            if (visibleState == null)
            {
                throw new ArgumentNullException("visibleState");
            }

            BattleSequenceSearchV2Options safeOptions =
                options ?? BattleSequenceSearchV2Options.CreateDefault();
            TriggerProbabilityResult safeProbability =
                triggerProbability != null && triggerProbability.IsValid ? triggerProbability : null;
            IReadOnlyList<BattleSequenceCandidate> baseCandidates = BattleSequenceSearch.Search(
                visibleState,
                playerIndex,
                cardRepository,
                null,
                safeProbability,
                safeOptions.BattleOptions);
            double triggerRisk = safeProbability == null
                ? 0d
                : Math.Max(0d, Math.Min(1d, safeProbability.ProbabilityAtLeastOneTrigger));

            var candidates = new List<BattleSequenceV2Candidate>(baseCandidates.Count);
            for (int i = 0; i < baseCandidates.Count; i++)
            {
                BattleSequenceCandidate baseCandidate = baseCandidates[i];
                double guardContribution = CalculateGuardPressure(
                    baseCandidate.Attackers,
                    guardEstimate,
                    safeOptions);
                double totalScore = baseCandidate.TotalScore + guardContribution;
                string explanation =
                    "battle-v2: " + baseCandidate.CandidateId +
                    " base=" + baseCandidate.TotalScore.ToString("0.###") +
                    " guard=" + guardContribution.ToString("0.###") +
                    " trigger=" + baseCandidate.TriggerPressureContribution.ToString("0.###") +
                    " total=" + totalScore.ToString("0.###");

                candidates.Add(new BattleSequenceV2Candidate(
                    baseCandidate.CandidateId,
                    baseCandidate.TotalScore,
                    guardContribution,
                    baseCandidate.TriggerPressureContribution,
                    triggerRisk,
                    totalScore,
                    explanation,
                    CloneAttackers(baseCandidate.Attackers)));
            }

            candidates.Sort(CompareCandidates);
            return candidates;
        }

        private static double CalculateGuardPressure(
            IReadOnlyList<BattleAttackCandidate> attackers,
            OpponentGuardEstimate guardEstimate,
            BattleSequenceSearchV2Options options)
        {
            if (attackers == null || guardEstimate == null)
            {
                return 0d;
            }

            int expectedShield = Math.Max(0, guardEstimate.ExpectedShieldEstimate);
            double weightedShieldDemand = 0d;
            int cumulativeShieldDemand = 0;
            for (int i = 0; i < attackers.Count; i++)
            {
                BattleAttackCandidate attacker = attackers[i];
                if (attacker == null)
                {
                    continue;
                }

                int shieldNeeded = GuardDecisionBot.CalculateShieldNeeded(
                    attacker.EstimatedPower,
                    options.BattleOptions.OpponentVanguardPower,
                    options.ShieldStep);
                cumulativeShieldDemand += shieldNeeded;
                double positionMultiplier = Math.Max(0.4d, 1d - (i * options.EarlyAttackPressureDecay));
                weightedShieldDemand += shieldNeeded * positionMultiplier;
            }

            double drainPressure =
                Math.Min(weightedShieldDemand, expectedShield) /
                Math.Max(1, options.ShieldStep) *
                options.GuardDrainPer5000Weight;
            double breakPressure =
                Math.Max(0d, cumulativeShieldDemand - expectedShield) /
                Math.Max(1, options.ShieldStep) *
                options.GuardBreakPer5000Weight;
            return drainPressure + breakPressure;
        }

        private static List<BattleAttackCandidate> CloneAttackers(IReadOnlyList<BattleAttackCandidate> attackers)
        {
            var clones = new List<BattleAttackCandidate>();
            if (attackers == null)
            {
                return clones;
            }

            for (int i = 0; i < attackers.Count; i++)
            {
                BattleAttackCandidate attacker = attackers[i];
                if (attacker == null)
                {
                    continue;
                }

                clones.Add(new BattleAttackCandidate(
                    attacker.CardInstanceId,
                    attacker.CardId,
                    attacker.Zone,
                    attacker.ZoneIndex,
                    attacker.EstimatedPower));
            }

            return clones;
        }

        private static int CompareCandidates(BattleSequenceV2Candidate left, BattleSequenceV2Candidate right)
        {
            int scoreCompare = right.TotalScore.CompareTo(left.TotalScore);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            return string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
