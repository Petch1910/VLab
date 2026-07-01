# Client Trust UX Spec

## Status

Implemented through `M13-02`.

## Purpose

Commitment-only rooms hide deck codes but are still client-authoritative. The
UI must explicitly tell players that this is casual trusted-client mode, not a
ranked secure server mode.

## Runtime Behavior

- `DeckPrivacyGameplayPolicy.Evaluate(room)` rejects `deck_commitment` rooms
  with `DECK_COMMITMENT_CLIENT_TRUST_POLICY_REQUIRED`.
- The rejection includes `requires_client_trust_warning = true` and a warning
  message explaining the trusted-client assumption and end-of-match reveal
  verification.
- `MultiplayerLobbyController.AcknowledgeClientTrustWarning()` records a
  per-room acknowledgment and refreshes the privacy decision.
- After acknowledgment, normal gameplay remains blocked with
  `OWNER_PRIVATE_GAMEPLAY_PATH_INCOMPLETE` until public-event state
  application/reconnect paths are complete.

## UI Surface

`MultiplayerLobbyBootstrap` shows:

- an `Acknowledge Trust` control
- a read-only trusted-client warning/status label

## Non-Goals

- ranked-grade anti-cheat
- server-held hidden-state validation
- enabling commitment-only normal gameplay in M13-02

## Verification

- `MultiplayerLobbyTests` verifies pre-ack trust-warning rejection.
- `MultiplayerLobbyTests` verifies post-ack gameplay remains blocked by the
  next technical gate.
- Runtime UI smoke verifies the `Acknowledge Trust` button exists.
