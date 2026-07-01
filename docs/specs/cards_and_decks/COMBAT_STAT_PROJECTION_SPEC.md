# Combat Stat Projection Spec

## Status

Implemented in `M10-05`.

## Purpose

Project advisory combat stat deltas for a visible unit by combining current unit
state with `CombatModifierLedger` summaries.

This scaffold is a read-only view. It does not apply modifiers to `GameState`
and does not yet integrate printed base power from card metadata.

## Inputs

- `GameState` or masked state view selected by caller
- player index
- target card instance id
- `CombatModifierLedger`

## Output

Projection fields:

- accepted/rejected status
- target card instance id and card id
- zone and zone index
- current power delta from `GameCardInstance.power_delta`
- ledger power delta
- ledger critical delta
- projected power delta total
- projected critical delta total
- modifier count
- explanation text

## Boundary

The projection helper must not:

- mutate `GameState`
- infer printed base power
- parse card text
- validate battle windows
- select hidden or face-down cards

## Acceptance Tests

- ledger deltas apply only to the requested target
- missing target returns a clear rejection
- hidden target is rejected
- projection JSON round-trips
- repeated projection is deterministic
- state JSON is unchanged after projection

## Future Extensions

- printed base power lookup through card repository
- base critical and drive data
- continuous-effect recomputation
- projection for a whole attack line
- UI display of projected combat stats
