# Pending Auto Ability Resolution Request Session Storage Spec

## Status

Implemented in `M10-68`.

## Purpose

Store received selected pending AUTO ability resolution request payloads in the
online game session without executing them.

This keeps request diagnostics and future resolve UX separate from true
`GameState` mutation.

## Behavior

- `MultiplayerGameSessionController` subscribes to
  `PendingAutoAbilityResolutionRequestReceived`.
- Valid same-room payloads are appended to
  `PendingAutoAbilityResolutionRequestPayloads`.
- Null payloads are ignored.
- Room mismatches are rejected with
  `PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_ROOM_MISMATCH`.
- Receipt updates `LastMessage` and raises `StateChanged`.
- Receipt does not mutate `GameState.event_log` or execute abilities.
- Normal game-event sync continues after request receipt.

## Boundary

This milestone must not:

- add PlayTable publish controls
- resolve pending abilities
- validate costs or targets
- mutate `GameState`
- auto-answer request payloads

## Acceptance Tests

- incoming request payload is stored without mutating true state
- null request payload is ignored
- normal game event sync still works after request receipt

## Future Extensions

- publish helper for selected request
- PlayTable publish button
- resolver validation against live pending queue state
