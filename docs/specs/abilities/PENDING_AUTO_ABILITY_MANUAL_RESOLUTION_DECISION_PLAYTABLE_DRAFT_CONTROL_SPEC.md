# Pending Auto Ability Manual Resolution Decision PlayTable Draft Control Spec

## Status

Implemented in `M10-87`.

## Purpose

Add an online PlayTable control for creating a pending AUTO ability manual
resolution decision draft payload from the latest selected resolution request.

This milestone only creates and stores the draft payload. It does not publish
the payload and does not apply the decision to the game state.

## Behavior

- Online PlayTable shows `DraftDec`.
- `DraftDec` decodes the latest stored pending AUTO ability resolution request.
- `DraftDec` creates a `Resolve` manual decision draft payload through
  `PendingAutoAbilityManualResolutionDecisionDraftFactory`.
- Accepted drafts are stored in session pending manual decision payload history.
- `DecAuto` remains responsible for publishing the latest stored decision
  payload.
- Local PlayTable does not show `DraftDec`.

## Boundary

This milestone must not:

- publish the draft payload
- auto-resolve, skip, or defer abilities
- mutate `GameState`
- reveal hidden source card identity

## Acceptance Tests

- session draft creation rejects missing request payloads
- session draft creation stores a payload without sending it
- hidden-source requests remain redacted in stored draft payloads
- online PlayTable `DraftDec` creates a stored draft without publishing
- local PlayTable does not expose `DraftDec`
