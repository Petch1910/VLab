# Photon Lobby Deck Readiness Guard Spec

Milestone: `M29-06`

## Purpose

Prevent the Online Room from creating or joining Photon rooms with an obviously
unready active lobby deck.

This is a lightweight player-facing guard. It does not replace full
repository-backed deck validation from Deck Builder.

## Runtime Contract

Before `Host`, `Join`, or `Reconnect` sends any room action:

- active deck must exist
- main deck count must be exactly `50`
- ride deck count must be at most `4`
- G deck count must be at most `16`

If the guard rejects:

- no Photon room is created
- no Photon join/reconnect player payload is created
- no reconnect request is sent
- `MultiplayerRoomState` remains null/unchanged
- the lobby shows a player-facing message explaining the count problem

## Non-Goals

- no repository-backed card-id validation
- no copy-limit validation
- no Photon payload or transport changes
- no deck privacy policy changes
- no `GameState` mutation

## Verification

- Pure readiness tests accept count-ready decks and reject missing/count-unready
  decks.
- Runtime lobby test invokes Host, Join, and Reconnect with an unready deck and
  verifies the fake transport receives no room/join/reconnect state.
- Run Unity compile, EditMode, editor client smoke, Windows build, and Windows
  player smoke after runtime lobby changes.

## Closeout Result

M29-06 is complete.

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m29_06_online_deck_readiness_guard.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m29_06_online_deck_readiness_guard.xml`
  passed `1149/1149`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m29_06_online_deck_readiness_guard.log`
  reported `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m29_06_online_deck_readiness_guard.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m29_06_online_deck_readiness_guard.json`
  reported `blockers=[]`.
