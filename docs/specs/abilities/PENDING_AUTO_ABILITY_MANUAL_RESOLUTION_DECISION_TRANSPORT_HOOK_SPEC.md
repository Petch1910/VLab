# Pending Auto Ability Manual Resolution Decision Transport Hook Spec

## Status

Implemented in `M10-77`.

## Purpose

Add transport send/receive hooks for pending AUTO ability manual resolution
decision payloads.

This milestone only moves an already-built decision payload through the
transport interface and Photon adapter decode/dispatch path. It does not store
received decisions in the game session, add PlayTable controls, or resolve
abilities.

## Behavior

- `IMultiplayerTransport` exposes a received event for
  `NetworkPendingAutoAbilityManualResolutionDecisionPayload`.
- `IMultiplayerTransport` exposes a send method for the same payload type.
- `PhotonRealtimeTransportAdapter` validates null sends before delegating.
- `PhotonRealtimeTransportAdapter` decodes Photon event code `11` payloads and
  raises the typed event.
- Invalid wrapped payloads are rejected without dispatching a typed event.

## Boundary

This milestone must not:

- mutate `GameState`
- add session storage
- add PlayTable decision buttons
- resolve or skip pending abilities
- inspect or alter ability text

## Acceptance Tests

- fake transport can send and emit a decision payload
- Photon adapter dispatches a decoded decision payload
- Photon adapter rejects null decision payload sends
- transport hook does not mutate `GameState`
