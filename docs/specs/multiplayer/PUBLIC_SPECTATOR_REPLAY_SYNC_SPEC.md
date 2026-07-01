# Public Spectator Replay Sync Spec

## Status

Implemented in `M13-09` as public-only replay state stepping and cursor-based
batch sync for spectator/replay consumers.

## Purpose

Commitment/private-deck rooms cannot give spectators or reconnecting clients a
true `GameState` event stream. They need a replay surface that can rebuild a
visible board from:

1. an initial masked public/spectator state view
2. ordered `NetworkPublicGameEvent` records
3. a public event cursor

This keeps hidden hand/deck identities out of spectator and replay surfaces.

## Runtime Model

`NetworkPublicGameReplay` stores:

- a masked `initial_state_json`
- replay perspective metadata
- cloned `NetworkPublicGameEvent` records

`NetworkPublicGameReplayPlayer` now keeps:

- `InitialStateView`: masked start state
- `CurrentStateView`: masked state after applied public events
- `CurrentIndex`: applied public event cursor

`StepForward()` applies the next public event through
`NetworkPublicGameEventApplier.ApplyToPublicView(...)`.

`ApplyBatch(NetworkPublicEventBatch)` is the live spectator sync path. It:

- requires protocol version match
- requires `batch.from_event_index == CurrentIndex`
- requires the player to be at the replay append cursor
- applies all events to a cloned working state first
- commits events/state/cursor only when every event is accepted

## Boundaries

- Never apply true `GameEvent` records in this path.
- Never append public events to `GameState.event_log`.
- Never require true opponent card ids or private instance ids.
- Rejects must preserve replay event list, cursor, and current state.
- Returned visible event logs are cloned so UI cannot mutate replay history.

## Verification

EditMode tests cover:

- `StepForward()` updates spectator state counts and phase from public events
- batch sync appends and applies events from the current public cursor
- cursor mismatch rejects without mutation
- invalid public event rejects without mutation
- source/visible public event clones cannot mutate replay history
- true opponent card ids/instance ids do not leak into replay JSON or current
  spectator state
