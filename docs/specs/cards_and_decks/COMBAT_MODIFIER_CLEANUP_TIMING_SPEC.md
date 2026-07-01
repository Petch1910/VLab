# Combat Modifier Cleanup Timing Spec

## Status

Implemented in `M10-06`.

## Purpose

Preview which combat modifiers would expire at a timing checkpoint and return a
new remaining ledger without mutating the original ledger or `GameState`.

This is a scaffold for future end-of-battle and end-of-turn RulesCore cleanup
commands.

## Inputs

- `CombatModifierLedger`
- `CombatModifierExpiration` timing to clean up

## Output

- timing
- expired modifier count
- remaining modifier count
- expired modifier ids
- remaining ledger
- explanation

## Boundary

The helper must not:

- mutate the input ledger
- mutate `GameState`
- decide that a phase/window has actually ended
- execute cleanup through RulesCore

## Acceptance Tests

- end-of-battle preview removes only end-of-battle modifiers
- end-of-turn preview removes only end-of-turn modifiers
- original ledger is unchanged
- result JSON round-trips if serializable
- repeated previews are deterministic

## Future Extensions

- cleanup commands routed through RulesCore
- event log entries for cleanup
- cleanup by source id
- phase/window controller integration
