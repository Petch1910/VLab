# Trigger Check Transport Hook Spec

## Status

Implemented in `M10-13`.

## Purpose

Expose transport-level send/receive hooks for
`NetworkTriggerCheckReplayLogPayload` so future multiplayer session controllers
can carry masked trigger check replay/debug logs.

This scaffold only adds transport surface and adapter dispatch. It does not
store received logs in a game session, mutate `GameState`, or render UI.

## Inputs

- `NetworkTriggerCheckReplayLogPayload`
- `PhotonRealtimePayload`
- fake/mock transport send and receive paths

## Output

- `IMultiplayerTransport` send method
- `IMultiplayerTransport` receive event
- Photon adapter send/decode dispatch path
- fake transport test coverage

## Boundary

The hook must not:

- mutate `GameState`
- append to `GameState.event_log`
- apply trigger modifiers
- perform implicit masking
- start sending trigger logs from gameplay controllers yet

## Acceptance Tests

- fake transport can send and emit trigger check replay payloads
- Photon adapter dispatches decoded trigger check replay payloads
- sending null payload is rejected
- transport hook does not mutate `GameState`
- Unity compile and EditMode tests pass

## Future Extensions

- `MultiplayerGameSessionController` storage for received trigger logs
- PlayTable/replay UI display of received trigger logs
- live Photon smoke test for trigger check log payload delivery
