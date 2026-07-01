# M30-02 Windows Replay Entry / Browser Closeout

Date: 2026-06-28

## Scope

Unlock the Home Replay route from a locked placeholder into a player-facing
Replay screen.

## Implemented

- Added `HomeReplayPanelFormatter` for replay empty-state copy.
- Replaced Home `Replay` button behavior:
  - old: `ShowReplayLocked()` with scheduled placeholder text
  - new: opens `Replay Screen`
- Added `Replay Summary Text` with clear empty-library guidance.
- Added `Choose File` placeholder action for the next replay slice.
- Added `Close` action to return to Home without mutating live `GameState`.

## Preserved Boundaries

- No replay event model changes.
- No replay protocol changes.
- No file auto-download.
- No Android/mobile/release work.
- No live `GameState` mutation.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_02_replay_entry.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_02_replay_entry.xml`
  - `1153/1153` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_02_replay_entry.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_02_replay_entry.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_02_replay_entry.json`
  - `blockers=[]`

## Result

Replay is no longer locked from Home. The next replay slice should make
`Choose File` accept a local replay JSON path and validate it without changing
replay semantics.
