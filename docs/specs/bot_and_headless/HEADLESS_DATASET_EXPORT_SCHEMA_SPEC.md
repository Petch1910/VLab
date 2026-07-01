# Headless Dataset Export Schema Spec

## Milestone

`M17-05`

## Goal

Define a compact dataset export schema for headless batch outputs. This schema
is intentionally summary-only and does not introduce RL observations, action
masks, rewards, or full replay data yet.

## Schema

`HeadlessDatasetExport`

- `schema_version`
- `dataset_id`
- `source`
- `source_policy`
- `run_count`
- `accepted_count`
- `blocked_count`
- `runs`

`HeadlessDatasetRunRecord`

- `index`
- `accepted`
- `failure_reason`
- `seed`
- `ruleset`
- `deck_source`
- `actions_executed`
- `event_count`
- `final_phase`
- `player0_deck_count`
- `player0_hand_count`
- `player0_rear_guard_count`
- `player0_protect_markers`

## Hidden-State Policy

The dataset export uses:

```text
summary_metrics_no_card_instance_ids
```

It must not include card instance ids, hand contents, deck order, or true hidden
card identities.

## Non-Goals

- No observation/action/reward API. That is `M17-06`.
- No full replay data. M17-03 owns redacted replay traces.
- No file writer or batch CLI integration yet.
- No distributed worker or RL training.

## Verification

- Exporting a deterministic two-run batch produces stable summary records.
- Export JSON does not contain `card_instance_id`, `card_instance_ids`, or
  `card_id` fields.
- Null batch input creates an empty dataset shell.
