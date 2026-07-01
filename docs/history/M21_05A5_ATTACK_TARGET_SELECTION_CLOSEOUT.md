# M21-05a5 Attack Target Selection Closeout

Status: Done

## Scope

- Added a compact opponent field surface for opponent Vanguard and Rear-guard
  public targets.
- Added target selection separate from local selected-card selection.
- Added `Atk Target` for the first explicit attack flow:
  select local attacker, select opponent target, execute matching legal
  `DeclareAttack`.
- Added availability checks so `Atk Target` is enabled only when the selected
  attacker and selected target match a legal action.
- Kept attack execution behind the RulesCore/legal-action facade; the UI does
  not mutate `GameState` directly.

## Limits

- This still does not implement full guard-step or battle-resolution guidance.
- This does not add circle-specific rear-guard slots yet; it renders compact
  public target cards.
- This does not change attack legality rules or rest/stand handling.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05e_attack_target_selection.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05e_attack_target_selection.xml`
  passed `949/949`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05e_attack_target_selection.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05e_attack_target_selection.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with guard-step guidance, battle-resolution/close-step status,
and manual note polish.
