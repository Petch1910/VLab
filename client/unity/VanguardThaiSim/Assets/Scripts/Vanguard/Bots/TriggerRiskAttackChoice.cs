using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class TriggerRiskAttackChoiceOptions
    {
        public BattleSequenceSearchOptions BattleOptions = BattleSequenceSearchOptions.CreateDefault();

        public static TriggerRiskAttackChoiceOptions CreateDefault()
        {
            return new TriggerRiskAttackChoiceOptions();
        }
    }

    public sealed class TriggerRiskAttackChoiceResult
    {
        private readonly List<BattleAttackCandidate> attackers;

        public bool HasChoice { get; private set; }
        public string CandidateId { get; private set; }
        public double TotalScore { get; private set; }
        public double TriggerRisk { get; private set; }
        public double TriggerPressureContribution { get; private set; }
        public bool UsesProbabilityAsPlanningSignal { get; private set; }
        public bool AppliesTriggerOutcome { get; private set; }
        public string Reason { get; private set; }
        public IReadOnlyList<BattleAttackCandidate> Attackers
        {
            get { return attackers; }
        }

        public TriggerRiskAttackChoiceResult(
            bool hasChoice,
            string candidateId,
            double totalScore,
            double triggerRisk,
            double triggerPressureContribution,
            bool usesProbabilityAsPlanningSignal,
            bool appliesTriggerOutcome,
            string reason,
            List<BattleAttackCandidate> attackers)
        {
            HasChoice = hasChoice;
            CandidateId = candidateId ?? string.Empty;
            TotalScore = totalScore;
            TriggerRisk = triggerRisk;
            TriggerPressureContribution = triggerPressureContribution;
            UsesProbabilityAsPlanningSignal = usesProbabilityAsPlanningSignal;
            AppliesTriggerOutcome = appliesTriggerOutcome;
            Reason = reason ?? string.Empty;
            this.attackers = attackers ?? new List<BattleAttackCandidate>();
        }
    }

    public static class TriggerRiskAttackChoice
    {
        public static TriggerRiskAttackChoiceResult Choose(
            GameState visibleState,
            int playerIndex,
            ICardRepository cardRepository,
            TriggerProbabilityResult triggerProbability = null,
            TriggerRiskAttackChoiceOptions options = null)
        {
            if (visibleState == null)
            {
                throw new ArgumentNullException("visibleState");
            }

            TriggerRiskAttackChoiceOptions safeOptions =
                options ?? TriggerRiskAttackChoiceOptions.CreateDefault();
            TriggerProbabilityResult safeProbability =
                triggerProbability != null && triggerProbability.IsValid ? triggerProbability : null;
            IReadOnlyList<BattleSequenceCandidate> candidates = BattleSequenceSearch.Search(
                visibleState,
                playerIndex,
                cardRepository,
                null,
                safeProbability,
                safeOptions.BattleOptions);

            double triggerRisk = safeProbability == null
                ? 0d
                : Math.Max(0d, Math.Min(1d, safeProbability.ProbabilityAtLeastOneTrigger));
            bool usesProbability = safeProbability != null;

            if (candidates.Count == 0)
            {
                return new TriggerRiskAttackChoiceResult(
                    false,
                    string.Empty,
                    0d,
                    triggerRisk,
                    0d,
                    usesProbability,
                    false,
                    "trigger-risk-attack: no visible attackers",
                    new List<BattleAttackCandidate>());
            }

            BattleSequenceCandidate best = candidates[0];
            string reason =
                "trigger-risk-attack: " + best.CandidateId +
                " score=" + best.TotalScore.ToString("0.###") +
                " trigger=" + triggerRisk.ToString("0.###") +
                " trigger_pressure=" + best.TriggerPressureContribution.ToString("0.###") +
                " planning_only=True";

            return new TriggerRiskAttackChoiceResult(
                true,
                best.CandidateId,
                best.TotalScore,
                triggerRisk,
                best.TriggerPressureContribution,
                usesProbability,
                false,
                reason,
                CloneAttackers(best.Attackers));
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
    }
}
