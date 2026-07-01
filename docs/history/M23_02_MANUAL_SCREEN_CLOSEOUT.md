# M23-02 Manual Screen Closeout

## Scope

Implemented the first in-app Manual screen as a reusable Windows UI overlay.
The screen opens from both Home and PlayTable and uses original project content
defined by `docs/specs/manual/MANUAL_CONTENT_SPEC.md`.

## Changes

- Added `ManualContentCatalog` with embedded App Guide and Vanguard Rules
  Basics sections plus loading tip candidates.
- Added `ManualScreenOverlay`, a reusable scrollable overlay with a close
  callback.
- Added a Home `Manual` button that opens the overlay without leaving Home.
- Added a PlayTable toolbar `Manual` button that opens the same overlay without
  mutating `GameState`.
- Added EditMode coverage for content, overlay creation, Home launch, and
  PlayTable launch.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m23_02_manual_screen.log`
  passed with no compiler errors.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m23_02_manual_screen.xml`
  passed `1020/1020`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m23_02_manual_screen.log`
  reported `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m23_02_manual_screen.json`
  reported `blockers=[]`.

## Guardrail Check

- Windows-only work; no Android, APK, LDPlayer, mobile QA, app packaging, or
  release packaging was run.
- Content is original for this project and does not copy comparator text,
  assets, icons, code, or data.
- Manual UI is read-only and does not mutate `GameState`.
- No private ids, raw network payloads, or hidden state are shown in manual
  content.

## Next Target

Continue with `M23-03`: loading tips for data reload, card images, and deck
load flows.
