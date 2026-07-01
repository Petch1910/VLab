# Structured Effect Resource Ops Template Spec

## Status

Implemented in `M12-07`.

## Purpose

Add the first structured resource operation effects without bypassing
`RulesCore`, replay determinism, or hidden-state boundaries.

## Runtime Surface

`StructuredEffectTemplate.Preview(state, playerIndex, effect)`:

- clones the state
- applies supported resource operations to the clone through `RulesCore`
- returns generated `GameEvent` entries
- does not mutate live state or `GameState.event_log`

`StructuredEffectTemplate.Apply(state, playerIndex, effect)`:

- applies supported resource operations to live state through `RulesCore`
- rejects without mutation when no legal resource action exists
- requires manual resolution for resource zones not modeled in live state yet

## Supported v1 Resource Effects

Event-sourced and replayable:

- `counter_blast`
- `counter_charge`

Both operations are represented as `GameActionType.ResourceFlip` and mutate only
the `face_up` state of cards in `GameZone.Damage`.

`counter_blast`:

- requires a face-up damage card
- flips that damage card face-down
- records `GameResourceOperationType.CounterBlast`
- records `previous_face_up = true`, `new_face_up = false`

`counter_charge`:

- requires a face-down damage card
- flips that damage card face-up
- records `GameResourceOperationType.CounterCharge`
- records `previous_face_up = false`, `new_face_up = true`

## Manual Placeholders

The following reject with `requires_manual_resolution = true`:

- `soul_charge`
- `soul_blast`

Reason: the current live `GameState` has no authoritative Soul zone. Do not use
the Vanguard stack as a hidden replacement for Soul. A later milestone must add
or explicitly model Soul before these effects can mutate live state.

## Event Sourcing

`ResourceFlip` is covered by:

- `GameActionType.ResourceFlip`
- `LegalActionGenerator`
- `RulesCore.Matches`
- `LegalActionExecutor`
- `GameActionService.ResourceFlip`
- `GameEventReducer.Apply`
- `GameEventReducer.UndoLast`
- `ReplayDeterminismVerifier`
- `EventSourcingCoverageCatalog`
- `RulesCoreCommandFacadeCoverage`

## Boundary

M12-07 does not:

- add a Soul zone
- execute arbitrary resource text
- bypass `RulesCore`
- mutate `GameState` directly from `StructuredEffectTemplate`
- publish network payloads
- treat probability or preview output as live RNG outcome

## Verification

EditMode coverage verifies:

- `counter_blast` apply flips damage face-down through `RulesCore`
- `counter_charge` preview does not mutate live state
- unavailable `counter_blast` rejects without mutation
- `soul_charge` and `soul_blast` require manual resolution
- `ResourceFlip` is covered by event-sourcing/facade reports
- `ResourceFlip` replay determinism passes
- event log formatting handles resource operations

## Next Work

`M12-08` adds structured PowerPlus/CriticalPlus modifier templates and duration
cleanup integration.
