# M24-02 Count-Line Deck Text Closeout

## Scope

Added human-readable count-line deck export/import alongside the existing
compact `VGTH1.` deck code.

## Changes

- Added `docs/specs/deck_builder/COUNT_LINE_DECK_TEXT_SPEC.md`.
- Added `CountLineDeckCodec`.
- Added Deck Tools buttons:
  - `Copy Text`
  - `Apply Text`
- Deck Tools input now accepts compact deck code or count-line deck text.
- Existing `DeckCodeCodec` behavior remains unchanged.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m24_02_count_line_deck_text.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m24_02_count_line_deck_text.xml`
  passed `1039/1039`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m24_02_count_line_deck_text.log`
  reported `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m24_02_count_line_deck_text.json`
  reported `blockers=[]`.

## Guardrail Check

- Invalid count-line text rejects before replacing the active deck.
- No deck legality rules changed.
- No compact deck code behavior changed.
- No card data, pack loading, or RulesCore behavior changed.
- Windows-only verification; no Android, APK, LDPlayer, mobile QA, app
  packaging, or release packaging was run.

## Next Target

Continue with `M24-03`: deck-code mismatch UI for missing card, pack version,
and hash mismatch.
