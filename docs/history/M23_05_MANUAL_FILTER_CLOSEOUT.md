# M23-05 Manual Filter Closeout

## Scope

Added a simple Manual search and category filter to the reusable Manual overlay.

## Changes

- Added `docs/specs/manual/MANUAL_FILTER_SPEC.md`.
- Added `ManualContentFilter` for pure search, category, formatting, and empty
  result fallback.
- Updated `ManualScreenOverlay` with:
  - `Manual Search Input`
  - `Manual Category Button`
  - filtered `Manual Body`
- Kept category filtering as a cycle button instead of a Unity dropdown to avoid
  dropdown-template risk in this Windows-first pass.
- Added EditMode tests for pure filtering and overlay controls.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m23_05_manual_filter.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m23_05_manual_filter.xml`
  passed `1032/1032`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m23_05_manual_filter.log`
  reported `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m23_05_manual_filter.json`
  reported `blockers=[]`.

## Guardrail Check

- Filter is read-only and does not mutate `GameState`.
- Manual content still comes from `ManualContentCatalog`.
- No comparator assets/code/data were copied.
- Windows-only verification; no Android, APK, LDPlayer, mobile QA, app
  packaging, or release packaging was run.

## Next Target

Continue with `M23-06`: content load, missing content fallback, and navigation
test closeout.
