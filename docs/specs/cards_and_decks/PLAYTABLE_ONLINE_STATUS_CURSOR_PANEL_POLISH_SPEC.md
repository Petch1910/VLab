# PlayTable Online Status / Cursor Panel Polish Spec

## Scope

`M16-05` polishes the PlayTable online mode/status summary. It keeps the same
inputs and session behavior but formats online state as explicit key/value
segments so cursor and reconnect information are easier to scan.

This is UI/readability work only. It does not change multiplayer session
state, event cursor logic, reconnect batch application, or `GameState`.

## Behavior

`PlayTableModeSummaryFormatter.Format()` now outputs online state as:

```text
Online | Status: <status> | Transport: <transport> | Cursor: <event_cursor> | Trigger logs: <count>
```

When reconnect replay applies events, it appends:

```text
| Reconnect: +<applied> from <from_event_index>
```

Null or blank status/transport values display as `unknown`.

Local mode remains:

```text
Local
```

## Boundaries

M16-05 does not:

- mutate `GameState`
- advance event cursors
- apply reconnect batches
- send or decode network payloads
- change trigger log counts

## Verification

EditMode tests cover:

- unchanged local mode label
- online key/value status, transport, cursor, and trigger log count
- reconnect suffix only when applied count is positive
- enum status formatting
- null status/transport fallback to `unknown`
