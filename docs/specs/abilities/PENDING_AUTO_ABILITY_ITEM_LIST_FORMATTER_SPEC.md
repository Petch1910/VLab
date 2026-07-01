# Pending Auto Ability Item List Formatter Spec

## Status

Implemented in `M10-55`.

## Purpose

Provide a pure formatter that decodes the latest pending AUTO ability queue
payload into a compact, masked-safe item list for future UI prompt/debug
surfaces.

This does not resolve abilities or mutate the queue. It only formats already
received payloads.

## Behavior

- No payloads format as `Pending ability items: 0`.
- Invalid latest payloads format with the deterministic rejection reason.
- Empty decoded queues show zero items plus queue id.
- Non-empty queues show item count, queue id, and capped item lines.
- Each item includes pending id, player index, timing event, source summary, and
  ability summary.
- Hidden/masked sources render as `source=hidden`.
- Long queues are capped and show a remaining-count line.

## Boundary

This milestone must not:

- add PlayTable prompt controls
- choose ability resolution order
- pay costs or resolve effects
- mutate `GameState`, payloads, or source queues

## Acceptance Tests

- no-payload output is deterministic
- invalid and empty payloads format correctly
- hidden source ids are not leaked
- visible source ids are formatted when not hidden
- long queues are capped with remaining count
- formatting does not mutate source payloads

## Future Extensions

- PlayTable pending ability prompt panel
- ability selection controls
- legal command generation for selected pending ability resolution
