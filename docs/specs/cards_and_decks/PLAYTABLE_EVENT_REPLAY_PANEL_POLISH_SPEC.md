# PlayTable Event / Replay Panel Polish Spec

## Scope

`M16-04` polishes the PlayTable event log panel. It adds a compact event/replay
panel formatter that shows event count, latest event, and bounded recent events
while preserving the existing raw event log formatter and replay/reducer
behavior.

This is UI/readability work only. It does not change event order, replay
serialization, undo, reducers, or `GameState`.

## Behavior

The PlayTable event log surface now uses
`PlayTableEventReplayPanelFormatter`.

The panel summary shows:

- `Event / replay` header
- total event count
- latest event summary
- newest-first bounded recent events

Resource flip panel lines intentionally omit card instance ids. The raw
`PlayTableEventLogFormatter` remains available for deterministic diagnostics.

## Boundaries

M16-04 does not:

- append or remove events
- change event ids
- alter `GameEventReducer`
- alter `GameReplay` or `GameReplayPlayer`
- change undo behavior
- expose hidden/private card instance ids in compact resource lines

## Verification

EditMode tests cover:

- compact empty panel message
- event count/latest/recent newest-first display
- resource flip instance id redaction
- max entry limiting
- no event mutation during formatting
- PlayTable event surface uses the polished panel formatter
