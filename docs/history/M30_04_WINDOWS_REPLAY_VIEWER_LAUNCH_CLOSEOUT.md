# M30-04 Windows Replay Viewer Launch Closeout

Date: 2026-06-28

## Scope

Let a loaded local replay become viewable through a minimal read-only replay
preview on the Replay screen.

## Implemented

- Replay screen now keeps a `GameReplayPlayer` after a replay JSON path loads.
- Added `Replay Preview Text`.
- Added controls:
  - `Start Preview`
  - `Step Replay`
  - `End Replay`
- Preview status shows cursor, event count, turn number, phase, and end/ready
  state.
- Loading a bad path clears the previous replay/player preview so stale replay
  state is not shown.

## Preserved Boundaries

- No replay event model changes.
- No replay protocol changes.
- No network replay changes.
- No online spectator rewrite.
- No Android/mobile/release work.
- No live `GameState` mutation.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_04_replay_viewer_launch.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_04_replay_viewer_launch.xml`
  - `1158/1158` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_04_replay_viewer_launch.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_04_replay_viewer_launch.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_04_replay_viewer_launch.json`
  - `blockers=[]`

## Result

Replay can now be loaded and stepped from the Home Replay screen. The next
replay slice should let local PlayTable export a replay JSON so the viewer has
a player-facing source file.
