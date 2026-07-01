# M25-05 Online PlayTable Default UI Closeout

## Status

Done.

## Scope Completed

- Added `docs/specs/multiplayer/ONLINE_PLAYTABLE_DEFAULT_UI_SPEC.md`.
- Removed trigger-log and reconnect debug counters from the primary online
  PlayTable toolbar summary.
- Added `PlayTableModeSummaryFormatter.FormatAdvancedDetails()` for debug
  counters inside Advanced.
- Added an `Online Debug` status block inside the Advanced drawer.
- Preserved multiplayer payload publishing, reconnect behavior, pending AUTO
  handling, and RulesCore behavior.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_05_online_playtable_default_ui.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_05_online_playtable_default_ui.xml`
  passed `1067/1067`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_05_online_playtable_default_ui.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_05_online_playtable_default_ui.json`
  passed with `blockers=[]`.

## Next Target

`M25-06`: Player-facing replay sync/status.
