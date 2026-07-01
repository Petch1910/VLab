# Structured Modifier Effect Template Spec

## Status

Implemented in `M12-08`.

## Purpose

Convert structured `power_plus` and `critical_plus` effects into
`CombatModifierLedger` entries without mutating `GameState` directly.

This keeps stat modifiers separate from event-sourced zone/resource commands
until a later milestone decides whether combat modifiers become their own
event stream or stay as a dedicated battle ledger.

## Runtime Surface

`StructuredModifierEffectTemplate.Preview(...)`:

- validates state, effect, target, and duration
- clones the source combat modifier ledger
- adds the modifier to the clone
- returns the cloned ledger and combat stat projection
- does not mutate source `GameState` or source ledger

`StructuredModifierEffectTemplate.ApplyToLedger(...)`:

- validates the same inputs
- appends the generated modifier to the provided `CombatModifierLedger`
- returns the ledger after mutation and combat stat projection
- does not mutate `GameState` or `GameState.event_log`

## Supported v1 Effects

- `power_plus`
- `critical_plus`

`amount` must be a positive integer. Missing/default `0` amounts reject instead
of guessing a default value.

## Duration Mapping

Supported duration/cleanup mapping:

- `cleanup_timing = end_of_battle` -> `CombatModifierExpiration.EndOfBattle`
- `cleanup_timing = end_of_turn` -> `CombatModifierExpiration.EndOfTurn`
- `cleanup_timing = manual` -> `CombatModifierExpiration.Manual`
- `type = until_end_of_battle` -> `EndOfBattle`
- `type = until_end_of_turn` -> `EndOfTurn`
- `type = continuous` -> `Permanent`
- `type = manual` -> `Manual`

`instant`/`none` rejects for modifier effects because a stat modifier needs an
explicit cleanup boundary.

## Target Rule

The current helper accepts a resolved `StructuredTargetCandidate`, not a target
ref string. Target resolution remains owned by `StructuredTargetTemplate`.

The helper verifies the target through `CombatStatProjector`, so hidden or
missing units reject without ledger mutation.

## Boundary

M12-08 does not:

- put combat modifiers into `GameState.event_log`
- mutate unit `power_delta` directly
- resolve targets from `target_ref`
- execute arbitrary card text
- publish network payloads

## Verification

EditMode coverage verifies:

- PowerPlus preview creates a ledger/projection and does not mutate source state
  or source ledger
- CriticalPlus apply mutates only the provided ledger
- unsupported duration rejects with manual-resolution requirement
- missing amount rejects without manual fallback
- hidden/missing target rejects without ledger mutation
- result JSON round-trip

## Next Work

`M12-09` should add an ability fixture DSL that can express before/action/after
scenarios for structured costs, targets, effects, and manual fallbacks.
