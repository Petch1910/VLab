# Pending Auto Ability Queue Session Publish Spec

## Status

Implemented in `M10-53`.

## Purpose

Allow `MultiplayerGameSessionController` to republish the latest stored pending
AUTO ability queue payload through the transport seam.

This is still diagnostic/scaffold behavior. It does not create PlayTable
buttons, resolve abilities, or commit queues into `GameState`.

## Behavior

- If no pending queue payload is stored, publishing is rejected.
- If a payload exists, the latest stored payload is sent through
  `IMultiplayerTransport.SendPendingAutoAbilityQueue`.
- Publishing does not add or remove stored payloads.
- Publishing does not mutate `GameState`.

## Boundary

This milestone must not:

- resolve pending abilities
- pay costs
- add PlayTable publish controls
- apply queue changes to `GameState`
- create owner-private prompt routing

## Acceptance Tests

- no-payload publish is rejected with a deterministic error code
- valid publish sends the latest stored payload
- stored payload count is unchanged by publishing
- `GameState` is unchanged by publishing

## Future Extensions

- PlayTable publish control
- pending ability prompt UI
- owner-private ability choice routing
- RulesCore command to commit selected pending ability resolution
