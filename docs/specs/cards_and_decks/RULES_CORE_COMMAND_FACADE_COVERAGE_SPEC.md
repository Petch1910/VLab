# RulesCore Command Facade Coverage Spec

## Status

Implemented as a read-only audit in `M11-03`.

## Purpose

Document and test which current gameplay mutation commands are already routed
through the RulesCore command/query facade.

This closes the gap between the Phase / Timing Matrix and later legal action
mask hardening. It does not yet change runtime behavior.

## Covered Commands

The current `GameActionType` commands are covered by:

- `LegalActionGenerator`
- `LegalActionExecutor`
- `RulesCore.TryExecute` / matcher
- `GameActionService` mutation method

Covered command ids:

- `GameAction.Draw` -> `GameActionService.Draw`
- `GameAction.MoveCard` -> `GameActionService.MoveCard`
- `GameAction.SetPhase` -> `GameActionService.SetPhase`
- `GameAction.AddGiftMarker` -> `GameActionService.AddGiftMarker`

## Explicit Exception

`GameAction.UndoLast` remains outside the command facade:

- no `LegalGameAction`
- no generator coverage
- no executor coverage
- no RulesCore matcher coverage
- direct method: `GameActionService.UndoLast`

This is retained as an explicit exception until M11 event-sourcing and replay
determinism work decides how undo should be represented.

## Boundary

M11-03 must not:

- change `RulesCore.GetLegalActions`
- change `RulesCore.TryExecute`
- change `GameActionService`
- mutate `GameState`
- alter PlayTable, bot, or multiplayer behavior
- enforce the Phase / Timing Matrix

## Verification

EditMode coverage verifies:

- all current `GameActionType` commands are marked facade-covered
- each covered command records generator/executor/matcher coverage
- `UndoLast` is the only current explicit exception
- covered lookup handles unknown commands
- JSON round-trip
- no duplicate command ids

## Next Work

`M11-04` uses this audit to harden legal action mask usage by UI/bot-facing
paths while keeping documented exceptions explicit.
