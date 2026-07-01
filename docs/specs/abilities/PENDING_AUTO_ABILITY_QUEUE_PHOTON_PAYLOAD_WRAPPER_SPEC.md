# Pending Auto Ability Queue Photon Payload Wrapper Spec

## Status

Implemented in `M10-49`.

## Purpose

Wrap `NetworkPendingAutoAbilityQueuePayload` in `PhotonRealtimePayload` with a
reserved event code so pending AUTO ability queue payloads can be carried by
Photon in a later transport milestone.

This scaffold only covers codec behavior. It does not add transport events,
adapter dispatch, PlayTable UI, or live sends.

## Event Code

- `PendingAutoAbilityQueueEventCode = 9`

## Boundary

The wrapper must not:

- mutate source payloads or queues
- perform masking implicitly
- send through Photon
- add `IMultiplayerTransport` events
- apply pending abilities to `GameState`

## Acceptance Tests

- Photon wrapper round-trips a pending auto ability queue payload
- wrong Photon event code is rejected
- inner protocol mismatch is rejected
- wrapper JSON is deterministic
- wrapping does not mutate the source payload

## Future Extensions

- `IMultiplayerTransport` send/receive hooks
- Photon adapter dispatch for event code 9
- multiplayer game-session storage of received pending ability prompts
- PlayTable pending ability prompt surface
