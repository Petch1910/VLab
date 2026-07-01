# M21-04c Board Thumbnail Closeout

Status: Done

## Scope

- Added a pure `PlayTableBoardCardFaceFormatter` for board card title, stats,
  and thumbnail visibility decisions.
- Wired PlayTable vanguard/rear-guard zone entries to render compact card-face
  buttons instead of raw short card ids.
- Used the existing runtime card repository plus `CardImageCache` for lazy
  thumbnail loading.
- Kept hidden cards safe: face-down/hidden-card placeholders never expose card
  names, image paths, or thumbnails.
- Kept the change visual-only: no RulesCore, network, deck, or `GameState`
  mutation behavior changed.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04c_board_thumbnail.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04c_board_thumbnail.xml`
  passed `929/929`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04c_board_thumbnail.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04c_board_thumbnail.json`
  passed with `blockers=[]`.

## Next

Continue with `M21-05`: player-facing common actions and phase-enforced controls.
