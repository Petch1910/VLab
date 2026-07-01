# M22-05 Cosmetic Metadata / Deck Legality Separation Closeout

Status: Done

## Scope

`M22-05` locked the rule that deck cosmetics and accessory metadata do not
affect deck legality validation.

Implemented:

- Functional regression test comparing validation before and after cosmetic
  metadata changes.
- Static source guard confirming `DeckValidator` does not reference appearance
  or cosmetic model names.

## Guardrails

- No format legality expansion.
- No Deck Builder UI change.
- No user asset file loading.
- No online payload or Photon change.
- No RulesCore or `GameState` mutation path change.
- No Android, APK, mobile QA, app packaging, or release work.

## Files

- Spec:
  `docs/specs/settings/COSMETIC_METADATA_DECK_LEGALITY_SEPARATION_SPEC.md`
- Tests:
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/DeckCosmeticLegalitySeparationTests.cs`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_05_cosmetic_legality.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_05_cosmetic_legality.xml`
  passed `1008/1008`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_05_cosmetic_legality.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_05_cosmetic_legality.json`
  passed with `blockers=[]`.

## Next

Proceed to `M22-06`: support user-provided asset slots through
manifest/hash/fallback only.
