# Pending Auto Ability Queue Commit Helper Spec

Status: Implemented in M10-105.

## Purpose

Provide a pure helper that turns a validated manual pending AUTO decision into a
new committed queue clone. This is the last pure step before M10-106 adds a
replayable event.

## API

```csharp
PendingAutoAbilityQueueCommitResult PendingAutoAbilityQueueCommitHelper.Commit(
    PendingAutoAbilityQueue queue,
    PendingAutoAbilityManualResolutionDecision decision)
```

## Result Metadata

`PendingAutoAbilityQueueCommitResult` contains:

- `accepted`
- `rejection_reason`
- `queue_id`
- `pending_id`
- `decision_id`
- `decision_type`
- `player_index`
- `before_queue_hash`
- `after_queue_hash`
- `manual_resolution_recorded`
- `queue`

The hash fields are stable FNV-1a hashes over canonical queue JSON clones. They
are metadata for M10-106 event verification and are not security commitments.

## Accepted Decisions

- `Skip`: removes the active pending item.
- `Defer`: moves the active pending item to the back of the queue.
- `Resolve`: removes the active pending item and sets
  `manual_resolution_recorded=true`. It does not execute card text.

## Rejections

- `PENDING_AUTO_ABILITY_COMMIT_QUEUE_MISSING`
- `PENDING_AUTO_ABILITY_COMMIT_DECISION_MISSING`
- `PENDING_AUTO_ABILITY_COMMIT_DECISION_TYPE_INVALID`
- `PENDING_AUTO_ABILITY_COMMIT_PENDING_ID_MISMATCH`

## Safety Rules

- The helper must not mutate the input queue.
- The helper must not mutate `GameState`.
- The helper must not append `GameState.event_log`.
- The helper must not publish network payloads.
- The helper result must not expose source card ids.

## Verification

EditMode coverage verifies:

- accepted `Skip`, `Defer`, and manual `Resolve`,
- missing queue, missing decision, invalid decision type, and pending id
  mismatch rejections,
- input queue no-mutation,
- null pending list no-normalization,
- result metadata needed by M10-106.
