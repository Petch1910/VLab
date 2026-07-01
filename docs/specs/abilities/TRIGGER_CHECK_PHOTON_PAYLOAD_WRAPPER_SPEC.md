# Trigger Check Photon Payload Wrapper Spec

## Status

Implemented in `M10-12`.

## Purpose

Wrap `NetworkTriggerCheckReplayLogPayload` in `PhotonRealtimePayload` with a
reserved event code so the payload can be carried by Photon later.

This scaffold only covers codec behavior. It does not add transport events,
adapter dispatch, lobby/session controller behavior, or live sends.

## Inputs

- `NetworkTriggerCheckReplayLogPayload`
- `PhotonRealtimePayload`

## Output

- Photon payload with trigger-check replay event code
- decoded trigger-check replay payload
- rejection reason for wrong event code, bad JSON, or protocol mismatch

## Boundary

The wrapper must not:

- mutate source replay logs
- perform masking implicitly
- send through Photon
- add `IMultiplayerTransport` events
- apply entries to `GameState`

## Acceptance Tests

- Photon wrapper round-trips a trigger check replay payload
- wrong Photon event code is rejected
- inner protocol mismatch is rejected
- wrapper JSON is deterministic
- wrapping does not mutate the source log

## Future Extensions

- `IMultiplayerTransport` send/receive hooks
- Photon adapter dispatch for the reserved event code
- multiplayer game-session storage of received trigger check logs
- replay UI hydration from received trigger check payloads
