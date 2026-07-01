# M25-02 Lobby Flow Closeout

## Status

Done.

## Scope Completed

- Added `docs/specs/multiplayer/WINDOWS_LOBBY_FLOW_SPEC.md`.
- Wired lobby ready/unready/start/rematch through `MultiplayerLobbyController`.
- Kept lifecycle transitions behind existing `RoomLifecycleController`.
- Added player-facing flow summary text for connect, room, ready, start, and
  rematch states.
- Added Ready, Not Ready, and Rematch buttons to the Windows Online Room UI.
- Changed Start Table so it passes the room start gate before creating the
  online game session.
- Kept Back Home as the explicit exit path to Home.
- Preserved Photon Realtime trusted-client room mode and did not change
  transport protocol, hidden-state protocol, or payload event codes.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_02_lobby_flow.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_02_lobby_flow.xml`
  passed `1061/1061`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_02_lobby_flow.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_02_lobby_flow.json`
  passed with `blockers=[]`.

## Next Target

`M25-03`: Room status: connection, player count, deck hash, pack hash, public
cursor.
