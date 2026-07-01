# Pending Auto Ability Manual Resolution Decision Draft Factory Spec

## Status

Implemented in `M10-86`.

## Purpose

Create a network-ready draft payload for a pending AUTO ability manual
resolution decision from an existing resolution request.

This milestone creates only the decision draft. It does not apply the decision,
resolve abilities, mutate `GameState`, or publish through transport.

## Behavior

- Accepts a room id, sender player index, pending resolution request, decision
  type, optional reason, perspective, and viewer player index.
- Supports explicit decision types:
  - `Resolve`
  - `Skip`
  - `Defer`
- Rejects missing requests with
  `PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING`.
- Rejects unsupported decision types with
  `PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID`.
- Encodes accepted decisions with
  `PendingAutoAbilityManualResolutionDecisionPayloadCodec`.
- Preserves hidden-source redaction from the source request.

## Boundary

This milestone must not:

- publish payloads
- auto-resolve, skip, or defer pending abilities
- mutate `GameState`
- mutate the source `PendingAutoAbilityResolutionRequest`
- reveal hidden source card identity in the decision JSON

## Acceptance Tests

- valid `Resolve`, `Skip`, and `Defer` decisions create network payloads
- missing request is rejected
- invalid decision type is rejected
- hidden-source requests remain redacted after payload decode
- source request is not mutated
