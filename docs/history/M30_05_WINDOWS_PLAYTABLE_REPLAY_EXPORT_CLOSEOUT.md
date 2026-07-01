# M30-05 Windows PlayTable Replay Export Closeout

Date: 2026-06-28

## Scope

Let local PlayTable export a replay JSON file that the Home Replay screen can
load and preview.

## Implemented

- Added `PlayTableReplayExporter`.
- Local PlayTable captures a clean initial-state snapshot before gameplay
  actions mutate the live table.
- Added `Export Replay` in the PlayTable Advanced drawer.
- Local export writes to:
  `client/unity/VanguardThaiSim/work/vanguard_latest_replay.json`
- Export success/failure is reported through the player-facing table status
  message.

## Preserved Boundaries

- No replay event model changes.
- No online replay protocol changes.
- No native save dialog dependency.
- No Android/mobile/release work.
- No online/private hidden-state export path.
- No live `GameState` mutation from export.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_05_playtable_replay_export.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_05_playtable_replay_export.xml`
  - `1160/1160` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_05_playtable_replay_export.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_05_playtable_replay_export.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_05_playtable_replay_export.json`
  - `blockers=[]`

## Result

The local Windows replay loop now has both sides: PlayTable can export a local
replay JSON, and the Home Replay screen can load and preview that JSON. The
next slice should audit the complete Windows playable loop before opening new
feature work.
