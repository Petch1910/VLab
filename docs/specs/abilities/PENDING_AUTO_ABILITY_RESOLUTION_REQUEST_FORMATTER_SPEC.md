# Pending Auto Ability Resolution Request Formatter Spec

## Status

Implemented in `M10-63`.

## Purpose

Add a pure formatter for the pending AUTO ability selected-resolution request
model.

This gives PlayTable/debug surfaces a stable text summary before any resolve
button or transport payload exists.

## Behavior

- Null result formats as `Pending resolve request: no result`.
- Null accepted request formats as `Pending resolve request: none`.
- Rejected results show the deterministic rejection reason.
- Visible requests show selected index, pending id, player index, timing event,
  and visible source card/instance ids.
- Hidden requests format the source as `hidden` without leaking source instance
  ids.
- Formatting does not mutate the request or selected ability source data.

## Boundary

This milestone must not:

- resolve pending abilities
- validate costs or targets
- mutate `GameState`
- send request payloads over transport
- add PlayTable resolve buttons

## Acceptance Tests

- null result and null request fall back to stable text
- rejected request result formats the rejection reason
- visible request formats source metadata
- hidden request does not leak source identity
- formatting does not mutate the request JSON

## Future Extensions

- PlayTable selected-resolution preview surface
- PlayTable resolve-selected button
- resolution request payload codec
