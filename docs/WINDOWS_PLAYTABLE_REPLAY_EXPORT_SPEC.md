# Windows PlayTable Replay Export Spec

Milestone: `M30-05`

## Purpose

Let local PlayTable create a replay JSON file that can be loaded by the Replay
screen introduced in M30-02 through M30-04.

## Minimum Slice

- Preserve the initial local match state before player actions mutate it.
- Export `GameReplay.Create(initialState, event_log)` for local PlayTable.
- Use a deterministic local path under the Unity project `work/` folder first.
- Show player-facing success/failure status.
- Keep export out of online/private hidden-state paths unless separately
  specified.

## Boundaries

- No replay event model change.
- No online replay protocol change.
- No native save dialog dependency.
- No Android/mobile/release work.

## Verification Plan

- Unit/UI tests for export path, JSON validity, and no live-state mutation.
- Unity compile and EditMode tests.
- Client smoke and Windows player smoke after runtime UI changes.

## Closeout - 2026-06-28

Implemented:

- Added `PlayTableReplayExporter` as a pure local export helper.
- Local PlayTable preserves a replay initial-state snapshot before player
  actions mutate the live table.
- Added `Export Replay` in the Advanced drawer.
- Export writes `client/unity/VanguardThaiSim/work/vanguard_latest_replay.json`
  for local PlayTable only.
- Export status is surfaced through the existing player-facing selection
  message.

Preserved:

- No replay event model change.
- No online replay protocol change.
- No native save dialog dependency.
- No Android/mobile/release work.
- Export tests verify source states are not mutated.

Verification:

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

Next:

- `M30-06` should audit the complete Windows playable loop after the Replay
  export/import/viewer path exists, then decide whether the next target is a
  polish/fix pass or a new feature track.
