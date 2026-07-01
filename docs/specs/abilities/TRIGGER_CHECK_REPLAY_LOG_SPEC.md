# Trigger Check Replay Log Spec

## Status

Implemented in `M10-09`.

## Purpose

Store ordered `TriggerCheckLogEntry` records in a serializable replay/debug log
without integrating with the live `GameEvent` reducer yet.

This provides a safe bridge for future trigger-check replay UI, debug panels,
and bot explainability while live gameplay remains manual.

## Inputs

- existing `TriggerCheckReplayLog` or null
- `TriggerCheckLogEntry`
- visible prefix count

## Output

- replay log id
- ordered trigger check log entries
- JSON representation
- cloned visible prefix log

## Boundary

The helper must not:

- mutate `GameState`
- mutate the source log when appending
- expose mutable references from visible-prefix reads
- integrate with `GameReplay` or `NetworkPublicGameReplay` yet
- send network payloads

## Acceptance Tests

- append preserves entry order and stable ids
- replay log JSON round-trips
- visible prefix returns a cloned subset and leaves source unchanged
- repeated append sequences are deterministic
- deriving entries and appending them does not mutate `GameState`

## Future Extensions

- bind trigger check logs to `GameReplay`
- public/private masking policy for trigger check logs
- Photon payload codec for advisory trigger logs
- replay UI panel for trigger check summaries
