# Replay Determinism Verifier Spec

## Status

Implemented in `M11-07`.

## Purpose

Verify that supported `GameEvent` sequences reproduce the same final
`GameState` when replayed from the initial state. This is the correctness gate
between event-sourcing coverage (`M11-06`) and snapshot/rollback verification
(`M11-08`).

## Scope

The verifier supports events handled by `GameEventReducer`:

- `Draw`
- `MoveCard`
- `SetPhase`
- `AddGiftMarker`

Explicit event-sourcing exceptions from `EVENT_SOURCING_COVERAGE_SPEC.md`
remain out of scope until they have replayable event representation:

- `GameActionService.UndoLast`
- `AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent`

## Behavior

`ReplayDeterminismVerifier.Verify(initialState, expectedFinalState, events)`:

1. Clones the initial state and clears its event log for replay construction.
2. Clones the input events.
3. Replays events through `GameReplayPlayer` / `GameEventReducer`.
4. Normalizes expected and replayed final state clones.
5. Compares stable hashes of those normalized states.

The verifier rejects:

- missing initial state
- missing expected final state
- replay apply failure
- final state mismatch

## Boundary

The verifier must not:

- mutate the source initial state
- mutate the source expected final state
- mutate source event entries
- resolve unsupported abilities
- include pending AUTO queue commits until they become replayable events

## Verification

EditMode coverage verifies:

- supported RulesCore command scripts replay to the same final state
- divergent final states reject
- unsupported replay events reject without throwing
- verifier does not mutate source states or event entries
- result JSON round-trip
- missing state rejection paths

## Next Work

`M11-08` verifies snapshot/rollback isolation so simulation branches cannot
mutate live state.
