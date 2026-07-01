# M21-05a7 Manual Note Surface Closeout

Status: Done

## Scope

- Added a player-facing `Note` action on PlayTable.
- Added session-local manual note storage and a Manual Notes side-panel
  summary.
- Notes capture current phase plus selected card/target card ids and zones.
- Notes are UI/session state only: they do not mutate `GameState`, do not
  append to `GameState.event_log`, and are not published to Photon.
- Added pure formatter tests for empty notes, latest note, list limiting, and
  player-facing placeholders.

## Limits

- This is not a typed note editor yet.
- Notes are intentionally local guidance for manual play and are not replay
  artifacts.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05g_manual_note_surface.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05g_manual_note_surface.xml`
  passed `960/960`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05g_manual_note_surface.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05g_manual_note_surface.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with player-facing battle resolution controls or move to the
M21-05b game setup/deck-to-PlayTable gap fix if the remaining common-action
surface is sufficient for now.
