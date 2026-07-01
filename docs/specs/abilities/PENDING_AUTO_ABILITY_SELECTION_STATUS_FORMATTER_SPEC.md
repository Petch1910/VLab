# Pending Auto Ability Selection Status Formatter Spec

## Status

Implemented in `M10-58`.

## Purpose

Format pending AUTO ability selection states into deterministic, masked-safe
status text for future PlayTable prompt controls.

## Behavior

- Null state and cleared selection format as no selection.
- Rejected states include the deterministic rejection reason.
- Accepted selections show selected index, pending id, player index, timing
  event, and source summary.
- Hidden source identities render as `source=hidden`.
- Formatting does not mutate selected ability data.

## Boundary

This milestone must not:

- add PlayTable controls
- select abilities from UI
- resolve pending abilities
- mutate `GameState` or selected ability data

## Acceptance Tests

- null/no-selection output is deterministic
- rejection output includes reason
- visible selection formats card and instance ids
- hidden selection does not leak source identity
- selected ability data is unchanged after formatting

## Future Extensions

- PlayTable selected pending ability status text
- cycle/select buttons
- resolution command factory
