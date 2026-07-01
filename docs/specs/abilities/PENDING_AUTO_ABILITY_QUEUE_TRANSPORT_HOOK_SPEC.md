# Pending Auto Ability Queue Transport Hook Spec

## Status

Implemented in `M10-50`.

## Purpose

Add transport seam support for pending AUTO ability queue payloads.

This milestone only adds interface, fake transport, and Photon adapter
send/receive hooks. It does not store received payloads in
`MultiplayerGameSessionController`, mutate `GameState`, or create PlayTable UI.

## Behavior

- `IMultiplayerTransport` exposes:
  - `PendingAutoAbilityQueueReceived`
  - `SendPendingAutoAbilityQueue`
- `PhotonRealtimeTransportAdapter` validates payloads before sending.
- Photon event code `9` dispatches decoded pending queue payloads.
- Test-only receive helper can dispatch a wrapped pending queue payload.

## Boundary

The hook must not:

- mutate `GameState`
- commit pending ability queues into gameplay state
- resolve ability text or costs
- add session storage
- add PlayTable UI
- publish automatically from live gameplay

## Acceptance Tests

- fake transport can send and emit pending ability queue payloads
- Photon adapter dispatches decoded pending ability queue payloads
- Photon adapter rejects null pending ability queue payloads
- transport hook does not mutate `GameState`

## Future Extensions

- session storage for received pending ability prompts
- PlayTable pending ability prompt surface
- explicit RulesCore command to commit pending queue updates
- owner-private payload delivery rules
