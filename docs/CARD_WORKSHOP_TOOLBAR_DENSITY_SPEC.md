# Card Workshop Toolbar Density Spec

Milestone: `M31-03`

## Purpose

Reduce Card Workshop/Card Browser toolbar density on Windows so common search
and filter actions stay clear while secondary maintenance controls stop reading
like primary gameplay actions.

## Problem

The current toolbar exposes search input, two filters, mode title, Search,
Clear, Cache, previous/next page, page number, and card-pool status in one row.
This was acceptable for debugging, but it is visually dense for player use and
matches the earlier feedback that the UI feels strange.

## Minimum Slice

- Keep Search, Clear, series/group filters, page navigation, and status usable.
- Move or visually demote secondary controls such as Cache.
- Make mode/status text easier to scan.
- Do not change card query behavior.
- Do not change deck validation or runtime pack data.
- Do not copy comparator assets/code/data.

## Verification Plan

- Formatter/layout helper tests if added.
- Runtime UI test for control placement or labels.
- Unity compile and EditMode.
- Client smoke and Windows player smoke because this touches runtime UI.

## Closeout - 2026-06-28

Implemented:

- Added `CardWorkshopToolbarFormatter`.
- Replaced long card-pool toolbar status with compact player-facing text:
  `Cards <total> | Shown <count> | Filters <none/on>`.
- Kept detailed filter/readiness guidance in the Card Workshop summary panel.
- Changed `Cache` into a secondary styled toolbar button with reduced width.

Preserved:

- Search/filter/page behavior unchanged.
- Card query behavior unchanged.
- Deck validation unchanged.
- Runtime pack data unchanged.
- No comparator assets/code/data.

Verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m31_03_card_workshop_toolbar_density_r2.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m31_03_card_workshop_toolbar_density_r3.xml`
  - `1169/1169` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m31_03_card_workshop_toolbar_density_r2.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m31_03_card_workshop_toolbar_density_r2.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m31_03_card_workshop_toolbar_density_r2.json`
  - `blockers=[]`

Next:

- `M31-04` should capture/review visual evidence from the current Windows build
  before more UI surgery.
