# M23-03 Loading Tips Closeout

## Scope

Added short player-facing loading tips for Windows flows where users can be
uncertain while data, images, or deck files are refreshed.

## Changes

- Added `docs/specs/manual/LOADING_TIPS_SPEC.md`.
- Added `LoadingTipCatalog` with central contexts:
  - `data_reload`
  - `card_images`
  - `deck_load`
- Home startup data reload now appends the data reload tip to the mode status.
- Card Browser card-detail image fallback can append the card image tip.
- Deck Tools load latest success/failure status can append the deck load tip.
- Existing formatter APIs remain compatible; new `WithTip` helpers are used
  only at the relevant UI integration points.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m23_03_loading_tips.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m23_03_loading_tips.xml`
  passed `1025/1025`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m23_03_loading_tips.log`
  reported `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m23_03_loading_tips.json`
  reported `blockers=[]`.

## Guardrail Check

- Windows-only verification; no Android, APK, LDPlayer, mobile QA, app
  packaging, or release packaging was run.
- Tips are original project text and do not copy comparator text or assets.
- Tips do not expose raw protocol payloads, hidden state, or local absolute
  paths.
- Tips do not change card data, deck legality, image cache behavior, or
  RulesCore state.

## Next Target

Continue with `M23-04`: original-content-only manual/tutorial gate.
