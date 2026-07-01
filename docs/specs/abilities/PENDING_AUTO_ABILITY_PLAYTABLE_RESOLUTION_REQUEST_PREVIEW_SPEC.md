# Pending Auto Ability PlayTable Resolution Request Preview Spec

## Status

Implemented in `M10-64`.

## Purpose

Show the selected pending AUTO ability resolution request preview on the
PlayTable side panel before adding any resolve button or transport payload.

## Behavior

- Local PlayTable shows the stable `Pending resolve request: none` preview.
- Online PlayTable creates a preview from the current pending AUTO ability
  selection state.
- No online selection formats the deterministic selection-missing rejection.
- Selected visible/hidden requests use
  `PendingAutoAbilityResolutionRequestFormatter`.
- Hidden requests keep source identity redacted.
- Rendering and preview creation do not mutate `GameState` or send transport
  payloads.

## Boundary

This milestone must not:

- resolve pending abilities
- add a resolve button
- send request payloads over transport
- validate costs or targets against live ability rules
- mutate `GameState`

## Acceptance Tests

- local PlayTable exposes a stable no-request preview
- online PlayTable creates the preview surface object
- no-selection online preview reports deterministic rejection
- selection cycling updates request preview index/source text
- clear selection resets the preview to deterministic rejection
- preview rendering does not mutate state or send pending queue payloads

## Future Extensions

- selected-resolution button
- request payload codec
- live queue revalidation before resolve
