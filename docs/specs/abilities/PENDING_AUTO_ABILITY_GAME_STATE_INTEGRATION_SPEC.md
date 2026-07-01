# Pending Auto Ability GameState Integration Spec

## Status

Implemented in `M10-44`.

## Purpose

Integrate the pending auto ability queue model into `GameState`.

This makes the queue available to future ability automation while keeping the
current simulator behavior unchanged.

## Behavior

- `GameState` owns one `pending_auto_abilities` queue.
- `GameState.EnsureLists` initializes the queue when missing.
- `GameState.EnsureLists` initializes the queue's internal pending list.
- `GameState.ToJson` / `GameState.FromJson` preserve queued pending ability
  data.

## Boundary

This milestone must not:

- enqueue automatic abilities from gameplay events
- resolve ability effects
- pay ability costs
- move cards between zones
- inspect deck order
- send multiplayer payloads
- change legal action generation

## Acceptance Tests

- `GameState.EnsureLists` initializes a missing pending auto ability queue
- `GameState.FromJson` initializes a missing pending auto ability queue
- `GameState` JSON round-trip preserves pending queue and ability fields
- Unity compile and EditMode tests pass

## Future Extensions

- ability trigger event collector
- pending ability priority chooser
- public/replay masking for pending ability data
- multiplayer payloads for ability resolution choices
