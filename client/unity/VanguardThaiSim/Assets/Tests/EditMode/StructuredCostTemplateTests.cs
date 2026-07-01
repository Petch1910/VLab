using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredCostTemplateTests
    {
        [Test]
        public void BuildRequestAggregatesLedgerBackedCostsAndOnceKeys()
        {
            StructuredAbility ability = CreateAbility(
                new StructuredAbilityCost { type = "counter_blast", amount = 1 },
                new StructuredAbilityCost { type = "counter_blast", amount = 2 },
                new StructuredAbilityCost { type = "soul_blast", amount = 1 },
                new StructuredAbilityCost { type = "energy_blast", amount = 3 },
                new StructuredAbilityCost { type = "once_per_turn", amount = 1 },
                new StructuredAbilityCost { type = "once_per_fight", amount = 1, key = "custom-fight-key" });

            StructuredCostTemplateResult result = StructuredCostTemplate.BuildRequest(0, 7, ability);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(3, result.request.counter_blast);
            Assert.AreEqual(1, result.request.soul_blast);
            Assert.AreEqual(3, result.request.energy_blast);
            Assert.AreEqual(ResourceLedger.BuildOncePerTurnKey(ability.ability_id, 7), result.request.once_per_turn_key);
            Assert.AreEqual("custom-fight-key", result.request.once_per_fight_key);
        }

        [Test]
        public void ValidateAgainstLedgerUsesResourceLedgerWithoutMutatingLedger()
        {
            StructuredAbility ability = CreateAbility(
                new StructuredAbilityCost { type = "counter_blast", amount = 1 },
                new StructuredAbilityCost { type = "soul_blast", amount = 1 },
                new StructuredAbilityCost { type = "once_per_turn", amount = 1 });
            ResourceLedgerState ledger = new ResourceLedgerState
            {
                player_index = 0,
                available_counter_blast = 2,
                available_soul = 1,
                available_energy = 0
            };
            string before = ledger.ToJson(false);

            ResourceLedgerValidationResult result =
                StructuredCostTemplate.ValidateAgainstLedger(ledger, 0, 2, ability);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.after_state.available_counter_blast);
            Assert.AreEqual(0, result.after_state.available_soul);
            Assert.AreEqual(before, ledger.ToJson(false));
        }

        [Test]
        public void DiscardCostRequiresManualResolutionPlaceholder()
        {
            StructuredAbility ability = CreateAbility(
                new StructuredAbilityCost { type = "discard", amount = 1 });

            StructuredCostTemplateResult result = StructuredCostTemplate.BuildRequest(0, 1, ability);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.requires_manual_resolution);
            Assert.IsTrue(result.rejection_reason.StartsWith(StructuredCostTemplateRejectionReasons.UnsupportedCostType));
        }

        [Test]
        public void NegativeCostRejectsBeforeLedgerValidation()
        {
            StructuredAbility ability = CreateAbility(
                new StructuredAbilityCost { type = "counter_blast", amount = -1 });

            StructuredCostTemplateResult result = StructuredCostTemplate.BuildRequest(0, 1, ability);
            ResourceLedgerValidationResult validation =
                StructuredCostTemplate.ValidateAgainstLedger(new ResourceLedgerState { player_index = 0 }, 0, 1, ability);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(StructuredCostTemplateRejectionReasons.NegativeAmount, result.rejection_reason);
            Assert.IsFalse(validation.accepted);
            Assert.AreEqual(StructuredCostTemplateRejectionReasons.NegativeAmount, validation.rejection_reason);
        }

        [Test]
        public void MultipleOnceFlagsReject()
        {
            StructuredAbility turnAbility = CreateAbility(
                new StructuredAbilityCost { type = "once_per_turn", amount = 1, key = "turn-a" },
                new StructuredAbilityCost { type = "once_per_turn", amount = 1, key = "turn-b" });
            StructuredAbility fightAbility = CreateAbility(
                new StructuredAbilityCost { type = "once_per_fight", amount = 1, key = "fight-a" },
                new StructuredAbilityCost { type = "once_per_fight", amount = 1, key = "fight-b" });

            StructuredCostTemplateResult turn = StructuredCostTemplate.BuildRequest(0, 1, turnAbility);
            StructuredCostTemplateResult fight = StructuredCostTemplate.BuildRequest(0, 1, fightAbility);

            Assert.IsFalse(turn.accepted);
            Assert.AreEqual(StructuredCostTemplateRejectionReasons.MultipleOncePerTurn, turn.rejection_reason);
            Assert.IsFalse(fight.accepted);
            Assert.AreEqual(StructuredCostTemplateRejectionReasons.MultipleOncePerFight, fight.rejection_reason);
        }

        [Test]
        public void TemplateResultRoundTripsJson()
        {
            StructuredCostTemplateResult result =
                StructuredCostTemplate.BuildRequest(
                    0,
                    4,
                    CreateAbility(new StructuredAbilityCost { type = "counter_blast", amount = 1 }));

            StructuredCostTemplateResult roundTrip =
                StructuredCostTemplateResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.request.counter_blast, roundTrip.request.counter_blast);
            Assert.AreEqual(result.requires_manual_resolution, roundTrip.requires_manual_resolution);
        }

        private static StructuredAbility CreateAbility(params StructuredAbilityCost[] costs)
        {
            return new StructuredAbility
            {
                ability_id = "ability-cost-template",
                card_id = "CARD-001",
                kind = "auto",
                manual_fallback = true,
                costs = new List<StructuredAbilityCost>(costs),
                targets = new List<StructuredAbilityTarget>(),
                effects = new List<StructuredAbilityEffect>
                {
                    new StructuredAbilityEffect { type = "manual" }
                },
                trigger = new StructuredAbilityTrigger { type = "manual" },
                timing = new StructuredAbilityTiming { phase = "Main", window = "PendingAutoResolution" },
                duration = new StructuredAbilityDuration { type = "manual", cleanup_timing = "manual" }
            };
        }
    }
}
