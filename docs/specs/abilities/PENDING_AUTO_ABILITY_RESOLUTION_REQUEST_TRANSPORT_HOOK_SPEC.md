# Pending Auto Ability Resolution Request Transport Hook Spec

## Status

Implemented in `M10-67`.

## Purpose

Add the transport interface hook for selected pending AUTO ability resolution
request payloads.

This lets fake transports and the Photon adapter send/receive the already
serialized request payload. It still does not resolve abilities or mutate
`GameState`.

## Behavior

- `IMultiplayerTransport` exposes send and received-event surfaces for
  `NetworkPendingAutoAbilityResolutionRequestPayload`.
- `PhotonRealtimeTransportAdapter` validates null sends with
  `PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING`.
- Photon test receive path decodes wrapper event code `10` and raises the
  received event.
- Live Photon bridge has a send path that raises event code `10`.
- Transport send/receive does not mutate game state.

## Boundary

This milestone must not:

- add session storage
- publish from PlayTable
- resolve pending abilities
- validate costs or targets
- mutate `GameState`

## Acceptance Tests

- fake transport can send and emit request payloads
- Photon adapter dispatches decoded request payloads
- Photon adapter rejects null request payloads
- transport hook does not mutate `GameState`

## Future Extensions

- session storage
- PlayTable publish selected-resolution request button
- resolver validation against live pending queue state
