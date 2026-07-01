# Photon Lobby Reconnect Flow Polish Spec

## Status

Implemented as a post-M19 online experience slice after the lobby UI polish.

`M29-02` follow-up is complete. It verified and polished the reconnect
request/batch/cursor handoff UI after `M29-01` navigation lockout without
changing Photon event codes or replay semantics.

## Purpose

Reconnect already had request and batch payloads. This slice tightens lobby
state handling so old reconnect data cannot silently survive across room
changes, and the player-facing lobby explains whether a pending reconnect batch
can be handed into the online PlayTable.

## Runtime Rules

- `MultiplayerLobbyController.JoinRoom(...)` clears the current room and all
  pending reconnect state before joining the requested room.
- `CreateRoom(...)`, `JoinRoom(...)`, and `Disconnect()` clear
  `LastReconnectRequest`, `LastReconnectBatch`, and the internal sent-request
  flag.
- When a room state with a different room id is received, reconnect transient
  state is cleared before any new reconnect request may be sent.
- `HandleReconnectBatchReceived(...)` ignores batches received before a room
  state exists.
- `HandleReconnectBatchReceived(...)` ignores batches whose `room_id` does not
  match the current room.

## UI Rules

`MultiplayerLobbyStatusFormatter.FormatReconnectSummary(...)` shows:

- no pending request/batch state
- fresh-vs-resume guidance for room id, cursor, and cursor `0`
- the latest reconnect request player and cursor
- waiting-for-peer text when a reconnect request exists but no batch has arrived
- the latest batch start cursor and event count
- handoff status:
  - batch from event `0` is ready to apply when `Start Table` opens
  - batch from a later event needs matching local state at that cursor before
    `Start Table`

## Non-Goals

- no Photon event code changes
- no saved local state resume implementation
- no ranked reconnect integrity guarantee
- no `GameState` mutation from lobby UI

## Verification

- `MultiplayerLobbyTests.LobbyControllerClearsStaleReconnectStateWhenJoiningOrDisconnecting`
  verifies stale reconnect request/batch data is cleared and pre-room batches
  are ignored.
- `MultiplayerLobbyStatusFormatterTests` verifies reconnect handoff text for
  event `0` and non-zero cursor batches.
- `M29-02` adds tests for fresh/resume guidance, request-only waiting text, and
  the runtime `Reconnect Summary Text` height.
- Run Unity compile and EditMode after changing reconnect lobby behavior.

`M29-02` verification included Unity compile, EditMode `1142/1142`, editor
client smoke `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows
player smoke `blockers=[]`.
