# Pending Auto Ability Manual Resolution Decision List Formatter Spec

## Status

Implemented in `M10-83`.

## Purpose

Format stored pending AUTO ability manual resolution decision payloads as a
bounded newest-first list.

This milestone only adds a pure formatter. It does not wire the formatter into
PlayTable, publish payloads, or apply decisions.

## Behavior

- No payloads format as `Pending manual decision list: 0`.
- Payloads are displayed newest-first.
- Output is capped by `maxItems`; older entries are summarized.
- Invalid payloads are reported with rejection reasons.
- Hidden source decisions show `source=hidden` and do not leak hidden source ids.
- Formatting does not mutate source payloads.

## Boundary

This milestone must not:

- add PlayTable labels
- publish payloads
- resolve, skip, or defer pending abilities
- mutate `GameState`

## Acceptance Tests

- no-payload list is deterministic
- newest-first order is deterministic
- invalid payloads are reported without throwing
- hidden-source entries do not leak source ids
- formatting does not mutate source payloads
