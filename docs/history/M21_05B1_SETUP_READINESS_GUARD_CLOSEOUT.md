# M21-05b1 Setup Readiness Guard Closeout

Status: Done

## Scope

- Added a pure `PlayTableSetupReadiness` model for deck-to-PlayTable start
  checks.
- Solo Play from Home now blocks PlayTable entry when the saved deck is not
  playable according to `DeckValidator`.
- Deck Builder `Start Game` now blocks PlayTable entry when the active deck is
  not playable according to `DeckValidator`.
- Player-facing rejection text includes Main/Ride/G counts and issue totals.
- This does not create the full setup wizard yet. First-vanguard selection,
  face-down setup, mulligan decisions, first-player selection, and stand-up
  remain the next M21-05b slices.

## Safety

- The guard is read-only and does not mutate `GameState`.
- The guard runs before `GameStateFactory.CreateTwoPlayerGame`.
- No online protocol, Photon payload, bot logic, Android flow, or release
  packaging was changed.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b1_setup_readiness.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b1_setup_readiness.xml`
  passed `965/965`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b1_setup_readiness.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b1_setup_readiness.json`
  passed with `blockers=[]`.

## Next

Continue M21-05b with the actual setup wizard slice: selected deck summary,
first Vanguard placement, mulligan loop, first-player selection, and stand-up
transition from `Mulligan` into the first turn.
