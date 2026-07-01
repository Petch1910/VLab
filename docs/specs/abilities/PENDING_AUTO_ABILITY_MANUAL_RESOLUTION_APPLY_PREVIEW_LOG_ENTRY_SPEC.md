# Pending Auto Ability Manual Resolution Apply Preview Log Entry Spec

## Status

Implemented in `M10-99`.

## Purpose

Add a pure log entry model for pending AUTO ability manual resolution apply
preview results.

This milestone records apply-preview outcomes for later session/UI integration.
It does not write to `GameState.event_log`, publish payloads, resolve card
effects, or mutate `GameState`.

## Behavior

- `PendingAutoAbilityManualResolutionApplyPreviewLogEntry` records:
  - log entry id
  - accepted flag
  - rejection reason
  - queue id
  - pending id
  - decision type
  - summary
- Accepted entries copy the safe fields from an apply result.
- Rejected entries keep queue, pending, decision type, and summary empty.
- Entries contain no source card identity fields.

## Boundary

This milestone must not:

- store preview logs in session history
- show preview logs in PlayTable
- send network payloads
- write to `GameState.event_log`
- resolve card effects
- mutate `GameState`

## Acceptance Tests

- accepted apply preview log entry round-trips JSON
- rejected apply preview log entry round-trips JSON
- created entries do not include source card identity fields
- source apply results are not mutated
