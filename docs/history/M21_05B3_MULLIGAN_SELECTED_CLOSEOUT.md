# M21-05b3 Mulligan Selected Card Closeout

Status: Done

## Scope

- Added player-facing `Mulligan` action on PlayTable.
- The button is enabled only during `Mulligan` when a hand card is selected.
- Legal action generation now emits one-card mulligan actions for each hand
  card, plus an empty keep-hand action.
- PlayTable executes mulligan through `RulesCore.TryExecute`; it does not
  mutate `GameState` directly.
- `RefreshUi()` now refreshes common action button states immediately, so
  buttons open in their correct enabled/disabled state before card selection.

## Limits

- This is a one-selected-card mulligan surface. Multi-select mulligan selection
  and a dedicated keep/finish setup button remain next.
- First-player selection and stand-up transition are not included in this
  slice.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b3_mulligan_selected_r2.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b3_mulligan_selected_r2.xml`
  passed `972/972`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b3_mulligan_selected.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b3_mulligan_selected.json`
  passed with `blockers=[]`.

## Next

Continue M21-05b with a player-facing keep/finish setup control, first-player
selection, and stand-up transition from `Mulligan` into the first turn.
