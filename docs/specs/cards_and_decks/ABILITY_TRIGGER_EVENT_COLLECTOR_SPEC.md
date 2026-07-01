# Ability Trigger Event Collector Spec

## Status

Implemented in `M10-45`; still pure after `M10-110`.

## Purpose

Introduce a pure collector that inspects one `GameEvent` and creates pending
AUTO ability queue items from explicit trigger registrations.

This is the pure collection layer for pending AUTO abilities. It does not parse
Thai card text, pay costs, choose targets, resolve effects, mutate gameplay
state, or publish network payloads.

`M10-110` adds a separate GameState adapter path that can commit the returned
queue clone from a real `GameEvent`. The collector itself remains pure.

## Inputs

- `GameEvent`: the already-recorded game event being inspected.
- `AbilityTriggerRegistration`: explicit structured data registered by future
  ability systems.
- optional source `PendingAutoAbilityQueue`: copied before appending new items.

## Timing Mapping

MVP timing keys are deterministic strings derived from `GameActionType`:

- `Draw` -> `OnDraw`
- `MoveCard` -> `OnMoveCard`
- `SetPhase` -> `OnSetPhase`
- `AddGiftMarker` -> `OnAddGiftMarker`

Registrations only match when `timing_event` equals the mapped timing key.

## Output

The collector returns a new `PendingAutoAbilityQueue`.

For every matching registration, it appends one `PendingAutoAbility` containing:

- deterministic `pending_id`
- source card instance id
- source card id
- owning player index
- timing event
- summary

## Boundary

The collector must not:

- mutate `GameState`
- mutate the source queue
- resolve ability text or effects
- pay costs
- consume RNG
- publish network payloads
- mutate live gameplay state directly

## Acceptance Tests

- matching timing events create pending auto abilities
- non-matching timing events create no abilities
- pending ids are deterministic for the same event and registration
- source queues are copied before appending and remain unchanged
- null events or null registrations return a copied/empty queue

## Future Extensions

- event-condition filters beyond timing
- zone/card/player predicates
- simultaneous AUTO ordering prompts
- integration with `GameState.pending_auto_abilities`
- manual resolution UI for pending ability choices
