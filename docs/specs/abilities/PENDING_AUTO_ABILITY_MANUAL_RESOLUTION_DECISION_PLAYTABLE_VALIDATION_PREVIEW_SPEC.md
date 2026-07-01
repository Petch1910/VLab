# Pending Auto Ability Manual Resolution Decision PlayTable Validation Preview Spec

## Status

Implemented in `M10-92`.

## Purpose

Show a read-only PlayTable preview of whether the latest pending AUTO ability
manual resolution decision payload is structurally valid.

This milestone only renders validation status. It does not publish, apply, or
resolve decisions.

## Behavior

- PlayTable side panel includes a manual resolution decision validation preview.
- Local PlayTable and online sessions without decision payloads show the
  validation fallback message.
- Online PlayTable validates the latest stored manual decision payload with
  `PendingAutoAbilityManualResolutionDecisionValidator`.
- Preview text is formatted with
  `PendingAutoAbilityManualResolutionDecisionValidationResultFormatter`.
- Hidden-source decisions remain redacted.

## Boundary

This milestone must not:

- publish payloads
- apply decisions to pending queues
- resolve, skip, or defer abilities
- mutate `GameState`
- reveal hidden source card identity

## Acceptance Tests

- local PlayTable shows validation fallback
- online PlayTable shows valid latest payload preview
- invalid latest payload preview shows rejection reason
- hidden-source validation preview does not leak source identity
- preview rendering does not mutate `GameState`
