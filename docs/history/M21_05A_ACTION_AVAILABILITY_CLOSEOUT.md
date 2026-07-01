# M21-05a Action Availability Closeout

Status: Done

## Scope

- Added a pure `PlayTableCommonActionAvailability` model for player-facing
  common action button state.
- Added PlayTable phase buttons for `Stand` and `Ride`.
- Updated PlayTable primary and move buttons to enable/disable from current
  phase, selected card, legal actions, and local/online mode.
- Updated `PhaseTimingMatrix` to allow `MoveCard` during Ride phase as the
  temporary ride-to-vanguard command path until a dedicated Ride command exists.
- Kept RulesCore execution behavior unchanged; this slice only gates the
  player-facing UI controls and matrix reference.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05_action_availability.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05_action_availability.xml`
  passed `934/934`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05_action_availability.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05_action_availability.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with player-facing action execution surfaces: drive check,
damage check, guard/manual note, and then attack declaration flow.
