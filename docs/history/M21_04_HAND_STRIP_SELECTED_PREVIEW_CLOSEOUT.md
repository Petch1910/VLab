# M21-04 Hand Strip And Selected-Card Preview Closeout

## Status

`M21-04` is complete.

## Completed Changes

- Added `PlayTableHandStripFormatter` for player-facing hand button labels.
- Added `PlayTableSelectedCardPreviewFormatter` for readable selected-card
  inspection text.
- PlayTable hand buttons now show compact index, grade, trigger, and display
  name when runtime card detail is available.
- Hand button object names remain `cardId + " Button"` so existing automation
  and EditMode tests keep working while the visible label improves.
- Selected-card preview now includes card id, display name, zone, type,
  grade/power/shield, trigger, clan/nation, Thai skill text, and a legal-action
  hint generated from current legal actions.
- Hidden or face-down cards render hidden preview text and do not leak runtime
  card detail.
- PlayTable repository loading now uses `CardPackFileSystem.LoadManifest` and
  `CardRepositoryFactory.LoadDefault`, matching Card Browser and Home.

## Safety

- No RulesCore behavior changed.
- No card data, deck validation, image assets, network payload contracts, or
  Photon behavior changed.
- UI still reads masked display state for rendered zones.
- Preview and hand-strip helpers are pure formatters and do not mutate
  `GameState`.
- Hidden-card formatter tests cover no card-id/name/text/action leak.

## Scope Adjustment Found During Work

The current code already contains `GameZone.Soul` and `PlayerGameState.soul`.
Therefore the next `M21-04b` task should not start by adding those types from
scratch. It should audit and wire the existing Soul model into PlayTable status,
ride/SoulBlast behavior, and tests where still missing.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04_hand_preview.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04_hand_preview.xml`
  passed `917/917`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04_hand_preview.log`
  reports `Windows build artifact result: Succeeded`, `errors=0`,
  `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04_hand_preview.json`
  reports four steps and `blockers=[]`.

## Next Target

`M21-04b`: audit and wire the existing Soul zone model into status/rules paths
that still treat Soul as unavailable.
