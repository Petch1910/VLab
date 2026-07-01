# Pending Auto Ability Queue Spec

## Status

Implemented in `M10-43`.

## Purpose

Add a minimal pending auto ability queue model for future ability automation.

This is only a deterministic state container and queue helper. It must not
resolve card effects yet.

## Model

`PendingAutoAbility`

- `pending_id`
- `source_card_instance_id`
- `source_card_id`
- `player_index`
- `timing_event`
- `summary`

`PendingAutoAbilityQueue`

- `queue_id`
- ordered `pending` list

## Behavior

- `EnsureLists` initializes null lists.
- `Enqueue` appends an ability to the end of a cloned queue.
- `Peek` returns the first pending ability without removing it.
- `Dequeue` removes and returns the first pending ability from a cloned queue.
- `Clear` returns an empty cloned queue with the same queue id.
- JSON round-trip preserves queue and pending ability fields.

## Boundary

The queue must not:

- inspect Unity UI objects
- execute ability effects
- pay ability costs
- move cards between zones
- inspect deck order
- send multiplayer payloads
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- enqueue preserves ordering and does not mutate source queue
- peek returns first pending ability without removing it
- dequeue returns first pending ability and cloned remainder
- clear returns an empty queue
- null pending list is initialized safely
- JSON round-trip preserves queue and pending ability fields
- Unity compile and EditMode tests pass

## Future Extensions

- typed timing events
- source zone and target metadata
- cost/effect preview payloads
- priority ordering when multiple auto abilities trigger simultaneously
