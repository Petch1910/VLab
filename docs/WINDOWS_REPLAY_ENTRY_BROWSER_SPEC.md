# Windows Replay Entry / Browser Spec

Milestone: `M30-02`

## Purpose

Unlock the Windows Replay route from Home so Replay is no longer a locked
placeholder in the playable loop.

## Minimum Slice

- Replace the Home `Replay` placeholder with a player-facing Replay screen.
- Show replay status and guidance when no local replay file is selected.
- Provide a deterministic sample/local replay entry if available from current
  smoke or generated fixture data.
- Let the player return Home without mutating live `GameState`.

## Boundaries

- No replay event model change.
- No network replay protocol change.
- No file auto-download.
- No public release packaging.
- No Android/mobile work.

## Verification Plan

- Formatter/helper tests for empty replay library/status text.
- Runtime UI test that Home `Replay` opens a Replay screen instead of locked
  placeholder text.
- Unity compile, EditMode, client smoke, Windows build, and Windows player
  smoke after runtime UI changes.

## Closeout Result

M30-02 is complete.

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m30_02_replay_entry.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m30_02_replay_entry.xml`
  passed `1153/1153`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_02_replay_entry.log`
  reported `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m30_02_replay_entry.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_02_replay_entry.json`
  reported `blockers=[]`.
