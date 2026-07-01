# Pending Auto Ability Resolution Request Session Publish Spec

## Status

Implemented in `M10-69`.

## Purpose

Add a session-level publish helper for the latest stored selected pending AUTO
ability resolution request payload.

This is still only a network request publication path. It does not resolve the
ability or mutate `GameState`.

## Behavior

- `CanPublishPendingAutoAbilityResolutionRequest` is true when at least one
  request payload is stored.
- Missing payloads reject with
  `PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING`.
- Publishing sends the latest stored request payload through
  `IMultiplayerTransport.SendPendingAutoAbilityResolutionRequest`.
- Publishing updates `LastMessage` and raises `StateChanged`.
- Publishing does not mutate `GameState.event_log`.

## Boundary

This milestone must not:

- add PlayTable publish controls
- resolve pending abilities
- validate costs or targets
- mutate `GameState`
- remove stored request payloads after sending

## Acceptance Tests

- publish without stored payload rejects deterministically
- publish sends the latest stored request payload
- publish keeps the stored payload available
- publish does not mutate `GameState`

## Future Extensions

- PlayTable publish selected-resolution request button
- resolver validation against live pending queue state
- request acknowledgement/result payload
