# M21-05b2 First Vanguard Setup Closeout

Status: Done

## Scope

- Added a legal `RideDeck -> Vanguard` setup move during `Mulligan` only when
  the player has no Vanguard.
- Added a local PlayTable Ride Deck panel so the player can select a ride-deck
  card and press `VG` instead of relying on an auto-pick.
- Updated common-action availability so `Mulligan` allows only
  `RideDeck -> Vanguard` movement; hand cards do not enable `VG` setup during
  `Mulligan`.
- Added PhaseTimingMatrix coverage for Mulligan setup movement while keeping
  battle movement illegal.

## Safety

- Setup uses `RulesCore` / `LegalGameAction` / `GameActionService.MoveCard`.
- No direct `GameState` mutation was added to PlayTable UI.
- First-vanguard setup does not change online protocol, bot logic, Android
  flow, or release packaging.
- This is not the complete setup wizard yet. Mulligan selection UI,
  first-player selection, and stand-up transition remain next.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b2_first_vanguard_setup.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b2_first_vanguard_setup.xml`
  passed `969/969`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b2_first_vanguard_setup.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b2_first_vanguard_setup.json`
  passed with `blockers=[]`.

## Next

Continue M21-05b with player-facing mulligan controls, first-player selection,
and stand-up transition from `Mulligan` to the first turn.
