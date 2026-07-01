# M21-05b5 Setup Status Guidance Closeout

Status: Done

## Scope

- Added `PlayTableSetupStatusFormatter` as a pure, read-only formatter for
  setup guidance.
- PlayTable now shows a `Setup` status panel that tells the player when to pick
  a Ride Deck card for first Vanguard, mulligan selected hand cards, or press
  `Stand` to begin.
- Added formatter tests for missing state, missing player, first Vanguard
  placement guidance, mulligan guidance, setup-complete guidance, and hidden id
  non-leakage.
- Added a runtime UI assertion that `PlayTable Setup Status` is created.

## Safety

- The formatter does not execute commands and does not mutate `GameState`.
- The PlayTable surface remains read-only; setup actions still go through
  `RulesCore` and legal action availability.
- No online protocol, bot logic, Android flow, app packaging, or release work
  was changed.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b5_setup_guidance.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b5_setup_guidance.xml`
  passed `981/981`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b5_setup_guidance.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b5_setup_guidance.json`
  passed with `blockers=[]`.

## Next

Move to `M21-06`: hide debug, automation, trigger draft, pending ability, and
network payload surfaces inside Advanced-only UI so the default Windows table is
player-facing.
