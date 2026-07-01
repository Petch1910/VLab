# Pending Auto Ability Queue Commit Event Spec

Status: Implemented in M10-106.

## Purpose

Represent an accepted pending AUTO queue commit as replay-ready metadata without
yet appending it to `GameState.event_log`.

This event model is the handoff from the pure queue commit helper to the future
replay/event-sourcing integration.

## Event Model

`PendingAutoAbilityQueueCommitEvent` fields:

- `event_id`
- `event_type`
- `queue_id`
- `pending_id`
- `decision_id`
- `decision_type`
- `player_index`
- `before_queue_hash`
- `after_queue_hash`
- `manual_resolution_recorded`
- `summary`

`event_type` is `PendingAutoAbilityQueueCommitted`.

## Builder

```csharp
PendingAutoAbilityQueueCommitEventBuildResult PendingAutoAbilityQueueCommitEventBuilder.Build(
    PendingAutoAbilityQueueCommitResult commitResult)
```

Accepted commit results produce an event. Missing or rejected commit results do
not produce an event.

## Hidden-State Safety

The event model intentionally excludes:

- source card id,
- source card instance id,
- ability summary source context,
- queue card payloads.

The commit helper creates a safe decision id from decision type, selected index,
pending id, and reason hash, rather than trusting a raw inbound decision id that
could contain source identity.

## Current Boundary

M10-106 creates only a pure event/log model. It does not:

- append to `GameState.event_log`,
- mutate `GameState`,
- mutate the committed queue,
- publish network payloads,
- replay or apply event state.

## Verification

EditMode coverage verifies:

- accepted commit result builds replay metadata,
- event JSON round-trip,
- manual `Resolve` flag preservation,
- missing/rejected commit result rejection,
- no source id/source instance id in event JSON,
- no queue/commit result mutation.
