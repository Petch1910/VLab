using System;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public enum GuardDecisionType
    {
        NoGuard,
        Guard,
        PerfectGuardPreferred,
        CannotGuard
    }

    public sealed class GuardDecisionRequest
    {
        public int defender_player_index;
        public int attacker_player_index;
        public int attack_power;
        public int defending_power;
        public int incoming_critical = 1;
        public bool attack_can_gain_triggers = true;
    }

    public sealed class GuardDecisionOptions
    {
        public int ShieldStep = 5000;
        public int LethalDamage = 6;
        public int HighDamageThreshold = 4;
        public int TriggerRiskGuardDamageThreshold = 3;
        public double HighTriggerRiskThreshold = 0.35d;
        public int PerfectGuardPreferredShield = 30000;

        public static GuardDecisionOptions CreateDefault()
        {
            return new GuardDecisionOptions();
        }
    }

    public sealed class GuardDecisionResult
    {
        public GuardDecisionType Decision { get; private set; }
        public int DefenderPlayerIndex { get; private set; }
        public int ShieldNeeded { get; private set; }
        public int DefenderDamage { get; private set; }
        public int DamageAfterNoGuard { get; private set; }
        public int ExpectedShieldAvailable { get; private set; }
        public int MaximumShieldAvailable { get; private set; }
        public double TriggerRisk { get; private set; }
        public bool LethalRisk { get; private set; }
        public bool HighDamageRisk { get; private set; }
        public bool HighTriggerRisk { get; private set; }
        public string Reason { get; private set; }

        public GuardDecisionResult(
            GuardDecisionType decision,
            int defenderPlayerIndex,
            int shieldNeeded,
            int defenderDamage,
            int damageAfterNoGuard,
            int expectedShieldAvailable,
            int maximumShieldAvailable,
            double triggerRisk,
            bool lethalRisk,
            bool highDamageRisk,
            bool highTriggerRisk,
            string reason)
        {
            Decision = decision;
            DefenderPlayerIndex = defenderPlayerIndex;
            ShieldNeeded = shieldNeeded;
            DefenderDamage = defenderDamage;
            DamageAfterNoGuard = damageAfterNoGuard;
            ExpectedShieldAvailable = expectedShieldAvailable;
            MaximumShieldAvailable = maximumShieldAvailable;
            TriggerRisk = triggerRisk;
            LethalRisk = lethalRisk;
            HighDamageRisk = highDamageRisk;
            HighTriggerRisk = highTriggerRisk;
            Reason = reason ?? string.Empty;
        }
    }

    public static class GuardDecisionBot
    {
        public static GuardDecisionResult Decide(
            GameState visibleState,
            GuardDecisionRequest request,
            ICardRepository cardRepository,
            TriggerProbabilityResult triggerProbability = null,
            GuardDecisionOptions options = null,
            OpponentGuardEstimatorOptions guardEstimatorOptions = null)
        {
            if (visibleState == null)
            {
                throw new ArgumentNullException("visibleState");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            GuardDecisionOptions safeOptions = options ?? GuardDecisionOptions.CreateDefault();
            int defenderIndex = request.defender_player_index;
            PlayerGameState defender = visibleState.GetPlayer(defenderIndex);
            OpponentGuardEstimate shieldEstimate = OpponentGuardEstimator.Estimate(
                visibleState,
                defenderIndex,
                cardRepository,
                guardEstimatorOptions);

            int incomingCritical = Math.Max(1, request.incoming_critical);
            int shieldNeeded = CalculateShieldNeeded(
                request.attack_power,
                request.defending_power,
                safeOptions.ShieldStep);
            int defenderDamage = defender.damage.Count;
            int damageAfterNoGuard = defenderDamage + incomingCritical;
            double triggerRisk = CalculateTriggerRisk(request, triggerProbability);
            bool lethalRisk = damageAfterNoGuard >= safeOptions.LethalDamage;
            bool highDamageRisk = defenderDamage >= safeOptions.HighDamageThreshold;
            bool highTriggerRisk =
                request.attack_can_gain_triggers &&
                defenderDamage >= safeOptions.TriggerRiskGuardDamageThreshold &&
                triggerRisk >= safeOptions.HighTriggerRiskThreshold;

            GuardDecisionType decision = ChooseDecision(
                shieldNeeded,
                shieldEstimate.ExpectedShieldEstimate,
                shieldEstimate.MaximumShieldEstimate,
                lethalRisk,
                highDamageRisk,
                highTriggerRisk,
                safeOptions);

            string reason = BuildReason(
                decision,
                shieldNeeded,
                defenderDamage,
                damageAfterNoGuard,
                shieldEstimate.ExpectedShieldEstimate,
                shieldEstimate.MaximumShieldEstimate,
                triggerRisk,
                lethalRisk,
                highDamageRisk,
                highTriggerRisk);

            return new GuardDecisionResult(
                decision,
                defenderIndex,
                shieldNeeded,
                defenderDamage,
                damageAfterNoGuard,
                shieldEstimate.ExpectedShieldEstimate,
                shieldEstimate.MaximumShieldEstimate,
                triggerRisk,
                lethalRisk,
                highDamageRisk,
                highTriggerRisk,
                reason);
        }

        public static int CalculateShieldNeeded(int attackPower, int defendingPower, int shieldStep = 5000)
        {
            int rawNeeded = attackPower - defendingPower + 1;
            if (rawNeeded <= 0)
            {
                return 0;
            }

            int safeStep = Math.Max(1, shieldStep);
            return ((rawNeeded + safeStep - 1) / safeStep) * safeStep;
        }

        private static double CalculateTriggerRisk(
            GuardDecisionRequest request,
            TriggerProbabilityResult triggerProbability)
        {
            if (request == null ||
                !request.attack_can_gain_triggers ||
                triggerProbability == null ||
                !triggerProbability.IsValid)
            {
                return 0d;
            }

            return Math.Max(0d, Math.Min(1d, triggerProbability.ProbabilityAtLeastOneTrigger));
        }

        private static GuardDecisionType ChooseDecision(
            int shieldNeeded,
            int expectedShield,
            int maximumShield,
            bool lethalRisk,
            bool highDamageRisk,
            bool highTriggerRisk,
            GuardDecisionOptions options)
        {
            if (shieldNeeded <= 0)
            {
                return GuardDecisionType.NoGuard;
            }

            bool shouldGuard = lethalRisk || highDamageRisk || highTriggerRisk;
            if (!shouldGuard)
            {
                return GuardDecisionType.NoGuard;
            }

            if (maximumShield < shieldNeeded)
            {
                return GuardDecisionType.CannotGuard;
            }

            if (expectedShield < shieldNeeded || shieldNeeded >= options.PerfectGuardPreferredShield)
            {
                return GuardDecisionType.PerfectGuardPreferred;
            }

            return GuardDecisionType.Guard;
        }

        private static string BuildReason(
            GuardDecisionType decision,
            int shieldNeeded,
            int defenderDamage,
            int damageAfterNoGuard,
            int expectedShield,
            int maximumShield,
            double triggerRisk,
            bool lethalRisk,
            bool highDamageRisk,
            bool highTriggerRisk)
        {
            return
                "guard-v1: " + decision +
                " shield=" + shieldNeeded +
                " damage=" + defenderDamage + "->" + damageAfterNoGuard +
                " expected=" + expectedShield +
                " max=" + maximumShield +
                " trigger=" + triggerRisk.ToString("0.###") +
                " lethal=" + lethalRisk +
                " high_damage=" + highDamageRisk +
                " high_trigger=" + highTriggerRisk;
        }
    }
}
