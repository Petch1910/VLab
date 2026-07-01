# Windows Replay Local File Import Spec

Milestone: `M30-03`

## Purpose

Make the Replay screen's `Choose File` workflow useful without adding a native
OS file picker yet.

## Minimum Slice

- Add a path input field on the Replay screen.
- Add a `Load Path` action that reads a local replay JSON file path entered by
  the user.
- Validate the JSON with `GameReplay.FromJson`.
- Show accepted replay id and event count, or a player-facing error.
- Failed import must not mutate live `GameState`.

## Boundaries

- No native file picker dependency.
- No replay protocol change.
- No replay event model change.
- No network replay change.
- No Android/mobile/release work.

## Verification Plan

- Formatter/helper tests for loaded/error replay status.
- Runtime UI test for invalid path no-mutation and valid JSON status if a
  temporary replay file is available.
- Unity compile and EditMode tests.

## Closeout Result

M30-03 is complete.

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_03_replay_local_file.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_03_replay_local_file.xml`
  passed `1157/1157`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_03_replay_local_file.log`
  reported `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_03_replay_local_file.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_03_replay_local_file.json`
  reported `blockers=[]`.
