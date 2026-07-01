# Windows Lobby Flow Spec

Milestone: `M25-02`

## Purpose

Make the Windows Online Room screen usable as a player-facing friend-room flow:

1. connect to Photon Realtime
2. host or join a room
3. mark each player ready
4. start the shared manual table
5. leave back to Home
6. expose rematch as a lifecycle action only after a room has ended

## Scope

This milestone improves lobby lifecycle UX and controller wiring only. It does
not change Photon event codes, command envelopes, hidden-state protocol, deck
privacy mode, or the selected transport.

## Runtime Contract

- Room state remains the networked source for lobby lifecycle.
- `RoomLifecycleController` is the only place that changes lifecycle state.
- UI actions call `MultiplayerLobbyController`; UI must not mutate
  `MultiplayerRoomState` directly.
- Ready/start/rematch transitions are published through `SendRoomState`.
- `Start Table` must call the room start gate before creating the gameplay
  session.
- `Back Home` destroys the lobby and disconnects unless the lobby is handing the
  transport to PlayTable.
- Status text must not show deck codes or revealed deck codes.

## Player-Facing Flow Text

The lobby should show a compact flow summary:

```text
Flow:
1 Connect: ConnectedToLobby
2 Room: ROOM-ID / players 2/2
3 Ready: you ready / players ready 2/2
4 Start: Start Table available
5 Rematch: available after ended games
```

## Rematch Boundary

M25-02 exposes rematch only as a lobby lifecycle reset when the current room is
already `ended`. It does not implement returning from PlayTable to lobby with a
finished session; that belongs to a later online-room polish task.

## Verification

- Formatter tests cover no-room, ready, and rematch summaries.
- Controller tests cover ready publishing, start rejection before ready,
  successful start publishing, and rematch publishing from ended rooms.
- Runtime UI smoke checks that Ready, Not Ready, Rematch, Start Table, and Back
  Home controls exist.
