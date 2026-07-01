# Pending Auto Ability Manual Resolution Decision Session Storage Spec

## Status

Implemented in `M10-78`.

## Purpose

Store received pending AUTO ability manual resolution decision payloads in the
multiplayer game session.

This milestone records the decision payload for UI/debug/replay surfaces later.
It does not resolve, skip, defer, or otherwise apply pending abilities.

## Behavior

- `MultiplayerGameSessionController` subscribes to manual resolution decision
  payload events from `IMultiplayerTransport`.
- Valid received payloads for the current room are appended to a read-only list.
- Null payloads are ignored.
- Wrong-room payloads are rejected with a session message and are not stored.
- Storage is separate from `GameState` and `GameState.event_log`.

## Boundary

This milestone must not:

- mutate `GameState`
- mutate `GameState.event_log`
- apply Resolve/Skip/Defer effects
- publish new decision payloads
- add PlayTable controls

## Acceptance Tests

- received decision payloads are stored outside `GameState`
- null decision payloads are ignored
- normal game-event sync still works after decision receipt
- receipt does not mutate true state
