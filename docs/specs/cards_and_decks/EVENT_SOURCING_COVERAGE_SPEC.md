# Event Sourcing Coverage Spec

## Status

Implemented as a read-only coverage report in `M11-06`.

## Purpose

Accepted gameplay mutations must be replayable. The first M11-06 slice makes
the current event-sourcing boundary explicit and testable before changing the
reducer or adding new `GameActionType` variants.

## Covered GameEvent Paths

The current `GameActionService` mutations are covered by `GameEvent` and
`GameEventReducer`:

- `GameActionService.Draw`
- `GameActionService.MoveCard`
- `GameActionService.SetPhase`
- `GameActionService.AddGiftMarker`

`AbilityCore.StructuredEffects` remains covered because structured effects
delegate to `RulesCore.TryExecute`, which returns the same `GameEvent` used by
live play, replay, bot, and multiplayer.

## Explicit Exceptions

Current explicit exceptions:

- `GameActionService.UndoLast`
  - reverses the last event and removes it from `GameState.event_log`
  - remains isolated from bot/network command paths
  - M11-07 must decide whether undo is represented as replay metadata or stays
    an editor/manual-only utility

- `AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent`
  - mutates `GameState.pending_auto_abilities`
  - currently records specialized pending AUTO metadata, not a
    `GameEventReducer` event
  - M11/M12 follow-up must decide whether pending AUTO queue commits become
    `GameEvent` variants or a parallel replay stream

## Boundary

M11-06 does not:

- change `GameEventReducer`
- add unsupported ability execution
- change PlayTable/session behavior
- publish network payloads
- mutate `GameState`

## Verification

EditMode coverage verifies:

- every current `GameActionType` has an event-sourced report entry
- current GameActionService mutation paths create `GameEvent`, append to
  `GameState.event_log`, and are replayable through `GameEventReducer`
- structured AbilityCore effects stay routed through RulesCore event paths
- `UndoLast` and pending AUTO timing commit remain explicit exceptions
- report JSON round-trip
- no duplicate path ids

## Next Work

`M11-07` adds replay determinism verification for the event log surfaces that
are already event-sourced.
