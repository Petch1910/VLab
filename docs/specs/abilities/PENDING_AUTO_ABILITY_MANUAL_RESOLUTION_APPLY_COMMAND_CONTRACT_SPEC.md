# Pending Auto Ability Manual Resolution Apply Command Contract Spec

## Status

Implemented in `M10-93`.

## Purpose

Define a stable command/result contract for a future step that applies pending
AUTO ability manual resolution decisions to a pending queue.

This milestone is contract-only. It does not mutate pending queues, resolve
abilities, publish payloads, or mutate `GameState`.

## Behavior

- `PendingAutoAbilityManualResolutionApplyCommand` records:
  - command id
  - queue id
  - pending id
  - decision id
  - decision type
  - player index
- `PendingAutoAbilityManualResolutionApplyResult` records:
  - accepted flag
  - rejection reason
  - queue id
  - pending id
  - decision type
  - summary
- Stable rejection reason constants are defined for:
  - missing queue
  - missing decision
  - pending id mismatch
  - unsupported decision type

## Boundary

This milestone must not:

- apply decisions to queues
- remove pending abilities
- resolve card effects
- publish payloads
- mutate `GameState`

## Acceptance Tests

- command model round-trips JSON
- result model round-trips JSON
- rejection reason constants are stable
- no queue mutation behavior exists in this milestone
