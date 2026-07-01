# M29-01 Photon Lobby Navigation Lockout Closeout

## Status

Done.

## Scope

This slice reopens the Windows Online Room surface for player-facing Photon
lobby polish. It keeps the existing Photon trusted-client transport, payloads,
deck privacy policy, reconnect protocol, and `GameState` untouched.

## Changes

- Added a dedicated `Leave Room` button to `MultiplayerLobbyBootstrap`.
- Stored the header `Back Home` button and disables it while a Photon room is
  active.
- Added a defensive `BackHome()` guard so direct invocation cannot leave a room
  accidentally.
- Added a compact navigation guidance line:
  `Back Home is locked while this Photon room is active. Use Leave Room first.`
- Added `MultiplayerLobbyStatusFormatter.FormatNavigationLockout(...)`.

## Tests

- `MultiplayerLobbyStatusFormatterTests.NavigationLockoutExplainsLeaveRoomWithoutDeckCode`
- `MultiplayerLobbyTests.MultiplayerLobbyBootstrapCreatesRuntimeUiWithInjectedTransport`
- `MultiplayerLobbyTests.MultiplayerLobbyLocksBackHomeWhileRoomIsActive`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m29_01_lobby_nav_lockout.log`
  exited `0` with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m29_01_lobby_nav_lockout.xml`
  passed `1140/1140`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m29_01_lobby_nav_lockout.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m29_01_lobby_nav_lockout.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m29_01_lobby_nav_lockout.json`
  passed with `blockers=[]`.

## Next Target

`M29-02`: Photon lobby reconnect flow polish review. Verify reconnect request,
batch, cursor, room mismatch, and Start Table handoff messaging against the
current runtime UI after the navigation lockout change.
