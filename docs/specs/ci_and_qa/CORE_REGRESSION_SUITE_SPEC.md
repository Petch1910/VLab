# Core Regression Suite Spec

## Milestone

`M18-05`

## Goal

Create a small, explicit regression-suite inventory for the correctness-first
core gates. This suite is a readable contract for CI, humans, and AI agents
before later ability, multiplayer, bot, and release tasks build on the core.

## Scope

`CoreRegressionSuiteReportBuilder.CreateCurrent()` returns a report with these
required categories:

- `rulescore_command_facade`: gameplay commands go through `RulesCore`.
- `legal_action_mask`: UI and bot paths use legal actions instead of direct
  state assumptions.
- `event_sourcing_replay`: accepted state mutations produce `GameEvent` data
  and can be replayed deterministically.
- `snapshot_rollback`: simulated branches and restore paths do not mutate the
  live state.
- `hidden_state_masking`: player, spectator, bot, and network views do not leak
  private hand/deck/source identity.
- `resource_ledger`: CounterBlast, SoulBlast, Energy, and once flags validate
  through ledger helpers.
- `ruleset_profiles`: Standard, V-Premium, Premium, and custom format flags are
  separated by profile instead of hard-coded across the core.

Each category must list representative EditMode tests. The report is an
inventory and closeout gate; it does not replace the individual fixture tests.

## Non-Goals

- No new game behavior.
- No packed-state optimization.
- No ability-template expansion.
- No multiplayer transport changes.

## Verification

- `CoreRegressionSuiteReportTests.CurrentReportValidatesRequiredCategories`
- `CoreRegressionSuiteReportTests.MissingRequiredCategoryRejectsReport`
- `CoreRegressionSuiteReportTests.RequiredCategoryWithoutRepresentativeTestsRejectsReport`
- `CoreRegressionSuiteReportTests.ReportJsonRoundTripKeepsMilestoneAndCategoriesVisible`
- Unity compile passes.
- Unity EditMode tests pass.

## Next Target

After this gate passes, continue with `M18-06 Ability regression suite`.
