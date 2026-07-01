# Ability Regression Suite Spec

## Milestone

`M18-06`

## Goal

Create an explicit regression-suite inventory for structured ability data and
runtime execution gates. This keeps M12/M15 ability work visible before later
multiplayer payload tests and release builds.

## Scope

`AbilityRegressionSuiteReportBuilder.CreateCurrent()` returns a report with
these required categories:

- `schema_contract_python`: JSON schema top-level shape, required ability
  sections, supported enum/template values, and sample schema version checks.
- `schema_validator_python`: Python validator acceptance/rejection for ability
  packs, duplicate ids, invalid enums, missing sections, and once-cost keys.
- `runtime_registry`: Unity runtime ability loading, indexing, clone safety,
  duplicate rejection, and load-result JSON reporting.
- `cost_target_templates`: cost request building, resource ledger validation,
  discard/manual placeholders, visible target resolution, and hidden-zone
  manual fallback.
- `effect_resource_templates`: draw, move-zone, CounterBlast, CounterCharge,
  soul placeholder, invalid move, no-mutation preview, and RulesCore-backed
  mutation paths.
- `modifier_templates`: PowerPlus/CriticalPlus ledger behavior, supported
  duration gates, unsupported duration fallback, hidden target rejection, and
  ledger-only mutation.
- `fixture_dsl_pack_smoke`: fixture DSL before/action/after flow, resource
  assertions, modifier ledger assertions, manual fallback result, and first
  structured pack smoke tests.
- `manual_fallback_bridge`: unsupported structured ability result to
  hidden-safe manual resolution bridge.
- `custom_pack_ability_metadata`: custom pack v2 ability metadata and runtime
  pack status validation.

This report is a QA inventory and closeout gate; it does not replace the
individual Python and Unity tests.

## Non-Goals

- No new ability behavior.
- No expansion of supported card text templates.
- No automatic full-card parser.
- No multiplayer payload changes. Those are covered by `M18-07`.

## Verification

Python:

```powershell
python -m unittest discover -s tests -p "test_*.py"
python tools\data\validate_ability_schema.py data\packs\vanguard_th\abilities\structured_ability_pack_m12_10.json
```

Unity:

- `AbilityRegressionSuiteReportTests.CurrentReportValidatesRequiredCategories`
- `AbilityRegressionSuiteReportTests.MissingRequiredCategoryRejectsReport`
- `AbilityRegressionSuiteReportTests.RequiredCategoryWithoutRepresentativeTestsRejectsReport`
- `AbilityRegressionSuiteReportTests.ReportJsonRoundTripKeepsMilestoneAndCategoriesVisible`
- Unity compile passes.
- Unity EditMode tests pass.

## Next Target

After this gate passes, continue with `M18-07 Multiplayer payload/no-leak tests`.
