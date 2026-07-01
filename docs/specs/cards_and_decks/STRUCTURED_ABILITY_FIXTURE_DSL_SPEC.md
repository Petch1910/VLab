# Structured Ability Fixture DSL Spec

## Status

Implemented in `M12-09`.

## Purpose

Provide a small before/action/after fixture surface for structured ability
tests. The fixture runner lets future contributors encode ruling and template
regressions without wiring UI, network, or bot code.

## Runtime Surface

`StructuredAbilityFixtureRunner.Run(fixture)`:

- clones `fixture.before_state`
- validates structured costs through `StructuredCostTemplate` and
  `ResourceLedger`
- resolves the first structured target through `StructuredTargetTemplate`
- applies draw/move/resource effects through `StructuredEffectTemplate`
- applies PowerPlus/CriticalPlus effects through
  `StructuredModifierEffectTemplate`
- compares optional expectation fields
- returns a `StructuredAbilityFixtureResult`

## Fixture Shape

`StructuredAbilityFixture` includes:

- `fixture_id`
- `description`
- `player_index`
- `before_state`
- `ability`
- optional `resource_ledger`
- optional `combat_modifier_ledger`
- optional `selected_target`
- `expected`

If no `resource_ledger` is supplied, the runner derives one from the cloned
`GameState` with `ResourceLedgerState.FromGameState`.

## Expectations

Each expectation is opt-in through a `check_*` flag:

- accepted result
- event count
- combat modifier count
- player hand count
- player deck count
- damage face-up count

Mismatches do not rewrite the accepted/rejected result. They only set
`expectation_met = false` and add human-readable entries to
`expectation_mismatches`.

## Boundary

M12-09 does not:

- execute full card text
- choose targets interactively
- support multiple target groups
- commit cost payment into live state
- publish network payloads
- mutate the source fixture state

## Verification

EditMode coverage verifies:

- draw fixture before/action/after counts and source no-mutation
- CounterBlast fixture damage face-up expectation
- PowerPlus fixture writes only to the combat modifier ledger
- expectation mismatch reporting
- manual fallback rejection for an explicit unsupported `manual` effect
- fixture/result JSON round-trip

## Next Work

`M12-10` should create a first structured card pack using the supported
template subset and fixture tests.
