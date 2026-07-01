# Pending Auto Ability Selection State Spec

## Status

Implemented in `M10-57`.

## Purpose

Add a pure state helper for selecting one pending AUTO ability from a decoded
queue. This prepares future prompt controls while keeping ability resolution out
of scope.

## Behavior

- Empty or null queues reject selection as
  `PENDING_AUTO_ABILITY_SELECTION_EMPTY`.
- Negative, too-high, or null-item indexes reject selection as
  `PENDING_AUTO_ABILITY_SELECTION_INDEX_OUT_OF_RANGE`.
- Valid selection returns an accepted state with selected index and a copied
  pending ability.
- Clearing returns an accepted no-selection state.
- Selection does not mutate the source queue.

## Boundary

This milestone must not:

- add PlayTable controls
- resolve pending abilities
- pay costs
- mutate `GameState`
- mutate the source queue

## Acceptance Tests

- empty/null queues reject deterministically
- valid index selects copied pending ability metadata
- out-of-range and null-item indexes reject deterministically
- clear returns no-selection state
- source queue is unchanged after selection

## Future Extensions

- PlayTable cycle/select controls
- selection status formatter
- ability resolution command factory
