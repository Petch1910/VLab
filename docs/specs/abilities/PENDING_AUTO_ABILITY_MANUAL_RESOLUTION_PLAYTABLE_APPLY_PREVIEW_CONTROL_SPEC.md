# Pending Auto Ability Manual Resolution PlayTable Apply Preview Control Spec

## Status

Implemented in `M10-98`.

## Purpose

Add an online PlayTable control for preview-applying the latest pending AUTO
ability manual resolution decision to the latest pending AUTO ability queue
payload.

This milestone only calls the session preview helper and updates status text.
It does not publish payloads, resolve card effects, or mutate `GameState`.

## Behavior

- Online PlayTable shows `ApplyDec`.
- `ApplyDec` calls
  `PreviewApplyLatestPendingAutoAbilityManualResolutionDecision()`.
- Status text is formatted with
  `PendingAutoAbilityManualResolutionApplyResultFormatter`.
- Local PlayTable does not show `ApplyDec`.

## Boundary

This milestone must not:

- send queue or decision payloads
- resolve card effects
- mutate `GameState`
- unblock automatic arbitrary card ability resolution

## Acceptance Tests

- online PlayTable apply-preview stores a returned queue payload without sending
- local PlayTable does not expose `ApplyDec`
- missing queue/decision shows a rejection status
- apply preview does not mutate `GameState`
