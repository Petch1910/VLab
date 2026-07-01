# Trigger Check Commit Event Spec

Status: Implemented in M10-107.

## Purpose

Represent a trigger check outcome as replay-ready metadata without yet applying
card movement, trigger allocation, or modifier ledger changes to `GameState`.

This is the committed-event model for manual trigger check draft/log data.

## Event Model

`TriggerCheckCommitEvent` fields:

- `event_id`
- `event_type`
- `source_log_entry_id`
- `check_source`
- `player_index`
- `check_index`
- `checked_card_instance_id`
- `checked_card_id`
- `trigger_type`
- `hides_checked_card_identity`
- `accepted`
- `needs_manual_resolution`
- `rejection_reason`
- `modifier_count`
- `modifier_ids`
- `rng_outcome_id`
- `summary`

`event_type` is `TriggerCheckCommitted`.

## Builder

```csharp
TriggerCheckCommitEventBuildResult TriggerCheckCommitEventBuilder.Build(
    TriggerCheckLogEntry entry)
```

The builder accepts trigger check log entries, including masked entries. It does
not generate random outcomes itself. It records the outcome already present in
the log entry.

## Current Boundary

M10-107 creates only a pure event/log model. It does not:

- move checked cards between zones,
- allocate trigger power/critical,
- mutate the combat modifier ledger,
- append to `GameState.event_log`,
- mutate `GameState`,
- publish network payloads.

M10-108 starts trigger allocation commit into the modifier ledger.

## Hidden-State Safety

The event preserves masking from the input log entry. If the input entry is
masked, the committed event keeps hidden checked-card ids and hidden modifier
ids.

## Verification

EditMode coverage verifies:

- accepted event metadata,
- JSON round-trip,
- masked identity preservation,
- missing/invalid metadata rejection,
- source log entry no-mutation.
