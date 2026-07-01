# M23-01 Manual Content Spec Closeout

Status: Done

## Scope

`M23-01` defined the original in-app manual content structure for the Windows
program.

Created:

- App Guide section outline.
- Vanguard Rules Basics section outline.
- Loading tip candidates.
- Manual content data shape for `M23-02`.
- Non-goals and comparator/copyright boundaries.

## Files

- Spec: `docs/specs/manual/MANUAL_CONTENT_SPEC.md`

## Verification

Docs-only change. No Unity runtime code changed in this milestone. The current
technical baseline remains the latest M22 run:

- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_06_user_deck_assets.xml`
  passed `1014/1014`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_06_user_deck_assets.json`
  passed with `blockers=[]`.

## Next

Proceed to `M23-02`: implement the Manual screen from Home and PlayTable.
