using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class AbilityRegressionSuiteReport
    {
        public int schema_version = 1;
        public string milestone = "M18-06";
        public string suite_status = "inventory_ready";
        public int category_count;
        public int representative_test_count;
        public List<AbilityRegressionSuiteCategory> categories = new List<AbilityRegressionSuiteCategory>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static AbilityRegressionSuiteReport FromJson(string json)
        {
            AbilityRegressionSuiteReport report = JsonUtility.FromJson<AbilityRegressionSuiteReport>(json);
            report?.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (categories == null)
            {
                categories = new List<AbilityRegressionSuiteCategory>();
            }

            foreach (AbilityRegressionSuiteCategory category in categories)
            {
                category?.EnsureLists();
            }
        }
    }

    [Serializable]
    public sealed class AbilityRegressionSuiteCategory
    {
        public string id;
        public string description;
        public List<string> representative_tests = new List<string>();

        public void EnsureLists()
        {
            if (representative_tests == null)
            {
                representative_tests = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class AbilityRegressionSuiteValidationResult
    {
        public bool accepted;
        public List<string> errors = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class AbilityRegressionSuiteReportBuilder
    {
        private static readonly string[] RequiredCategories =
        {
            "schema_contract_python",
            "schema_validator_python",
            "runtime_registry",
            "cost_target_templates",
            "effect_resource_templates",
            "modifier_templates",
            "fixture_dsl_pack_smoke",
            "manual_fallback_bridge",
            "custom_pack_ability_metadata"
        };

        public static AbilityRegressionSuiteReport CreateCurrent()
        {
            AbilityRegressionSuiteReport report = new AbilityRegressionSuiteReport();
            report.categories.Add(Category(
                "schema_contract_python",
                "Python schema tests for structured ability top-level shape, required sections, enums, and sample data.",
                "tests.test_ability_schema_v1.AbilitySchemaV1Tests.test_schema_has_required_top_level_shape",
                "tests.test_ability_schema_v1.AbilitySchemaV1Tests.test_ability_definition_requires_core_sections",
                "tests.test_ability_schema_v1.AbilitySchemaV1Tests.test_schema_enums_include_m12_template_targets",
                "tests.test_ability_schema_v1.AbilitySchemaV1Tests.test_sample_uses_schema_version_and_required_sections"));
            report.categories.Add(Category(
                "schema_validator_python",
                "Python validator tests for duplicate ids, missing sections, invalid enums, and once-cost keys.",
                "tests.test_ability_schema_validator.AbilitySchemaValidatorTests.test_sample_ability_file_validates",
                "tests.test_ability_schema_validator.AbilitySchemaValidatorTests.test_duplicate_ability_id_is_rejected",
                "tests.test_ability_schema_validator.AbilitySchemaValidatorTests.test_invalid_enums_are_rejected",
                "tests.test_ability_schema_validator.AbilitySchemaValidatorTests.test_once_cost_requires_key"));
            report.categories.Add(Category(
                "runtime_registry",
                "Unity runtime registry loading, indexing, clone safety, duplicate rejection, and JSON reporting.",
                "RuntimeAbilityRegistryTests.LoadValidPackIndexesByCardAndAbilityId",
                "RuntimeAbilityRegistryTests.ReturnedAbilitiesAreClones",
                "RuntimeAbilityRegistryTests.DuplicateAbilityIdIsRejected",
                "RuntimeAbilityRegistryTests.LoadResultRoundTripsJsonWithoutRegistryPayload"));
            report.categories.Add(Category(
                "cost_target_templates",
                "Structured cost and target template coverage for ledger-backed costs, once flags, visibility, and hidden zones.",
                "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys",
                "StructuredCostTemplateTests.ValidateAgainstLedgerUsesResourceLedgerWithoutMutatingLedger",
                "StructuredCostTemplateTests.DiscardCostRequiresManualResolutionPlaceholder",
                "StructuredTargetTemplateTests.ResolvesSelfRearGuardVisibleUnit",
                "StructuredTargetTemplateTests.HiddenOrUnsupportedZonesRequireManualResolution",
                "StructuredTargetTemplateTests.ResolveDoesNotMutateStateAndResultRoundTripsJson"));
            report.categories.Add(Category(
                "effect_resource_templates",
                "Structured draw, move-zone, CounterBlast, CounterCharge, SoulCharge, and SoulBlast effect coverage.",
                "StructuredEffectTemplateTests.PreviewDrawDoesNotMutateLiveState",
                "StructuredEffectTemplateTests.ApplyDrawMutatesThroughRulesCoreEventPath",
                "StructuredEffectTemplateTests.ApplyMoveZoneUsesRulesCoreMoveCommand",
                "StructuredEffectTemplateTests.ApplyCounterBlastFlipsFaceDownThroughRulesCoreEventPath",
                "StructuredEffectTemplateTests.PreviewCounterChargeDoesNotMutateLiveState",
                "StructuredEffectTemplateTests.SoulChargeMovesTopDeckToSoulThroughRulesCoreEventPath",
                "StructuredEffectTemplateTests.PreviewSoulChargeDoesNotMutateLiveState",
                "StructuredEffectTemplateTests.SoulBlastMovesSoulToDropThroughRulesCoreEventPath",
                "StructuredEffectTemplateTests.SoulBlastRejectsWithoutMutationWhenSoulIsEmpty",
                "StructuredEffectTemplateTests.InvalidMoveRejectsWithoutMutation"));
            report.categories.Add(Category(
                "modifier_templates",
                "Power/Critical modifier templates, duration gates, hidden target rejection, and ledger-only mutation.",
                "StructuredModifierEffectTemplateTests.PreviewPowerPlusCreatesLedgerAndDoesNotMutateStateOrSourceLedger",
                "StructuredModifierEffectTemplateTests.ApplyCriticalPlusMutatesOnlyLedgerAndUsesEndOfBattleDuration",
                "StructuredModifierEffectTemplateTests.UnsupportedDurationRequiresManualResolutionWithoutLedgerMutation",
                "StructuredModifierEffectTemplateTests.HiddenOrMissingTargetRejectsWithoutLedgerMutation"));
            report.categories.Add(Category(
                "fixture_dsl_pack_smoke",
                "Ability fixture DSL and first structured pack smoke tests through runtime registry and fixture runner.",
                "StructuredAbilityFixtureDslTests.DrawFixtureRunsBeforeActionAfterScenarioWithoutMutatingSource",
                "StructuredAbilityFixtureDslTests.ResourceFixtureCanAssertCounterBlastDamageFaceUpCount",
                "StructuredAbilityFixtureDslTests.ModifierFixtureWritesCombatLedgerOnly",
                "StructuredAbilityFixtureDslTests.ManualFallbackEffectRejectsWithManualResolution",
                "FirstStructuredCardPackTests.PackLoadsThroughRuntimeAbilityRegistry",
                "FirstStructuredCardPackTests.PackDrawAbilityRunsThroughFixtureDsl",
                "FirstStructuredCardPackTests.PackPowerPlusAbilityRunsThroughFixtureDsl",
                "tests.test_first_structured_card_pack.FirstStructuredCardPackTests.test_pack_validates_with_ability_schema_validator"));
            report.categories.Add(Category(
                "manual_fallback_bridge",
                "Manual fallback bridge from unsupported structured fixture result into hidden-safe manual resolved decision data.",
                "StructuredAbilityManualFallbackBridgeTests.UnsupportedFixtureResultCreatesResolveDecision",
                "StructuredAbilityManualFallbackBridgeTests.HiddenFallbackDoesNotLeakSourceIdentity",
                "StructuredAbilityManualFallbackBridgeTests.BridgeCreationDoesNotMutateAbilityOrGameState",
                "StructuredAbilityManualFallbackBridgeTests.BridgeResultRoundTripsJson"));
            report.categories.Add(Category(
                "custom_pack_ability_metadata",
                "Custom pack v2 ability metadata and runtime pack status coverage.",
                "CardRepositoryTests.PackValidationStatusReportsV2CapabilitiesAndAbilityMetadata",
                "tests.test_custom_pack_schema.CustomPackSchemaTests.test_v2_template_validates",
                "tests.test_custom_pack_schema.CustomPackSchemaTests.test_v2_rejects_ability_for_unknown_card",
                "tests.test_custom_pack_schema.CustomPackSchemaTests.test_import_custom_pack_v2_preserves_source_metadata"));

            RefreshCounts(report);
            return report;
        }

        public static AbilityRegressionSuiteValidationResult Validate(AbilityRegressionSuiteReport report)
        {
            AbilityRegressionSuiteValidationResult result = new AbilityRegressionSuiteValidationResult();
            if (report == null)
            {
                result.errors.Add("report_missing");
                return result;
            }

            report.EnsureLists();
            if (report.schema_version != 1)
            {
                result.errors.Add("schema_version_must_be_1");
            }

            if (report.milestone != "M18-06")
            {
                result.errors.Add("milestone_must_be_M18-06");
            }

            foreach (string required in RequiredCategories)
            {
                AbilityRegressionSuiteCategory category = FindCategory(report, required);
                if (category == null)
                {
                    result.errors.Add("missing_category_" + required);
                    continue;
                }

                if (category.representative_tests.Count == 0)
                {
                    result.errors.Add("category_has_no_tests_" + required);
                }
            }

            result.accepted = result.errors.Count == 0;
            return result;
        }

        private static AbilityRegressionSuiteCategory Category(
            string id,
            string description,
            params string[] representativeTests)
        {
            return new AbilityRegressionSuiteCategory
            {
                id = id,
                description = description,
                representative_tests = new List<string>(representativeTests)
            };
        }

        private static AbilityRegressionSuiteCategory FindCategory(AbilityRegressionSuiteReport report, string id)
        {
            for (int i = 0; i < report.categories.Count; i++)
            {
                AbilityRegressionSuiteCategory category = report.categories[i];
                if (category != null && category.id == id)
                {
                    return category;
                }
            }

            return null;
        }

        private static void RefreshCounts(AbilityRegressionSuiteReport report)
        {
            report.EnsureLists();
            report.category_count = report.categories.Count;
            report.representative_test_count = 0;
            foreach (AbilityRegressionSuiteCategory category in report.categories)
            {
                if (category != null)
                {
                    report.representative_test_count += category.representative_tests.Count;
                }
            }
        }
    }
}
