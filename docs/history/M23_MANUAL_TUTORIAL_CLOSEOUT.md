# M23 Manual / Tutorial Closeout

## Scope

Closed `M23` In-App Manual / Tutorial for the Windows-first program track.

## Completed Tasks

- `M23-01`: Manual content spec.
- `M23-02`: Manual screen from Home and PlayTable.
- `M23-03`: Loading tips for data reload, card images, and deck load.
- `M23-04`: Original-content-only gate.
- `M23-05`: Manual search/category filter.
- `M23-06`: Content load, fallback, and navigation tests.
- `M23-07`: Manual/Tutorial readiness closeout.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m23_07_manual_closeout.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m23_07_manual_closeout.xml`
  passed `1033/1033`.
- Latest Windows build/smoke remains `M23-05`:
  - `client/unity/VanguardThaiSim/work/windows_build_m23_05_manual_filter.log`
    reported `Succeeded`, `errors=0`, `warnings=0`.
  - `client/unity/VanguardThaiSim/work/player_smoke_m23_05_manual_filter.json`
    reported `blockers=[]`.

Windows player smoke was not rerun for `M23-07` because it added verifier/test
coverage only; no runtime flow changed after `M23-05`.

## Guardrail Check

- No Android, APK, LDPlayer, mobile QA, app packaging, release candidate, or
  public distribution work was run.
- Manual content is original project text and has an automated originality
  guard.
- Manual UI is read-only and does not mutate `GameState`.
- Manual text avoids raw payload/private-id/hidden-state wording.
- No comparator assets/code/data were copied.

## Next Target

Continue with `M24-01`: Windows landscape Deck Builder layout with preview,
grid, deck list, counters, and rule badge.
