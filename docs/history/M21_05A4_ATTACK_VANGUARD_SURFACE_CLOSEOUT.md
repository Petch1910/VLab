# M21-05a4 Attack Vanguard Surface Closeout

Status: Done

## Scope

- Added a player-facing `Atk VG` PlayTable button for the first narrow attack
  declaration path.
- The action is enabled only when the selected local vanguard or rear-guard has
  a legal `DeclareAttack` action targeting the opponent vanguard.
- The PlayTable command still goes through the RulesCore/legal-action facade;
  it does not mutate `GameState` directly.
- Added player-facing unavailable-action text for attack attempts.
- Updated event and replay formatting so attack declarations read as match-log
  text and do not expose private card instance ids.

## Limits

- This is not the full battle target picker yet.
- It only covers the common manual shortcut: selected attacker attacks the
  opponent vanguard.
- Full attack sequence UX, guard-step guidance, battle resolution polish, and
  target selection remain in M21-05.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05d_attack_vanguard_surface.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05d_attack_vanguard_surface.xml`
  passed `945/945`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05d_attack_vanguard_surface.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05d_attack_vanguard_surface.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with the remaining battle-flow polish: target selection,
guard-step guidance, battle-resolution/close-step status, and manual note
surface.
