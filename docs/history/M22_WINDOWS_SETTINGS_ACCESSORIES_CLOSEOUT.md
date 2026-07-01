# M22 Windows Settings / Deck Type / Accessories Closeout

Status: Done

## Completed Tasks

- `M22-01` PlayerSettings.
- `M22-02` DeckAppearanceMetadata.
- `M22-03` Home Settings screen.
- `M22-04` Deck Type / Accessories dialog.
- `M22-05` cosmetic metadata / deck legality separation.
- `M22-06` user deck asset slots through manifest/hash/fallback.
- `M22-07` settings/accessories test roll-up.
- `M22-08` closeout.

## Result

The Windows build now has a basic Settings screen from Home, deck appearance
metadata, a Deck Builder Deck Type / Accessories dialog, safe local user asset
slot validation, and regression coverage proving cosmetics do not affect deck
legality.

## Guardrails Preserved

- No Android, APK, mobile QA, app packaging, or release work.
- No official/comparator asset copying.
- No online payload or Photon transport change.
- No RulesCore or `GameState` mutation path change.
- Deck cosmetics remain separate from deck legality validation.

## Verification

Latest verification is the `M22-06` full run plus `M22-07` test roll-up:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_06_user_deck_assets.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_06_user_deck_assets.xml`
  passed `1014/1014`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_06_user_deck_assets.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_06_user_deck_assets.json`
  passed with `blockers=[]`.

## Next

Proceed to `M23-01`: In-App Manual / Tutorial content spec.
