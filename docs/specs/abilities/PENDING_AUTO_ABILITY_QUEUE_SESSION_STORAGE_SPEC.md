# Pending Auto Ability Queue Session Storage Spec

## Status

Implemented in `M10-51`.

## Purpose

Store received pending AUTO ability queue payloads in
`MultiplayerGameSessionController` separately from `GameState`.

This mirrors trigger-check diagnostic payload storage and prepares a later
PlayTable summary/prompt surface without changing live gameplay state.

## Behavior

- Session subscribes to `IMultiplayerTransport.PendingAutoAbilityQueueReceived`.
- Non-null payloads for the current room are appended to a read-only exposed
  list.
- Null payloads are ignored.
- Room mismatches are rejected with `LastMessage` and are not stored.
- Received payloads do not mutate `GameState.event_log` or any card zones.

## Boundary

This milestone must not:

- apply pending queues into `GameState`
- resolve ability effects
- pay costs
- open PlayTable prompts
- publish pending ability payloads automatically
- change normal game-event sync behavior

## Acceptance Tests

- received pending ability queue payloads are stored outside `GameState`
- null payloads are ignored
- normal game-event sync still works after pending payload receipt
- `GameState` remains unchanged by receipt storage

## Future Extensions

- PlayTable pending ability queue summary surface
- pending ability prompt UI
- explicit command to commit returned queues into `GameState`
- owner-private payload routing rules
