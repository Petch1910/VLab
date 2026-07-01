# M29-02 Photon Lobby Reconnect Flow Polish Closeout

## Status

Done.

## Scope

This slice reviews and polishes the player-facing reconnect area in the Photon
lobby after `M29-01` navigation lockout. It does not change Photon event codes,
reconnect payload schemas, replay semantics, deck privacy policy, or
`GameState`.

## Changes

- Expanded the Online Room panel height within the Windows 720p reference
  layout.
- Increased the reconnect summary area from a short status strip to a readable
  block.
- Added a stable runtime object name: `Reconnect Summary Text`.
- Clarified no-batch guidance:
  - enter room id + cursor, then Reconnect to resume
  - use cursor `0` for a fresh table
- Clarified request-only state with a waiting-for-peer batch message.
- Kept existing room mismatch, cursor gap, and Start Table handoff messaging.

## Tests

- `MultiplayerLobbyStatusFormatterTests.ReconnectSummaryGuidesFreshAndResumePaths`
- `MultiplayerLobbyStatusFormatterTests.ReconnectSummaryShowsWaitingForPeerWhenRequestHasNoBatch`
- `MultiplayerLobbyTests.MultiplayerLobbyBootstrapCreatesRuntimeUiWithInjectedTransport`
  verifies the `Reconnect Summary Text` surface and minimum preferred height.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m29_02_reconnect_flow_polish.log`
  exited `0` with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m29_02_reconnect_flow_polish.xml`
  passed `1142/1142`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m29_02_reconnect_flow_polish.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m29_02_reconnect_flow_polish.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m29_02_reconnect_flow_polish.json`
  passed with `blockers=[]`.

## Next Target

`M29-03`: Photon lobby Quick Deck Selector / Quick Edit planning and first
implementation slice. It should let a player adjust the active lobby deck before
hosting/joining, without changing deck privacy policy or mutating room state
after a room is active.
