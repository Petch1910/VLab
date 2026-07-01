# Pending Auto Ability Manual Resolution Decision PlayTable Summary Spec

## Status

Implemented in `M10-82`.

## Purpose

Show a read-only PlayTable summary for the latest pending AUTO ability manual
resolution decision payload.

This milestone only renders diagnostic/status text. It does not publish,
create, or apply decisions.

## Behavior

- Local PlayTable shows the zero-summary message.
- Online PlayTable shows the latest stored decision summary.
- Summary text uses
  `PendingAutoAbilityManualResolutionDecisionSummaryFormatter`.
- Hidden source decisions remain redacted.
- Rendering does not mutate `GameState`.

## Boundary

This milestone must not:

- resolve, skip, or defer pending abilities
- create new decision payloads
- add list surfaces
- mutate `GameState`

## Acceptance Tests

- local PlayTable shows no decision payloads
- online PlayTable shows latest received decision summary
- hidden-source summary does not leak source ids
- rendering does not mutate `GameState`
