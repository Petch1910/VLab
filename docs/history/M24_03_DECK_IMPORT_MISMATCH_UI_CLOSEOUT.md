# M24-03 Deck Import Mismatch UI Closeout

## Scope

Added compatibility reporting after deck code or count-line deck text import.

## Changes

- Added `docs/specs/deck_builder/DECK_IMPORT_MISMATCH_UI_SPEC.md`.
- Added `DeckImportCompatibilityAnalyzer`.
- Added `DeckImportCompatibilityFormatter`.
- Added compatibility status to `Apply Code` and `Apply Text`.
- Extended count-line deck text with optional `PackDefinitionHash`.

## Compatibility Checks

- Missing card ids in the active pack.
- Pack id mismatch.
- Pack version mismatch.
- Pack definition hash mismatch when imported count-line text includes a hash.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m24_03_deck_import_mismatch.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m24_03_deck_import_mismatch.xml`
  passed `1042/1042`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m24_03_deck_import_mismatch.log`
  reported `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m24_03_deck_import_mismatch.json`
  reported `blockers=[]`.

## Guardrail Check

- Parse failures still reject without replacing the active deck.
- Compatibility warnings do not mutate card data or deck legality rules.
- Existing compact deck code remains compatible.
- Windows-only verification; no Android, APK, LDPlayer, mobile QA, app
  packaging, or release packaging was run.

## Next Target

Continue with `M24-04`: CGS-like custom pack adapter spec.
