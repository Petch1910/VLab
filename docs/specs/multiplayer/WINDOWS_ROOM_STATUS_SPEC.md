# Windows Room Status Spec

Milestone: `M25-03`

## Purpose

Give the Windows Online Room screen a compact status block that answers the
questions players need before starting a friend room:

- connection status
- player count
- deck hash readiness
- pack hash match status
- public/event cursor position

## Scope

This is a display and validation-surface milestone only. It must not change
Photon transport, event codes, hidden-state protocol, deck privacy policy, or
room lifecycle semantics.

## Required Fields

The room status block must include:

```text
Room Status:
Connection: ConnectedToLobby
Players: 2/2 connected
Deck hash: 2/2 ready
Pack hash: match
Public cursor: 0
```

For player rows:

- show display name, ready state, deck-hash readiness, and event cursor
- do not show `deck_code`
- do not show revealed deck code

## Cursor Boundary

In the current trusted-client shared-deck friend room, the room-level public
cursor is represented by the highest player event cursor known in room state.
Owner-private public-event stream cursors remain a later gameplay/replay
surface concern.

## Verification

- Formatter tests cover no-room, hash mismatch, player/deck hash status, cursor
  display, and no deck-code leak.
- Runtime lobby UI uses the room status block instead of raw debug payloads.
- Windows-only verification: Unity compile, EditMode, Windows build, and player
  smoke.
