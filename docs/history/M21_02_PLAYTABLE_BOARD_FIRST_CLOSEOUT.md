# M21-02 PlayTable Board-First Closeout

## Status

`M21-02` is complete.

## Completed Changes

- Reduced the desktop PlayTable toolbar height in `ResponsiveLayoutProfile`.
- Reduced the desktop PlayTable side panel width from `320` to `300` so the
  board/table area has more room.
- Increased desktop PlayTable field/resource/hand row budgets.
- Renamed the primary table panel to `Board Table` and gave it stronger layout
  weight than the side panel.
- Added board-first layout formatter helpers and desktop board-to-toolbar ratio
  checks.
- Added Windows PlayTable board-first QA verifier coverage.
- Updated client smoke so the active smoke path checks Windows PlayTable
  board-first layout instead of Android reference layout.
- Updated editor smoke build-setting checks to Windows-only.

## Safety

- No RulesCore behavior changed.
- No deck validation changed.
- No network payload contract changed.
- No bot/simulation behavior changed.
- No UI path was added that mutates `GameState` directly.
- No Android build, APK, LDPlayer, ADB/emulator smoke, mobile layout QA, app
  packaging, or release-candidate packaging was run for the final verification.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_02_playtable_board_first_c.log`
  completed with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_02_playtable_board_first_c.xml`
  passed `902/902`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_02_playtable_board_first_b.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_02_playtable_board_first_b.json`
  reports four steps and `blockers=[]`, with final step:
  `Windows layout smoke: PlayTable board-first reference viewports passed.`

## Next Target

`M21-03`: add zone count/status for deck, hand, drop, damage, soul, bind, order,
ride deck, and trigger zone.
