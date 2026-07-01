# Room Lifecycle Spec

## Status

Implemented through `M13-08` as a pure room lifecycle helper. UI/transport
wiring remains later work.

## Purpose

Online rooms need explicit, testable lifecycle transitions for ready, start,
end, and rematch flows.

## States

`RoomLifecycleStates`:

- `waiting`
- `ready`
- `playing`
- `ended`

## Runtime Model

`RoomLifecycleController` returns a cloned `MultiplayerRoomState` on accepted
transitions and never mutates the source room.

Supported transitions:

- `SetPlayerReady(room, playerId, ready)`
- `Start(room)`
- `End(room)`
- `Rematch(room)`

Rules:

- `ready` requires at least two connected ready players.
- `start` requires at least two connected ready players and not already
  playing/ended.
- `end` requires `playing`.
- `rematch` requires `ended`, clears player ready flags, and resets player
  event cursors.

## Verification

EditMode tests cover:

- ready transitions use clones and preserve source room
- start rejects until all connected players are ready
- start/end/rematch happy path
- invalid transition rejection without source mutation
