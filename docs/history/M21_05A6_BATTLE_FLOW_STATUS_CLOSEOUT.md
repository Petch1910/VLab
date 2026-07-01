# M21-05a6 Battle Flow Status Closeout

Status: Done

## Scope

- Added a player-facing Battle Flow status panel to PlayTable.
- Added pure formatter coverage for common manual battle states:
  not in Battle phase, ready to attack, attack declared, guard placed, and
  trigger checked.
- Status text is derived from the display state/event log and does not expose
  private card instance ids.
- This is guidance only; it does not auto-resolve battle math or card effects.

## Limits

- Full battle resolution, damage application, and close-step automation remain
  future M21/M10+ work.
- The status panel intentionally gives next-step guidance rather than changing
  RulesCore state.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05f_battle_flow_status.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05f_battle_flow_status.xml`
  passed `955/955`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05f_battle_flow_status.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05f_battle_flow_status.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with manual note polish and the remaining player-facing battle
resolution controls.
