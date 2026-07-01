# Phase / Timing Matrix Spec

## Status

Implemented as a read-only first matrix in `M11-02`.

## Purpose

Convert the M11-01 timing audit into a first explicit matrix that says which
current commands or resolver surfaces are legal in representative phases and
timing windows.

This is a correctness scaffold. It does not yet enforce `RulesCore` behavior.
M11-03 through M11-05 will harden command facade coverage and reject
no-mutation guarantees.

## TimingWindow Enum

The first `TimingWindow` enum includes:

- `None`
- `PhaseTransition`
- `StandAndDrawStep`
- `RideStep`
- `MainStep`
- `BattleStep`
- `AttackDeclaration`
- `GuardStep`
- `DriveCheck`
- `DamageCheck`
- `BattleResolution`
- `CloseStep`
- `EndPhase`
- `OnDraw`
- `OnMoveCard`
- `OnSetPhase`
- `OnAddGiftMarker`
- `PendingAutoResolution`
- `CleanupEndOfBattle`
- `CleanupEndOfTurn`
- `ManualTriggerCheck`

Some windows are reserved before full battle automation exists. They are present
so later work can move from string timing keys to typed windows without
renaming the whole system again.

## Matrix Entries

`PhaseTimingMatrix.CreateCurrentMatrix()` currently covers:

- `GameAction.Draw`
- `GameAction.MoveCard`
- `GameAction.SetPhase`
- `GameAction.AddGiftMarker`
- `PendingAuto.Resolution`
- `TriggerCheck.Drive`
- `TriggerCheck.Damage`
- `TriggerCheck.Manual`
- `Cleanup.EndOfBattle`
- `Cleanup.EndOfTurn`

Each entry records:

- command id
- phase
- timing window
- legal flag
- notes

## Current Representative Rules

- Draw is legal at `StandAndDraw` / `OnDraw`.
- Draw is rejected at `Battle` / `OnDraw`.
- MoveCard is legal at `Ride` / `OnMoveCard` for the current ride-to-vanguard
  UI path until a dedicated Ride command exists.
- MoveCard is legal at `Main` / `OnMoveCard`.
- MoveCard is rejected at `Battle` / `OnMoveCard` until battle-specific move
  commands exist.
- SetPhase remains legal from the current coarse phases as a manual phase
  transition surface.
- AddGiftMarker is legal at `Main` / `OnAddGiftMarker` and rejected at
  `Battle` / `OnAddGiftMarker`.
- Drive and damage trigger checks are legal in battle windows, rejected in main
  phase.
- End-of-battle cleanup is legal in battle cleanup timing.
- End-of-turn cleanup is legal in end phase cleanup timing.

## Boundary

M11-02 must not:

- change `RulesCore.GetLegalActions`
- change `RulesCore.TryExecute`
- mutate `GameState`
- append to `GameState.event_log`
- change PlayTable behavior
- publish network payloads

## Verification

EditMode coverage verifies:

- current `GameActionType` values map to typed timing windows
- matrix includes current game-action entries
- representative game action legal/rejected combinations
- representative trigger check and cleanup legal/rejected combinations
- JSON round-trip
- no duplicate command/phase/window entries

## Next Work

`M11-03` audits command facade coverage and starts routing important gameplay
actions through the same command/query boundary using this matrix as reference.
