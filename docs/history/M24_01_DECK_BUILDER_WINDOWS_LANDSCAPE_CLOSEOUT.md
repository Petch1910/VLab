# M24-01 Deck Builder Windows Landscape Closeout

## Scope

Completed the first Windows landscape Deck Builder readability pass.

## Changes

- Added `docs/specs/deck_builder/DECK_BUILDER_WINDOWS_LANDSCAPE_SPEC.md`.
- Renamed the deck side title to player-facing `Deck Builder`.
- Added an explicit rule badge with format and playable state.
- Added compact deck counters for Main, Ride, G, and issue counts.
- Reduced deck panel text block heights so the deck list and action buttons fit
  better in the Windows landscape layout.
- Kept deck validation and deck mutation behavior unchanged.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m24_01_deck_builder_landscape.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m24_01_deck_builder_landscape.xml`
  passed `1035/1035`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m24_01_deck_builder_landscape.log`
  reported `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m24_01_deck_builder_landscape.json`
  reported `blockers=[]`.

## Guardrail Check

- No card legality rules changed.
- No card data, pack loading, save/load, import/export, or RulesCore state
  behavior changed.
- No comparator assets/code/data were copied.
- Windows-only verification; no Android, APK, LDPlayer, mobile QA, app
  packaging, or release packaging was run.

## Next Target

Continue with `M24-02`: human-readable count-line deck export/import.
