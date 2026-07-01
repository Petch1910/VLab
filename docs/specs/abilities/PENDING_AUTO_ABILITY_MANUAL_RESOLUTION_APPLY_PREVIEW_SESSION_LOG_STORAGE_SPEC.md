# Pending Auto Ability Manual Resolution Apply Preview Session Log Storage Spec

## Status

Implemented in `M10-101`.

## Purpose

Store pending AUTO ability manual resolution apply preview log entries in the
game-session controller.

This milestone keeps the storage session-local. It does not publish logs, write
to `GameState.event_log`, resolve card effects, or mutate `GameState`.

## Behavior

- `MultiplayerGameSessionController` exposes:
  - `PendingAutoAbilityManualResolutionApplyPreviewLogs`
  - `LatestPendingAutoAbilityManualResolutionApplyPreviewLog`
- Every call to
  `PreviewApplyLatestPendingAutoAbilityManualResolutionDecision()` appends one
  log entry for the resulting apply preview.
- Accepted preview attempts continue to store the returned queue payload in
  session history only.
- Rejected preview attempts store only a rejected log entry.

## Boundary

This milestone must not:

- send apply preview logs over transport
- add PlayTable log surfaces
- write to `GameState.event_log`
- resolve card effects
- mutate source queue payloads, decision payloads, or `GameState`

## Acceptance Tests

- accepted apply preview appends a log entry
- rejected missing-queue preview appends a rejected log entry
- rejected missing-decision preview appends a rejected log entry
- latest log accessor returns the newest log
- storage does not publish network payloads or mutate `GameState`
- accepted queue-payload storage behavior remains unchanged
