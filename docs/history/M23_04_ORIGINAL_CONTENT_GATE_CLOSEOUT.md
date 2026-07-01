# M23-04 Original Content Gate Closeout

## Scope

Added an automated gate for runtime Manual and loading-tip text so future AI
handoffs do not accidentally introduce copied comparator text, URLs, raw
implementation language, or hidden-state details into player-facing content.

## Changes

- Added `docs/specs/manual/ORIGINAL_CONTENT_GATE_SPEC.md`.
- Added `ManualContentOriginalityGuard` and `ManualContentOriginalityReport`.
- Added `LoadingTipCatalog.AllTips()` so runtime loading tips are included in
  the originality scan.
- Reworded the Replay manual section from implementation/network wording to
  player-facing wording.
- Added EditMode tests for the current manual and loading tips.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m23_04_original_content_gate.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m23_04_original_content_gate.xml`
  passed `1027/1027`.

Windows player smoke was not rerun for this docs/test/content gate because no
runtime flow, scene routing, build setting, or RulesCore behavior changed after
the previous `M23-03` smoke.

## Guardrail Check

- No comparator text, assets, icons, code, or data were copied.
- Runtime manual text avoids URLs, local absolute paths, raw payload wording,
  private-card-id wording, and hidden-state wording.
- No `GameState` mutation behavior changed.
- Windows-only; no Android, APK, LDPlayer, mobile QA, app packaging, or release
  packaging was run.

## Next Target

Continue with `M23-05`: simple search/category filter in Manual.
