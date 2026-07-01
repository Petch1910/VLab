# Pending Auto Ability Resolution Request Spec

## Status

Implemented in `M10-62`.

## Purpose

Add a pure request model for a future "resolve selected pending AUTO ability"
command path.

This only captures intent and selected metadata. It does not execute ability
effects, pay costs, mutate `GameState`, or send network events.

## Behavior

- Request creation requires an accepted selection with a selected ability.
- Missing, cleared, rejected, or null selections reject deterministically as
  `PENDING_AUTO_ABILITY_SELECTION_MISSING`.
- Valid selections create a serializable request containing selected index,
  pending id, player index, timing event, source visibility fields, and summary.
- Hidden selections preserve `hides_source_card_identity` and redact source
  instance id.
- Request creation does not mutate selection state or selected ability data.

## Boundary

This milestone must not:

- resolve pending abilities
- pay costs
- mutate `GameState`
- send request payloads over transport
- add PlayTable resolve buttons

## Acceptance Tests

- no-selection request is rejected deterministically
- rejected selection is rejected deterministically
- visible selection creates a JSON round-trippable request
- hidden selection preserves visibility without leaking source instance id
- request creation does not mutate selected ability data

## Future Extensions

- PlayTable resolve-selected button
- resolution validator against live queue state
- RulesCore command factory for pending AUTO resolution
