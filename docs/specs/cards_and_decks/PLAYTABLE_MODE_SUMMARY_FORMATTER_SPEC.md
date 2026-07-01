# PlayTable Mode Summary Formatter Spec

## Status

Implemented in `M10-39`; polished in `M16-05`.

## Purpose

Extract PlayTable local/online mode summary text construction into a pure UI
formatter.

This keeps the status line deterministic and covered by tests while preserving
the same local, online, trigger-log count, and reconnect sync inputs.

## Inputs

- whether the PlayTable is online
- multiplayer session status
- transport name
- event cursor
- trigger-check replay log count
- last reconnect applied count
- last reconnect source event index

## Output

- local mode: `Local`
- online mode:
  `Online | Status: <status> | Transport: <transport> | Cursor: <event_cursor> | Trigger logs: <count>`
- online mode with reconnect sync:
  `Online | Status: <status> | Transport: <transport> | Cursor: <event_cursor> | Trigger logs: <count> | Reconnect: +<applied> from <from_event_index>`

## Boundary

The formatter must not:

- inspect Unity UI objects
- send multiplayer payloads
- decode trigger-check replay logs
- inspect deck order
- move cards
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- local mode formats as `Local`
- online mode exposes status/transport/event cursor/trigger log count as
  key/value segments
- reconnect sync text is appended only when applied count is positive
- PlayTable uses the formatter without changing status text
- Unity compile and EditMode tests pass

## Future Extensions

- compact mobile status text
- warning badges for disconnected or reconnecting states
- localized mode summary strings
