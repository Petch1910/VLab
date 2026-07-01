# Headless CLI Replay/Result Output Spec

## Milestone

`M17-03`

## Goal

Expand the headless CLI output from a single minimal result JSON into explicit
result and replay artifacts while keeping hidden-state policy visible.

## Output Artifacts

- Result JSON:
  - accepted/rejected status
  - seed
  - ruleset
  - deck source
  - action count
  - event count
  - final phase
  - basic player zone counters
  - result/replay artifact paths
- Replay JSON:
  - schema version
  - hidden-state policy
  - accepted/rejected status
  - seed/ruleset/deck source
  - event count
  - redacted event records

## Hidden-State Policy

M17-03 replay output is a local headless trace, not a public network payload.
It still redacts `card_instance_id` from replay records and records:

```text
local_headless_trace_card_instance_ids_redacted
```

Full true-state replay export remains future work and must be separately gated.

## CLI Argument

- `-headlessReplayPath <path>`
  - Optional.
  - If omitted, the CLI writes `headless_replay.json` next to the result JSON.

## Non-Goals

- No full reducer-replay artifact yet.
- No public multiplayer replay payload change.
- No dataset export. That starts at `M17-05`.
- No batch self-play. That starts at `M17-04`.

## Verification

- `RunWithReplay()` creates an accepted result and replay artifact.
- Replay artifact has four deterministic redacted event records for the default
  script.
- CLI parser accepts `-headlessReplayPath`.
- Unity compile and full EditMode suite pass.
