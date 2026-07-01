# Pending Auto Ability Manual Resolved Event Spec

## Status

Implemented in `M10-111`.

## Purpose

Record a committed manual `Resolve` decision for an unsupported pending AUTO
ability as replay/debug metadata.

This does not execute structured card effects. It marks that the unsupported
ability was resolved manually and preserves enough metadata for future replay,
audit, and closeout checks.

## Inputs

`PendingAutoAbilityManualResolvedEventBuilder.Build` accepts:

- accepted `PendingAutoAbilityQueueCommitResult`
- `PendingAutoAbilityManualResolutionDecision`

The builder only accepts decisions with:

- `decision_type = Resolve`
- `commitResult.manual_resolution_recorded = true`
- matching pending id between commit result and decision

## Event Model

`PendingAutoAbilityManualResolvedEvent` fields:

- `event_id`
- `event_type`
- `queue_id`
- `pending_id`
- `decision_id`
- `decision_type`
- `player_index`
- `timing_event`
- `before_queue_hash`
- `after_queue_hash`
- `manual_resolution_reason`
- `manual_resolution_reason_hash`
- `hides_source_card_identity`
- `summary`

`event_type` is `PendingAutoAbilityManualResolved`.

## Hidden-State Safety

When a decision hides source card identity, the builder:

- replaces `pending_id` with a deterministic hidden pending id
- replaces `decision_id` with a deterministic hidden-safe decision id
- replaces the visible manual reason with `<hidden>`
- stores only a hash of the raw reason
- excludes source card id and source card instance id from the event model

The event JSON must not contain raw source card ids, raw source card instance
ids, or hidden reason text.

## Boundary

The builder must not:

- mutate the source queue
- mutate the decision
- mutate the commit result
- mutate `GameState`
- append to `GameState.event_log`
- publish network payloads
- resolve card effects
- pay costs or select targets

## Acceptance Tests

- accepted manual `Resolve` commit builds a resolved event
- event JSON round-trips
- skipped/deferred/rejected commit paths do not build resolved events
- missing/mismatched inputs reject
- hidden-source events do not leak source ids or hidden reason text
- source queue, decision, and commit result are not mutated

## Next Work

`M10-112` closes the ability/trigger automation milestone and updates the next
target to `M11-01` TimingWindow audit.
