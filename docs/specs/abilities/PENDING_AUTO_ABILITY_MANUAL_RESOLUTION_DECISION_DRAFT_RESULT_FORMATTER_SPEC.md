# Pending Auto Ability Manual Resolution Decision Draft Result Formatter Spec

## Status

Implemented in `M10-89`.

## Purpose

Format PlayTable status text for pending AUTO ability manual resolution decision
draft creation results.

This milestone only centralizes UI text. It does not change draft creation,
session storage, transport, or ability resolution behavior.

## Behavior

- Accepted result formats as
  `Created pending auto ability manual resolution decision draft.`
- Rejected result formats as the rejection reason.
- Null result formats as
  `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_DRAFT_RESULT_MISSING`.
- PlayTable `DraftDec` uses this formatter.

## Boundary

This milestone must not:

- create draft payloads directly
- publish payloads
- resolve, skip, or defer abilities
- mutate `GameState`

## Acceptance Tests

- accepted result formats deterministic success message
- rejected result formats rejection reason
- null result formats deterministic fallback
- PlayTable `DraftDec` status uses the formatter
