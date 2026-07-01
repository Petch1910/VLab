# M21-05a2 Check And Guard Surface Closeout

Status: Done

## Scope

- Added the first player-facing trigger-check surface and `Guard` button to the
  PlayTable move/action row.
- Wired `Check` through the existing `GameActionType.TriggerCheck` RulesCore
  path.
- Wired `Guard` through the existing `GameActionType.Guard` RulesCore path for
  the selected hand card.
- Extended `PlayTableCommonActionAvailability` so `Check` and `Guard` are only
  interactable in battle contexts where the underlying legal action exists.
- Added player-facing fallback text for trigger-check and guard failures.

## Boundary

Drive Check and Damage Check were intentionally not split in this slice because
the committed action path still used a single `TriggerCheck` event. This gap is
closed by `docs/history/M21_05A3_TRIGGER_SOURCE_SPLIT_CLOSEOUT.md`, which
replaces the single trigger-check surface with separate `Drive` and
`Damage Check` buttons.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b_check_guard_surface.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b_check_guard_surface.xml`
  passed `937/937`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b_check_guard_surface.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b_check_guard_surface.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with trigger-check source metadata or attack declaration flow.
