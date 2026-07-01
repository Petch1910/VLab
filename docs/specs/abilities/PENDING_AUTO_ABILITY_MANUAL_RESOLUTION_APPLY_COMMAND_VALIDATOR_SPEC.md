# Pending Auto Ability Manual Resolution Apply Command Validator Spec

## Status

Implemented in `M10-94`.

## Purpose

Validate that a pending AUTO ability manual resolution decision can be applied
to the current pending queue.

This milestone validates only. It does not mutate the queue, resolve abilities,
publish payloads, or mutate `GameState`.

## Behavior

- Rejects missing or empty pending queue with
  `PENDING_AUTO_ABILITY_QUEUE_MISSING`.
- Rejects missing decision with
  `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING`.
- Rejects unsupported decision types with
  `PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID`.
- Rejects decisions whose pending id does not match the first pending queue
  item with `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_PENDING_ID_MISMATCH`.
- Accepted validation returns an accepted apply result with queue id, pending id,
  decision type, and a summary.

## Boundary

This milestone must not:

- remove pending abilities from the queue
- apply `Resolve`, `Skip`, or `Defer`
- resolve card effects
- publish payloads
- mutate `GameState`
- mutate source queue or decision objects

## Acceptance Tests

- valid queue plus decision creates an accepted apply result
- missing queue is rejected
- missing decision is rejected
- pending id mismatch is rejected
- unsupported decision type is rejected
- source queue and decision are not mutated
