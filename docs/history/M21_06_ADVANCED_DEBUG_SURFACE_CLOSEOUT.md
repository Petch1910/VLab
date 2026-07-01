# M21-06 Advanced Debug Surface Closeout

Status: Done

## Scope

- The `Advanced` drawer is now available in local and online PlayTable modes.
- The drawer is hidden by default with `CanvasGroup.alpha=0` and
  `LayoutElement.ignoreLayout=true`.
- Trigger draft, trigger check summary, pending AUTO, manual resolution
  decision, apply preview, and related diagnostic controls now live under the
  Advanced drawer instead of the default side panel.
- The default side panel stays focused on player-facing setup guidance, battle
  flow, manual notes, selected-card preview, zone status, and match log.
- Updated EditMode tests that previously expected local debug buttons not to
  exist; the new invariant is that they exist only inside hidden Advanced UI.

## Safety

- No diagnostic control was removed; development tools remain available after
  opening Advanced.
- No direct `GameState` mutation was added to PlayTable UI.
- No hidden-state masking, online transport protocol, bot logic, Android flow,
  app packaging, or release work was changed.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_06_advanced_debug_cleanup.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_06_advanced_debug_cleanup_r2.xml`
  passed `981/981`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_06_advanced_debug_cleanup.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_06_advanced_debug_cleanup.json`
  passed with `blockers=[]`.

## Next

Move to `M21-07`: make the default event/replay log player-readable and keep
raw event/protocol details out of the primary PlayTable surface.
