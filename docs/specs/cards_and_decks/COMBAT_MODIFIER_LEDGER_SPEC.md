# Combat Modifier Ledger Spec

## Status

Implemented in `M10-03`.

## Purpose

Track temporary power and critical modifiers as structured data before full
trigger/ability automation mutates unit stats.

The ledger is a scaffold. It stores and summarizes modifiers, but it does not
apply them to `GameState`, validate target legality, or perform end-phase
cleanup on the live board.

## Modifier Fields

- modifier id
- source id
- target card instance id
- power delta
- critical delta
- expiration timing
- note

## Expiration Timing

MVP enum:

- `Manual`
- `EndOfBattle`
- `EndOfTurn`
- `Permanent`

The enum records intent only. The future phase/window controller will decide
when cleanup is legal.

## Ledger Operations

- add modifier
- summarize total power/critical delta for one target
- list active modifiers for one target
- return a filtered copy without expired timing categories
- JSON round-trip through Unity `JsonUtility`

## Boundary

The ledger must not:

- mutate `GameState`
- parse card text
- consume RNG
- decide when a real battle/window has ended
- bypass future RulesCore cleanup actions

## Acceptance Tests

- summaries add power and critical deltas independently
- filtering by expiration returns a new ledger and leaves original intact
- JSON round-trip preserves modifier data
- repeated summaries are deterministic
- state JSON is unchanged after using the ledger

## Future Extensions

- integrate with trigger allocation application
- end-of-battle and end-of-turn cleanup actions
- continuous-effect recalculation layer
- source ability/event linkage in replay logs
