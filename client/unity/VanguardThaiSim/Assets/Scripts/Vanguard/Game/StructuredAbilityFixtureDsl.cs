using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredAbilityFixtureRejectionReasons
    {
        public const string FixtureMissing = "STRUCTURED_ABILITY_FIXTURE_MISSING";
        public const string BeforeStateMissing = "STRUCTURED_ABILITY_FIXTURE_BEFORE_STATE_MISSING";
        public const string AbilityMissing = "STRUCTURED_ABILITY_FIXTURE_ABILITY_MISSING";
        public const string CostRejected = "STRUCTURED_ABILITY_FIXTURE_COST_REJECTED";
        public const string TargetRejected = "STRUCTURED_ABILITY_FIXTURE_TARGET_REJECTED";
        public const string EffectRejected = "STRUCTURED_ABILITY_FIXTURE_EFFECT_REJECTED";
        public const string ModifierRejected = "STRUCTURED_ABILITY_FIXTURE_MODIFIER_REJECTED";
    }

    [Serializable]
    public sealed class StructuredAbilityFixtureExpectation
    {
        public bool check_accepted;
        public bool expected_accepted;
        public bool check_event_count;
        public int expected_event_count;
        public bool check_modifier_count;
        public int expected_modifier_count;
        public bool check_player_hand_count;
        public int expected_player_hand_count;
        public bool check_player_deck_count;
        public int expected_player_deck_count;
        public bool check_damage_face_up_count;
        public int expected_damage_face_up_count;
    }

    [Serializable]
    public sealed class StructuredAbilityFixture
    {
        public string fixture_id;
        public string description;
        public int player_index;
        public GameState before_state;
        public StructuredAbility ability;
        public ResourceLedgerState resource_ledger;
        public CombatModifierLedger combat_modifier_ledger = new CombatModifierLedger();
        public StructuredTargetCandidate selected_target;
        public StructuredAbilityFixtureExpectation expected = new StructuredAbilityFixtureExpectation();

        public void EnsureObjects()
        {
            if (combat_modifier_ledger == null)
            {
                combat_modifier_ledger = new CombatModifierLedger();
            }

            combat_modifier_ledger.EnsureLists();
            if (expected == null)
            {
                expected = new StructuredAbilityFixtureExpectation();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureObjects();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredAbilityFixture FromJson(string json)
        {
            StructuredAbilityFixture fixture = JsonUtility.FromJson<StructuredAbilityFixture>(json);
            if (fixture == null)
            {
                throw new ArgumentException("Structured ability fixture JSON could not be parsed.", "json");
            }

            fixture.EnsureObjects();
            fixture.ability?.EnsureLists();
            return fixture;
        }
    }

    [Serializable]
    public sealed class StructuredAbilityFixtureResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public bool expectation_met;
        public string fixture_id;
        public int event_count;
        public int modifier_count;
        public int player_hand_count;
        public int player_deck_count;
        public int damage_face_up_count;
        public ResourceLedgerValidationResult cost_result;
        public StructuredTargetTemplateResult target_result;
        public List<string> effect_summaries = new List<string>();
        public List<string> expectation_mismatches = new List<string>();
        public GameState after_state;
        public CombatModifierLedger combat_modifier_ledger_after = new CombatModifierLedger();
        public string summary;

        public void EnsureLists()
        {
            if (effect_summaries == null) effect_summaries = new List<string>();
            if (expectation_mismatches == null) expectation_mismatches = new List<string>();
            if (combat_modifier_ledger_after == null) combat_modifier_ledger_after = new CombatModifierLedger();
            combat_modifier_ledger_after.EnsureLists();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredAbilityFixtureResult FromJson(string json)
        {
            StructuredAbilityFixtureResult result =
                JsonUtility.FromJson<StructuredAbilityFixtureResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Structured ability fixture result JSON could not be parsed.",
                    "json");
            }

            result.EnsureLists();
            return result;
        }
    }

    public static class StructuredAbilityFixtureRunner
    {
        public static StructuredAbilityFixtureResult Run(StructuredAbilityFixture fixture)
        {
            if (fixture == null)
            {
                return Reject(
                    StructuredAbilityFixtureRejectionReasons.FixtureMissing,
                    string.Empty,
                    false,
                    null,
                    null,
                    null);
            }

            fixture.EnsureObjects();
            if (fixture.before_state == null)
            {
                return Reject(
                    StructuredAbilityFixtureRejectionReasons.BeforeStateMissing,
                    fixture.fixture_id,
                    false,
                    null,
                    null,
                    null);
            }

            if (fixture.ability == null)
            {
                return Reject(
                    StructuredAbilityFixtureRejectionReasons.AbilityMissing,
                    fixture.fixture_id,
                    false,
                    null,
                    null,
                    null);
            }

            fixture.ability.EnsureLists();
            GameState workingState = GameState.FromJson(fixture.before_state.ToJson(false));
            CombatModifierLedger workingLedger = CloneLedger(fixture.combat_modifier_ledger);
            ResourceLedgerState resourceLedger = fixture.resource_ledger == null
                ? ResourceLedgerState.FromGameState(workingState, fixture.player_index)
                : ResourceLedgerState.FromJson(fixture.resource_ledger.ToJson(false));

            StructuredAbilityTemplateAutomationGateResult templateGate =
                StructuredAbilityTemplateAutomationGate.Validate(fixture.ability);
            if (!templateGate.accepted)
            {
                return FinalizeResult(
                    fixture,
                    workingState,
                    workingLedger,
                    null,
                    null,
                    new List<string>(),
                    false,
                    templateGate.rejection_reason,
                    templateGate.requires_manual_resolution);
            }

            ResourceLedgerValidationResult costResult = StructuredCostTemplate.ValidateAgainstLedger(
                resourceLedger,
                fixture.player_index,
                workingState.turn_number,
                fixture.ability);
            if (!costResult.accepted)
            {
                return FinalizeResult(
                    fixture,
                    workingState,
                    workingLedger,
                    costResult,
                    null,
                    new List<string>(),
                    false,
                    costResult.rejection_reason,
                    false);
            }

            StructuredTargetTemplateResult targetResult = ResolveTarget(fixture, workingState);
            if (targetResult != null && !targetResult.accepted)
            {
                return FinalizeResult(
                    fixture,
                    workingState,
                    workingLedger,
                    costResult,
                    targetResult,
                    new List<string>(),
                    false,
                    StructuredAbilityFixtureRejectionReasons.TargetRejected + ": " + targetResult.rejection_reason,
                    targetResult.requires_manual_resolution);
            }

            StructuredTargetCandidate selectedTarget = SelectTarget(fixture.selected_target, targetResult);
            var effectSummaries = new List<string>();
            for (int i = 0; i < fixture.ability.effects.Count; i++)
            {
                StructuredAbilityEffect effect = fixture.ability.effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.type == "power_plus" || effect.type == "critical_plus")
                {
                    StructuredModifierEffectTemplateResult modifierResult =
                        StructuredModifierEffectTemplate.ApplyToLedger(
                            workingState,
                            fixture.player_index,
                            workingLedger,
                            effect,
                            selectedTarget,
                            fixture.ability.duration,
                            fixture.ability.ability_id);
                    effectSummaries.Add(modifierResult.summary ?? string.Empty);
                    if (!modifierResult.accepted)
                    {
                        return FinalizeResult(
                            fixture,
                            workingState,
                            workingLedger,
                            costResult,
                            targetResult,
                            effectSummaries,
                            false,
                            StructuredAbilityFixtureRejectionReasons.ModifierRejected + ": " +
                            modifierResult.rejection_reason,
                            modifierResult.requires_manual_resolution);
                    }

                    continue;
                }

                StructuredEffectTemplateResult effectResult =
                    StructuredEffectTemplate.Apply(workingState, fixture.player_index, effect);
                effectSummaries.Add(effectResult.summary ?? string.Empty);
                if (!effectResult.accepted)
                {
                    return FinalizeResult(
                        fixture,
                        workingState,
                        workingLedger,
                        costResult,
                        targetResult,
                        effectSummaries,
                        false,
                        StructuredAbilityFixtureRejectionReasons.EffectRejected + ": " +
                        effectResult.rejection_reason,
                        effectResult.requires_manual_resolution);
                }
            }

            return FinalizeResult(
                fixture,
                workingState,
                workingLedger,
                costResult,
                targetResult,
                effectSummaries,
                true,
                string.Empty,
                false);
        }

        private static StructuredTargetTemplateResult ResolveTarget(
            StructuredAbilityFixture fixture,
            GameState workingState)
        {
            if (fixture.ability.targets == null || fixture.ability.targets.Count == 0)
            {
                return null;
            }

            return StructuredTargetTemplate.Resolve(
                workingState,
                fixture.player_index,
                fixture.ability.targets[0]);
        }

        private static StructuredTargetCandidate SelectTarget(
            StructuredTargetCandidate selectedTarget,
            StructuredTargetTemplateResult targetResult)
        {
            if (selectedTarget != null && !string.IsNullOrEmpty(selectedTarget.instance_id))
            {
                return CloneTarget(selectedTarget);
            }

            if (targetResult != null &&
                targetResult.candidates != null &&
                targetResult.candidates.Count > 0)
            {
                return CloneTarget(targetResult.candidates[0]);
            }

            return null;
        }

        private static StructuredAbilityFixtureResult FinalizeResult(
            StructuredAbilityFixture fixture,
            GameState workingState,
            CombatModifierLedger workingLedger,
            ResourceLedgerValidationResult costResult,
            StructuredTargetTemplateResult targetResult,
            List<string> effectSummaries,
            bool accepted,
            string rejectionReason,
            bool requiresManualResolution)
        {
            var result = new StructuredAbilityFixtureResult
            {
                accepted = accepted,
                rejection_reason = rejectionReason ?? string.Empty,
                requires_manual_resolution = requiresManualResolution,
                expectation_met = true,
                fixture_id = fixture.fixture_id ?? string.Empty,
                cost_result = costResult,
                target_result = targetResult,
                effect_summaries = effectSummaries ?? new List<string>(),
                after_state = workingState == null ? null : GameState.FromJson(workingState.ToJson(false)),
                combat_modifier_ledger_after = CloneLedger(workingLedger),
                summary = accepted
                    ? "Structured ability fixture accepted: " + (fixture.fixture_id ?? string.Empty)
                    : "Structured ability fixture rejected: " + (rejectionReason ?? string.Empty)
            };

            PopulateCounts(result, workingState, fixture.player_index);
            ApplyExpectations(fixture.expected, result);
            return result;
        }

        private static StructuredAbilityFixtureResult Reject(
            string rejectionReason,
            string fixtureId,
            bool requiresManualResolution,
            GameState afterState,
            CombatModifierLedger ledger,
            List<string> effectSummaries)
        {
            var result = new StructuredAbilityFixtureResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                requires_manual_resolution = requiresManualResolution,
                expectation_met = false,
                fixture_id = fixtureId ?? string.Empty,
                after_state = afterState,
                combat_modifier_ledger_after = CloneLedger(ledger),
                effect_summaries = effectSummaries ?? new List<string>(),
                summary = "Structured ability fixture rejected: " + (rejectionReason ?? string.Empty)
            };
            result.EnsureLists();
            return result;
        }

        private static void PopulateCounts(
            StructuredAbilityFixtureResult result,
            GameState state,
            int playerIndex)
        {
            result.EnsureLists();
            if (state == null)
            {
                return;
            }

            state.EnsureLists();
            result.event_count = state.event_log == null ? 0 : state.event_log.Count;
            if (playerIndex < 0 || playerIndex >= state.players.Count)
            {
                return;
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            result.player_hand_count = player.hand.Count;
            result.player_deck_count = player.deck.Count;
            result.damage_face_up_count = CountFaceUp(player.damage);
            result.modifier_count = result.combat_modifier_ledger_after.modifiers.Count;
        }

        private static void ApplyExpectations(
            StructuredAbilityFixtureExpectation expected,
            StructuredAbilityFixtureResult result)
        {
            if (expected == null)
            {
                return;
            }

            CheckBool(expected.check_accepted, result.accepted, expected.expected_accepted, "accepted", result);
            CheckInt(expected.check_event_count, result.event_count, expected.expected_event_count, "event_count", result);
            CheckInt(expected.check_modifier_count, result.modifier_count, expected.expected_modifier_count, "modifier_count", result);
            CheckInt(expected.check_player_hand_count, result.player_hand_count, expected.expected_player_hand_count, "player_hand_count", result);
            CheckInt(expected.check_player_deck_count, result.player_deck_count, expected.expected_player_deck_count, "player_deck_count", result);
            CheckInt(expected.check_damage_face_up_count, result.damage_face_up_count, expected.expected_damage_face_up_count, "damage_face_up_count", result);
            result.expectation_met = result.expectation_mismatches.Count == 0;
        }

        private static void CheckBool(
            bool enabled,
            bool actual,
            bool expected,
            string field,
            StructuredAbilityFixtureResult result)
        {
            if (!enabled || actual == expected)
            {
                return;
            }

            result.expectation_mismatches.Add(field + " expected " + expected + " but was " + actual);
        }

        private static void CheckInt(
            bool enabled,
            int actual,
            int expected,
            string field,
            StructuredAbilityFixtureResult result)
        {
            if (!enabled || actual == expected)
            {
                return;
            }

            result.expectation_mismatches.Add(field + " expected " + expected + " but was " + actual);
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

        private static CombatModifierLedger CloneLedger(CombatModifierLedger ledger)
        {
            if (ledger == null)
            {
                return new CombatModifierLedger();
            }

            return CombatModifierLedger.FromJson(ledger.ToJson(false));
        }

        private static StructuredTargetCandidate CloneTarget(StructuredTargetCandidate target)
        {
            if (target == null)
            {
                return null;
            }

            return JsonUtility.FromJson<StructuredTargetCandidate>(JsonUtility.ToJson(target, false));
        }
    }
}
