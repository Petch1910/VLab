# M21-03 PlayTable Zone Status Closeout

## Status

`M21-03` is complete.

## Completed Changes

- Added `PlayTableZoneStatusFormatter`.
- Added a read-only `Zone Status` panel to PlayTable.
- Zone status summarizes deck, hand, drop, damage, bind, order, ride deck, and
  trigger zone counts.
- Soul is shown as `Soul: not modeled` because the current core has no
  `GameZone.Soul`. This keeps the UI honest and avoids expanding RulesCore
  inside a Windows UX task.
- Added EditMode tests for null-player fallback, count formatting, and the soul
  modeling boundary.

## Safety

- No `GameZone` values were added.
- No `PlayerGameState` zones were added.
- No RulesCore behavior changed.
- No deck validation changed.
- No network payload contract changed.
- The PlayTable status surface reads from the masked display state and does not
  mutate `GameState`.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_03_zone_status.log`
  completed with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_03_zone_status.xml`
  passed `904/904`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_03_zone_status.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_03_zone_status.json`
  reports four steps and `blockers=[]`.

## Next Target

`M21-04`: add a clearer hand strip and selected-card preview.

## Superseded Note

`M21-04b` found that `GameZone.Soul` and `PlayerGameState.soul` already existed.
The `Soul: not modeled` status in this closeout was accurate for the M21-03 UI
slice but is now superseded by
`docs/history/M21_04B_SOUL_STATUS_LEDGER_CLOSEOUT.md`, which wires real Soul
counts into PlayTable status and ResourceLedger previews.
