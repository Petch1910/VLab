# M30-03 Windows Replay Local File Import Closeout

Date: 2026-06-28

## Scope

Make the Replay screen validate a local replay JSON path without adding a
native OS file picker.

## Implemented

- Added a `Replay JSON path` input field on the Replay screen.
- Added `Load Path`.
- Valid paths are read from disk and validated through:
  - `GameReplay.FromJson`
  - `GameReplay.CreateInitialState`
- Loaded status shows replay id, event count, and source path.
- Empty/missing/invalid paths show player-facing errors.
- Failed import does not mutate live `GameState`.

## Preserved Boundaries

- No native file picker dependency.
- No replay protocol changes.
- No replay event model changes.
- No network replay changes.
- No Android/mobile/release work.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_03_replay_local_file.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_03_replay_local_file.xml`
  - `1157/1157` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_03_replay_local_file.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_03_replay_local_file.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_03_replay_local_file.json`
  - `blockers=[]`

## Result

Replay local file validation is available from Home. The next replay slice
should launch or preview the loaded replay through `GameReplayPlayer` without
changing replay semantics.
