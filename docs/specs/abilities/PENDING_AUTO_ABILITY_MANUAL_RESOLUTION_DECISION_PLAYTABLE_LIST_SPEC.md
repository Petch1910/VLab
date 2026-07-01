# Pending Auto Ability Manual Resolution Decision PlayTable List Spec

## Status

Implemented in `M10-84`.

## Purpose

Show a read-only PlayTable list for stored pending AUTO ability manual
resolution decision payloads.

This milestone only renders diagnostic/status text. It does not publish,
create, or apply decisions.

## Behavior

- Local PlayTable shows the zero-list message.
- Online PlayTable shows a newest-first bounded decision list.
- List text uses `PendingAutoAbilityManualResolutionDecisionListFormatter`.
- Hidden source decisions remain redacted.
- Rendering does not mutate `GameState`.

## Boundary

This milestone must not:

- resolve, skip, or defer pending abilities
- create new decision payloads
- publish payloads
- mutate `GameState`

## Acceptance Tests

- local PlayTable shows no decision payload list
- online PlayTable shows newest-first received decision list
- hidden-source list entries do not leak source ids
- rendering does not mutate `GameState`
