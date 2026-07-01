# M21-05b4 Phase Actions Closeout

Status: Done

## Scope

- Added missing legal `SetPhase` actions for `StandAndDraw` and `Ride`.
- Existing PlayTable phase buttons now map to legal actions instead of relying
  on default button interactivity.
- `Stand` can now act as the current finish-setup path from `Mulligan` into
  the first turn.
- Added tests for generated Stand/Ride phase actions and Mulligan phase button
  availability.

## Safety

- Phase changes still execute through `RulesCore` and
  `GameActionService.SetPhase`.
- No direct `GameState` mutation was added to PlayTable UI.
- No online protocol, bot logic, Android flow, or release packaging was
  changed.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b4_phase_actions.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b4_phase_actions.xml`
  passed `973/973`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b4_phase_actions.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b4_phase_actions.json`
  passed with `blockers=[]`.

## Next

Continue M21 with a player-facing setup status/finish guidance pass, then move
to M21-06 Advanced drawer cleanup or M21-07 player-readable event log polish.
