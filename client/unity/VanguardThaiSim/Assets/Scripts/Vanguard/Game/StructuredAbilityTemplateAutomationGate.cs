using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredAbilityTemplateAutomationGateRejectionReasons
    {
        public const string AbilityMissing = "STRUCTURED_ABILITY_TEMPLATE_GATE_ABILITY_MISSING";
        public const string CoverageReportMissing = "STRUCTURED_ABILITY_TEMPLATE_GATE_COVERAGE_REPORT_MISSING";
        public const string CoverageReportInvalid = "STRUCTURED_ABILITY_TEMPLATE_GATE_COVERAGE_REPORT_INVALID";
        public const string CoverageMissing = "STRUCTURED_ABILITY_TEMPLATE_GATE_COVERAGE_MISSING";
        public const string ManualFallbackRequired = "STRUCTURED_ABILITY_TEMPLATE_GATE_MANUAL_FALLBACK_REQUIRED";
        public const string ManualFallbackDisabled = "STRUCTURED_ABILITY_TEMPLATE_GATE_MANUAL_FALLBACK_DISABLED";
    }

    [Serializable]
    public sealed class StructuredAbilityTemplateCoverageEntry
    {
        public string category;
        public string template_id;
        public bool automated;
        public bool covered_by_tests;
        public string fallback_policy;
        public List<string> test_names = new List<string>();

        public void EnsureLists()
        {
            if (test_names == null)
            {
                test_names = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class StructuredAbilityTemplateCoverageReport
    {
        public string schema_version;
        public string policy_summary;
        public List<StructuredAbilityTemplateCoverageEntry> templates =
            new List<StructuredAbilityTemplateCoverageEntry>();

        public void EnsureLists()
        {
            if (templates == null)
            {
                templates = new List<StructuredAbilityTemplateCoverageEntry>();
            }

            for (int i = 0; i < templates.Count; i++)
            {
                templates[i]?.EnsureLists();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredAbilityTemplateCoverageReport FromJson(string json)
        {
            StructuredAbilityTemplateCoverageReport report =
                JsonUtility.FromJson<StructuredAbilityTemplateCoverageReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Structured ability template coverage report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    [Serializable]
    public sealed class StructuredAbilityTemplateCoverageValidationResult
    {
        public bool accepted;
        public string rejection_reason;
        public List<string> issues = new List<string>();
        public string summary;

        public void EnsureLists()
        {
            if (issues == null)
            {
                issues = new List<string>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredAbilityTemplateCoverageValidationResult FromJson(string json)
        {
            StructuredAbilityTemplateCoverageValidationResult result =
                JsonUtility.FromJson<StructuredAbilityTemplateCoverageValidationResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Structured ability template coverage validation result JSON could not be parsed.",
                    "json");
            }

            result.EnsureLists();
            return result;
        }
    }

    [Serializable]
    public sealed class StructuredAbilityTemplateAutomationGateResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public List<string> unsupported_templates = new List<string>();
        public string summary;

        public void EnsureLists()
        {
            if (unsupported_templates == null)
            {
                unsupported_templates = new List<string>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredAbilityTemplateAutomationGateResult FromJson(string json)
        {
            StructuredAbilityTemplateAutomationGateResult result =
                JsonUtility.FromJson<StructuredAbilityTemplateAutomationGateResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Structured ability template automation gate result JSON could not be parsed.",
                    "json");
            }

            result.EnsureLists();
            return result;
        }
    }

    public static class StructuredAbilityTemplateAutomationGate
    {
        public const string CoverageSchemaVersion = "structured_ability_template_coverage_v1";

        public static StructuredAbilityTemplateCoverageReport CreateCurrentCoverageReport()
        {
            var report = new StructuredAbilityTemplateCoverageReport
            {
                schema_version = CoverageSchemaVersion,
                policy_summary =
                    "Only templates with named regression coverage may run automatically; unsupported templates use manual fallback."
            };

            Add(report, "cost", "none", true, "no payment template", "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys");
            Add(report, "cost", "counter_blast", true, "resource ledger cost", "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys", "StructuredCostTemplateTests.ValidateAgainstLedgerUsesResourceLedgerWithoutMutatingLedger");
            Add(report, "cost", "soul_blast", true, "resource ledger cost", "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys", "StructuredCostTemplateTests.ValidateAgainstLedgerUsesResourceLedgerWithoutMutatingLedger");
            Add(report, "cost", "energy_blast", true, "resource ledger cost", "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys");
            Add(report, "cost", "once_per_turn", true, "resource ledger once flag", "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys", "StructuredCostTemplateTests.MultipleOnceFlagsReject");
            Add(report, "cost", "once_per_fight", true, "resource ledger once flag", "StructuredCostTemplateTests.BuildRequestAggregatesLedgerBackedCostsAndOnceKeys", "StructuredCostTemplateTests.MultipleOnceFlagsReject");
            Add(report, "cost", "discard", false, "manual fallback until target/payment choice is implemented", "StructuredCostTemplateTests.DiscardCostRequiresManualResolutionPlaceholder");

            Add(report, "target", "none", true, "no target required", "StructuredAbilityFixtureDslTests.DrawFixtureRunsBeforeActionAfterScenarioWithoutMutatingSource");
            Add(report, "target", "self", true, "visible self target", "StructuredTargetTemplateTests.ResolvesSelfRearGuardVisibleUnit");
            Add(report, "target", "unit", true, "visible unit target", "StructuredTargetTemplateTests.ResolvesSelfRearGuardVisibleUnit", "StructuredTargetTemplateTests.ResolvesAnyOwnerPublicZoneAndSkipsFaceDownCards");
            Add(report, "target", "card", true, "visible card target in supported public zones", "StructuredTargetTemplateTests.HiddenOrUnsupportedZonesRequireManualResolution");
            Add(report, "target", "circle", false, "manual fallback until circle targeting is represented", "StructuredTargetTemplateTests.CircleTargetRequiresManualResolution");
            Add(report, "target", "filters", false, "manual fallback until filter predicates are executable", "StructuredTargetTemplateTests.ResolvesSelfRearGuardVisibleUnit");
            Add(report, "target", "hidden_zone", false, "manual fallback for hidden/private target zones", "StructuredTargetTemplateTests.HiddenOrUnsupportedZonesRequireManualResolution");

            Add(report, "effect", "draw", true, "RulesCore Draw command", "StructuredEffectTemplateTests.ApplyDrawMutatesThroughRulesCoreEventPath", "StructuredEffectTemplateTests.PreviewDrawDoesNotMutateLiveState");
            Add(report, "effect", "move_zone", true, "RulesCore MoveCard command", "StructuredEffectTemplateTests.ApplyMoveZoneUsesRulesCoreMoveCommand", "StructuredEffectTemplateTests.InvalidMoveRejectsWithoutMutation");
            Add(report, "effect", "counter_blast", true, "RulesCore ResourceFlip command", "StructuredEffectTemplateTests.ApplyCounterBlastFlipsFaceDownThroughRulesCoreEventPath", "StructuredEffectTemplateTests.CounterBlastRejectsWithoutMutationWhenNoFaceUpDamageExists");
            Add(report, "effect", "counter_charge", true, "RulesCore ResourceFlip command", "StructuredEffectTemplateTests.PreviewCounterChargeDoesNotMutateLiveState");
            Add(report, "effect", "soul_blast", true, "RulesCore MoveCard Soul to Drop", "StructuredEffectTemplateTests.SoulBlastMovesSoulToDropThroughRulesCoreEventPath", "StructuredEffectTemplateTests.SoulBlastRejectsWithoutMutationWhenSoulIsEmpty");
            Add(report, "effect", "soul_charge", true, "RulesCore MoveCard Deck to Soul", "StructuredEffectTemplateTests.SoulChargeMovesTopDeckToSoulThroughRulesCoreEventPath", "StructuredEffectTemplateTests.PreviewSoulChargeDoesNotMutateLiveState");
            Add(report, "effect", "power_plus", true, "combat modifier ledger", "StructuredModifierEffectTemplateTests.PreviewPowerPlusCreatesLedgerAndDoesNotMutateStateOrSourceLedger");
            Add(report, "effect", "critical_plus", true, "combat modifier ledger", "StructuredModifierEffectTemplateTests.ApplyCriticalPlusMutatesOnlyLedgerAndUsesEndOfBattleDuration");
            Add(report, "effect", "manual", false, "manual fallback by definition", "StructuredEffectTemplateTests.UnsupportedEffectRequiresManualResolution");

            Add(report, "duration", "instant", true, "non-modifier effect duration marker", "StructuredAbilityFixtureDslTests.DrawFixtureRunsBeforeActionAfterScenarioWithoutMutatingSource");
            Add(report, "duration", "until_end_of_turn", true, "modifier cleanup at end of turn", "StructuredModifierEffectTemplateTests.PreviewPowerPlusCreatesLedgerAndDoesNotMutateStateOrSourceLedger");
            Add(report, "duration", "until_end_of_battle", true, "modifier cleanup at end of battle", "StructuredModifierEffectTemplateTests.ApplyCriticalPlusMutatesOnlyLedgerAndUsesEndOfBattleDuration");
            Add(report, "duration", "continuous", false, "manual fallback until continuous condition tests exist", "StructuredModifierEffectTemplateTests.UnsupportedDurationRequiresManualResolutionWithoutLedgerMutation");
            Add(report, "duration", "manual", false, "manual fallback until manual ledger lifecycle tests exist", "StructuredModifierEffectTemplateTests.UnsupportedDurationRequiresManualResolutionWithoutLedgerMutation");

            return report;
        }

        public static StructuredAbilityTemplateCoverageValidationResult ValidateCoverageReport(
            StructuredAbilityTemplateCoverageReport report)
        {
            if (report == null)
            {
                return RejectCoverage(
                    StructuredAbilityTemplateAutomationGateRejectionReasons.CoverageReportMissing,
                    null);
            }

            report.EnsureLists();
            var issues = new List<string>();
            if (!string.Equals(report.schema_version, CoverageSchemaVersion, StringComparison.Ordinal))
            {
                issues.Add("schema_version expected " + CoverageSchemaVersion + " but was " + (report.schema_version ?? string.Empty));
            }

            var keys = new HashSet<string>();
            for (int i = 0; i < report.templates.Count; i++)
            {
                StructuredAbilityTemplateCoverageEntry entry = report.templates[i];
                if (entry == null)
                {
                    issues.Add("template entry " + i + " is null");
                    continue;
                }

                entry.EnsureLists();
                string key = BuildCoverageKey(entry.category, entry.template_id);
                if (!keys.Add(key))
                {
                    issues.Add("duplicate template coverage key " + key);
                }

                if (entry.automated && (!entry.covered_by_tests || entry.test_names.Count == 0))
                {
                    issues.Add("automated template " + key + " has no regression test coverage");
                }
            }

            if (issues.Count > 0)
            {
                return RejectCoverage(
                    StructuredAbilityTemplateAutomationGateRejectionReasons.CoverageReportInvalid,
                    issues);
            }

            return new StructuredAbilityTemplateCoverageValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                issues = new List<string>(),
                summary = "Structured ability template coverage report accepted with " +
                          report.templates.Count + " template entry(s)."
            };
        }

        public static StructuredAbilityTemplateAutomationGateResult Validate(StructuredAbility ability)
        {
            return Validate(ability, CreateCurrentCoverageReport());
        }

        public static StructuredAbilityTemplateAutomationGateResult Validate(
            StructuredAbility ability,
            StructuredAbilityTemplateCoverageReport coverageReport)
        {
            StructuredAbilityTemplateCoverageValidationResult coverage =
                ValidateCoverageReport(coverageReport);
            if (!coverage.accepted)
            {
                return Reject(
                    StructuredAbilityTemplateAutomationGateRejectionReasons.CoverageReportInvalid + ": " +
                    coverage.rejection_reason,
                    false,
                    coverage.issues);
            }

            if (ability == null)
            {
                return Reject(
                    StructuredAbilityTemplateAutomationGateRejectionReasons.AbilityMissing,
                    false,
                    null);
            }

            StructuredAbility workingAbility = CloneAbility(ability);
            workingAbility.EnsureLists();
            var unsupported = new List<string>();

            CheckCosts(workingAbility, coverageReport, unsupported);
            CheckTargets(workingAbility, coverageReport, unsupported);
            CheckEffects(workingAbility, coverageReport, unsupported);

            if (unsupported.Count == 0)
            {
                return new StructuredAbilityTemplateAutomationGateResult
                {
                    accepted = true,
                    rejection_reason = string.Empty,
                    requires_manual_resolution = false,
                    unsupported_templates = new List<string>(),
                    summary = "Structured ability templates are covered for " +
                              (workingAbility.ability_id ?? string.Empty) + "."
                };
            }

            return Reject(
                workingAbility.manual_fallback
                    ? StructuredAbilityTemplateAutomationGateRejectionReasons.ManualFallbackRequired
                    : StructuredAbilityTemplateAutomationGateRejectionReasons.ManualFallbackDisabled,
                workingAbility.manual_fallback,
                unsupported);
        }

        private static void CheckCosts(
            StructuredAbility ability,
            StructuredAbilityTemplateCoverageReport coverageReport,
            List<string> unsupported)
        {
            for (int i = 0; i < ability.costs.Count; i++)
            {
                StructuredAbilityCost cost = ability.costs[i];
                if (cost == null)
                {
                    continue;
                }

                string costType = NormalizeTemplateId(cost.type, "none");
                if (!IsAutomatedAndCovered(coverageReport, "cost", costType))
                {
                    AddUnsupported(unsupported, "cost:" + costType);
                }
            }
        }

        private static void CheckTargets(
            StructuredAbility ability,
            StructuredAbilityTemplateCoverageReport coverageReport,
            List<string> unsupported)
        {
            for (int i = 0; i < ability.targets.Count; i++)
            {
                StructuredAbilityTarget target = ability.targets[i];
                if (target == null)
                {
                    continue;
                }

                target.EnsureLists();
                string targetType = NormalizeTemplateId(target.type, "none");
                if (!IsAutomatedAndCovered(coverageReport, "target", targetType))
                {
                    AddUnsupported(unsupported, "target:" + targetType);
                }

                if (target.filters.Count > 0)
                {
                    AddUnsupported(unsupported, "target:filters");
                }

                if (targetType == "none")
                {
                    continue;
                }

                if (!IsSupportedOwner(target.owner))
                {
                    AddUnsupported(unsupported, "target_owner:" + (target.owner ?? string.Empty));
                }

                if (!Enum.TryParse(target.zone ?? string.Empty, false, out GameZone zone))
                {
                    AddUnsupported(unsupported, "target_zone:" + (target.zone ?? string.Empty));
                    continue;
                }

                if (IsHiddenOrUnsupportedTargetZone(target.owner, zone))
                {
                    AddUnsupported(unsupported, "target:hidden_zone:" + zone);
                }
            }
        }

        private static void CheckEffects(
            StructuredAbility ability,
            StructuredAbilityTemplateCoverageReport coverageReport,
            List<string> unsupported)
        {
            bool hasModifierEffect = false;
            for (int i = 0; i < ability.effects.Count; i++)
            {
                StructuredAbilityEffect effect = ability.effects[i];
                if (effect == null)
                {
                    continue;
                }

                string effectType = NormalizeTemplateId(effect.type, string.Empty);
                if (!IsAutomatedAndCovered(coverageReport, "effect", effectType))
                {
                    AddUnsupported(unsupported, "effect:" + effectType);
                }

                if (effectType == "move_zone" &&
                    (!Enum.TryParse(effect.from_zone ?? string.Empty, false, out GameZone _) ||
                     !Enum.TryParse(effect.to_zone ?? string.Empty, false, out GameZone _)))
                {
                    AddUnsupported(
                        unsupported,
                        "effect:move_zone:" + (effect.from_zone ?? string.Empty) + "->" + (effect.to_zone ?? string.Empty));
                }

                if (effectType == "power_plus" || effectType == "critical_plus")
                {
                    hasModifierEffect = true;
                }
            }

            if (hasModifierEffect)
            {
                string durationTemplate = ResolveModifierDurationTemplate(ability.duration);
                if (!IsAutomatedAndCovered(coverageReport, "duration", durationTemplate))
                {
                    AddUnsupported(unsupported, "duration:" + durationTemplate);
                }
            }
        }

        private static string ResolveModifierDurationTemplate(StructuredAbilityDuration duration)
        {
            if (duration == null)
            {
                return string.Empty;
            }

            string cleanup = NormalizeTemplateId(duration.cleanup_timing, string.Empty);
            string type = NormalizeTemplateId(duration.type, string.Empty);
            if (cleanup == "end_of_turn" || type == "until_end_of_turn")
            {
                return "until_end_of_turn";
            }

            if (cleanup == "end_of_battle" || type == "until_end_of_battle")
            {
                return "until_end_of_battle";
            }

            return type;
        }

        private static bool IsAutomatedAndCovered(
            StructuredAbilityTemplateCoverageReport report,
            string category,
            string templateId)
        {
            StructuredAbilityTemplateCoverageEntry entry = FindEntry(report, category, templateId);
            return entry != null && entry.automated && entry.covered_by_tests && entry.test_names.Count > 0;
        }

        private static StructuredAbilityTemplateCoverageEntry FindEntry(
            StructuredAbilityTemplateCoverageReport report,
            string category,
            string templateId)
        {
            if (report == null || report.templates == null)
            {
                return null;
            }

            string key = BuildCoverageKey(category, templateId);
            for (int i = 0; i < report.templates.Count; i++)
            {
                StructuredAbilityTemplateCoverageEntry entry = report.templates[i];
                if (entry == null)
                {
                    continue;
                }

                if (string.Equals(BuildCoverageKey(entry.category, entry.template_id), key, StringComparison.Ordinal))
                {
                    entry.EnsureLists();
                    return entry;
                }
            }

            return null;
        }

        private static bool IsSupportedOwner(string owner)
        {
            string safeOwner = owner ?? string.Empty;
            return safeOwner == string.Empty ||
                   safeOwner == "self" ||
                   safeOwner == "opponent" ||
                   safeOwner == "any";
        }

        private static bool IsHiddenOrUnsupportedTargetZone(string owner, GameZone zone)
        {
            if (zone == GameZone.Deck || zone == GameZone.Soul || zone == GameZone.GZone)
            {
                return true;
            }

            string safeOwner = owner ?? string.Empty;
            if (safeOwner != string.Empty &&
                safeOwner != "self" &&
                (zone == GameZone.Hand || zone == GameZone.RideDeck))
            {
                return true;
            }

            return false;
        }

        private static StructuredAbilityTemplateCoverageValidationResult RejectCoverage(
            string rejectionReason,
            List<string> issues)
        {
            return new StructuredAbilityTemplateCoverageValidationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                issues = CloneStrings(issues),
                summary = "Structured ability template coverage report rejected: " +
                          (rejectionReason ?? string.Empty)
            };
        }

        private static StructuredAbilityTemplateAutomationGateResult Reject(
            string rejectionReason,
            bool requiresManualResolution,
            List<string> unsupported)
        {
            return new StructuredAbilityTemplateAutomationGateResult
            {
                accepted = false,
                rejection_reason = BuildRejectionReason(rejectionReason, unsupported),
                requires_manual_resolution = requiresManualResolution,
                unsupported_templates = CloneStrings(unsupported),
                summary = "Structured ability template gate rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static string BuildRejectionReason(string rejectionReason, List<string> unsupported)
        {
            string safeReason = rejectionReason ?? string.Empty;
            if (unsupported == null || unsupported.Count == 0)
            {
                return safeReason;
            }

            return safeReason + ": " + string.Join(", ", unsupported.ToArray());
        }

        private static void Add(
            StructuredAbilityTemplateCoverageReport report,
            string category,
            string templateId,
            bool automated,
            string fallbackPolicy,
            params string[] testNames)
        {
            var entry = new StructuredAbilityTemplateCoverageEntry
            {
                category = category ?? string.Empty,
                template_id = templateId ?? string.Empty,
                automated = automated,
                covered_by_tests = testNames != null && testNames.Length > 0,
                fallback_policy = fallbackPolicy ?? string.Empty,
                test_names = new List<string>()
            };

            if (testNames != null)
            {
                for (int i = 0; i < testNames.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(testNames[i]))
                    {
                        entry.test_names.Add(testNames[i]);
                    }
                }
            }

            report.templates.Add(entry);
        }

        private static void AddUnsupported(List<string> unsupported, string value)
        {
            if (unsupported == null)
            {
                return;
            }

            string safeValue = value ?? string.Empty;
            for (int i = 0; i < unsupported.Count; i++)
            {
                if (string.Equals(unsupported[i], safeValue, StringComparison.Ordinal))
                {
                    return;
                }
            }

            unsupported.Add(safeValue);
        }

        private static string NormalizeTemplateId(string value, string fallback)
        {
            string safeValue = value ?? string.Empty;
            safeValue = safeValue.Trim();
            return string.IsNullOrEmpty(safeValue) ? fallback ?? string.Empty : safeValue;
        }

        private static string BuildCoverageKey(string category, string templateId)
        {
            return (category ?? string.Empty) + ":" + (templateId ?? string.Empty);
        }

        private static List<string> CloneStrings(List<string> source)
        {
            var result = new List<string>();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                result.Add(source[i] ?? string.Empty);
            }

            return result;
        }

        private static StructuredAbility CloneAbility(StructuredAbility ability)
        {
            if (ability == null)
            {
                return null;
            }

            StructuredAbility clone =
                JsonUtility.FromJson<StructuredAbility>(JsonUtility.ToJson(ability, false));
            clone?.EnsureLists();
            return clone;
        }
    }
}
