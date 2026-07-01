# Windows Replay Viewer Launch Spec

Milestone: `M30-04`

## Purpose

Let a loaded local replay open into a player-facing replay viewer instead of
stopping at validation.

## Minimum Slice

- Keep `GameReplay` and `GameReplayPlayer` semantics unchanged.
- Add a replay launch action after a replay JSON path is accepted.
- Show replay state through an existing PlayTable-compatible surface or a
  minimal read-only replay viewer.
- Provide Start / Step / End status if practical in the same slice.
- Failed launch must not mutate live `GameState`.

## Boundaries

- No replay event protocol change.
- No network replay change.
- No online spectator rewrite.
- No Android/mobile/release work.

## Verification Plan

- Unit/UI tests that a loaded replay can launch or preview without direct live
  `GameState` mutation.
- Unity compile and EditMode tests.
- Client smoke and Windows player smoke after runtime UI changes.

## Closeout Result

M30-04 is complete.

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_04_replay_viewer_launch.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_04_replay_viewer_launch.xml`
  passed `1158/1158`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_04_replay_viewer_launch.log`
  reported `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_04_replay_viewer_launch.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_04_replay_viewer_launch.json`
  reported `blockers=[]`.
