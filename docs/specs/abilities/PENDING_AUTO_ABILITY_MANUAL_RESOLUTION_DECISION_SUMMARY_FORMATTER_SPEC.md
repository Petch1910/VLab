# Pending Auto Ability Manual Resolution Decision Summary Formatter Spec

## Status

Implemented in `M10-81`.

## Purpose

Format the latest stored pending AUTO ability manual resolution decision payload
for UI/debug surfaces.

This milestone only adds a pure formatter. It does not wire the formatter into
PlayTable, publish decisions, or resolve abilities.

## Behavior

- No payloads format as `Pending manual decisions: 0`.
- Invalid latest payloads include the rejection reason.
- Valid latest payloads show:
  - total payload count
  - latest payload id
  - decision type
  - selected index
  - pending id
  - player index
  - timing event
  - source summary
- Hidden source decisions show `source=hidden` and do not expose hidden source
  card ids or instance ids.
- Formatting does not mutate payloads or decoded decisions.

## Boundary

This milestone must not:

- add PlayTable labels
- publish payloads
- apply Resolve/Skip/Defer decisions
- mutate `GameState`

## Acceptance Tests

- no-payload summary is deterministic
- invalid payload summary is deterministic
- visible decision summary includes decision type, selected index, pending id,
  and payload id
- hidden-source summary does not leak hidden source ids
- formatting does not mutate the source payload
