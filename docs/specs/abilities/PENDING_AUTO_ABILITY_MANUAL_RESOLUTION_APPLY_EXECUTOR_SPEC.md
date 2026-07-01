# Pending Auto Ability Manual Resolution Apply Executor Spec

## Status

Implemented in `M10-96`.

## Purpose

Apply a validated pending AUTO ability manual resolution decision to a cloned
pending queue.

This milestone mutates only the returned cloned queue. It does not mutate the
source queue, resolve card effects, publish payloads, or mutate `GameState`.

## Behavior

- Runs `PendingAutoAbilityManualResolutionApplyCommandValidator` first.
- Rejected validation returns the original queue cloned when possible plus the
  rejected apply result.
- `Skip` removes the first pending item from the returned cloned queue.
- `Defer` moves the first pending item to the end of the returned cloned queue.
- `Resolve` returns accepted result but leaves the returned cloned queue
  unchanged because structured ability execution is not wired here yet.

## Boundary

This milestone must not:

- resolve card effects
- call `AbilityCore`
- publish payloads
- mutate source queue or decision objects
- mutate `GameState`

## Acceptance Tests

- skip removes the matching first pending item from the returned queue
- defer moves the matching first pending item to the end of the returned queue
- resolve returns accepted result but does not execute effects or mutate queue
- invalid apply inputs are rejected through the validator
- source queue and decision are not mutated
