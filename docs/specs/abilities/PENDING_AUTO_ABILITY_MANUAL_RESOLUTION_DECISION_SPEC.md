# Pending Auto Ability Manual Resolution Decision Spec

## Goal

Add a pure, serializable decision model for manually handling a selected
pending auto ability resolution request.

This is still an intent/audit model. It must not execute card effects, mutate
`GameState`, remove queue entries, or decide legality of the underlying card
ability.

## Decision Types

Supported decision types:

- `Resolve`: the player/judge intends to resolve the selected pending ability
  manually or through a later resolver.
- `Skip`: the player/judge intentionally skips the pending ability.
- `Defer`: the player/judge leaves the pending ability unresolved for now.

## Model Contract

`PendingAutoAbilityManualResolutionDecision` stores:

- `decision_id`
- `decision_type`
- `selected_index`
- `pending_id`
- `player_index`
- `timing_event`
- source card metadata with hidden-source safety
- `reason`
- `summary`

The model copies data from `PendingAutoAbilityResolutionRequest`; it must not
hold mutable references to the source request.

## Safety Rules

- Missing request is rejected with
  `PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING`.
- Unsupported decision type is rejected with
  `PENDING_AUTO_ABILITY_DECISION_TYPE_INVALID`.
- Hidden source card identity remains hidden:
  - source card id becomes `<hidden-card>`
  - source card instance id becomes empty
- Reason text is sanitized into a compact single-line audit string.
- Decision creation does not mutate the request or `GameState`.

## Verification

- Unity compile passes.
- EditMode tests cover missing request, invalid type, resolve, skip/defer
  reason text, hidden-source safety, JSON round-trip, request no-mutation, and
  `GameState` no-mutation.
