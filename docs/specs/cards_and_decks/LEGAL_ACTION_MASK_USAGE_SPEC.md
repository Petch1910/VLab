# Legal Action Mask Usage Spec

## Status

Implemented as a read-only usage report in `M11-04`.

## Purpose

Harden the command path by making UI, bot, session, and ability-core usage of
legal actions explicit and testable before changing runtime enforcement.

This report distinguishes:

- paths that choose from `RulesCore.GetLegalActions`
- paths that build an intent but validate through `RulesCore.TryExecute`
- paths that still mutate directly and must remain documented exceptions

## Hardened Paths

Current hardened paths:

- `PlayTable.Draw`: uses legal action generator and validates through RulesCore
- `PlayTable.MoveSelected`: builds a selected-card move intent and validates
  through RulesCore before mutation
- `PlayTable.SetPhase`: uses legal action generator and validates through
  RulesCore
- `PlayTable.AddGiftMarker`: uses legal action generator and validates through
  RulesCore
- `EasyBot.Decision`: chooses from legal actions and executes through RulesCore
- `ProfileBot.Decision`: chooses from legal actions and executes through
  RulesCore
- `MultiplayerSession.ExecuteLocalAction`: validates inbound local action
  through RulesCore before publishing
- `AbilityCore.ExecuteCommand`: uses legal action lookup / RulesCore validation
  for structured scaffold commands

## Explicit Exception

`PlayTable.Undo` is the only current direct mutation exception:

- uses `GameActionService.UndoLast`
- does not use legal action generator
- does not validate through RulesCore

This remains allowed only as a documented manual/replay exception until M11
event-sourcing and replay determinism work decides how undo is represented.

## Boundary

M11-04 must not:

- change PlayTable behavior
- change bot behavior
- change multiplayer session behavior
- mutate `GameState`
- add command enforcement beyond the existing RulesCore calls
- remove the documented Undo exception

## Verification

EditMode coverage verifies:

- UI, bot, session, and ability-core paths are reported
- hardened paths either use legal action generator or validate with RulesCore
- `PlayTable.Undo` is the only direct mutation exception
- report JSON round-trip
- no duplicate path ids

## Next Work

`M11-05` adds reject no-mutation guarantees for invalid commands and related
preview/queue paths.
