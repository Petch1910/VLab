# M29-06 Online Deck Readiness Guard Closeout

Date: 2026-06-28

## Scope

M29-06 closes the remaining player-facing Online Room gap found by the M29-05
audit: Host, Join, and Reconnect should not create room/join/reconnect state
when the active lobby deck is obviously unready.

## Implemented

- Added `MultiplayerLobbyDeckReadiness` as a pure local readiness helper.
- Host, Join, and Reconnect now check the active lobby deck before room action
  state is created.
- The guard rejects:
  - missing deck
  - main deck count not equal to `50`
  - ride deck count over `4`
  - G deck count over `16`
- Rejected actions show a player-facing count problem in the lobby.
- Rejected actions do not create Photon room state, join payloads, reconnect
  requests, or `MultiplayerRoomState`.

## Non-Goals Preserved

- No repository-backed card-id validation in this slice.
- No copy-limit validation in this slice.
- No Photon payload/event-code changes.
- No deck privacy policy changes.
- No `GameState` mutation.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m29_06_online_deck_readiness_guard.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m29_06_online_deck_readiness_guard.xml`
  - `1149/1149` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m29_06_online_deck_readiness_guard.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m29_06_online_deck_readiness_guard.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m29_06_online_deck_readiness_guard.json`
  - `blockers=[]`

## Result

M29 Online Room reopen pass is closed for the current Windows-first scope. The
next active target should return to the deferred local PlayTable work:
`M28-10` Match Log / Preview density review.
