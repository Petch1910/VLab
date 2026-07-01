# Pending Auto Ability PlayTable Selection Status Spec

## Status

Implemented in `M10-59`.

## Purpose

Add a read-only PlayTable text surface for pending AUTO ability selection
status. This creates a stable UI location before adding selection controls.

## Behavior

- PlayTable creates `Pending Ability Selection Status`.
- Local and online PlayTable show `Pending selection: none` while no selection
  controls exist.
- The surface uses `PendingAutoAbilitySelectionStatusFormatter`.
- Rendering does not send transport payloads or mutate `GameState`.

## Boundary

This milestone must not:

- add ability selection buttons
- resolve pending abilities
- mutate selection state from UI
- mutate `GameState`

## Acceptance Tests

- local PlayTable reports no pending selection
- online PlayTable reports no pending selection
- status surface exists
- rendering does not send pending queue payloads
- rendering does not mutate `GameState`

## Future Extensions

- cycle/select pending ability controls
- clear selected pending ability control
- selected pending ability resolution command
