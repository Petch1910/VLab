# Pending Auto Ability Manual Resolution Decision Validation Spec

## Status

Implemented in `M10-90`.

## Purpose

Validate pending AUTO ability manual resolution decision payloads before any
future apply step.

This milestone only validates and returns a sanitized decoded decision. It does
not apply the decision, resolve abilities, publish payloads, or mutate
`GameState`.

## Behavior

- Decode through `PendingAutoAbilityManualResolutionDecisionPayloadCodec`.
- Reject invalid payload/decode failures with the codec rejection reason.
- Reject unsupported decision types with
  `PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID`.
- Reject missing pending ids with
  `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PENDING_ID_MISSING`.
- Return a sanitized clone of the decoded decision.
- Redact hidden source identity in the returned decision.

## Boundary

This milestone must not:

- mutate the source payload
- publish payloads
- apply decisions to pending queues
- mutate `GameState`
- reveal hidden source card identity in validation output

## Acceptance Tests

- valid payload passes and returns a decoded decision
- invalid payload is rejected
- unsupported decision type is rejected
- missing pending id is rejected
- hidden-source decision output is redacted
- validation does not mutate the source payload
