# Card Workshop First-Screen Clarity Spec

Milestone: `M31-02`

## Purpose

Make the Card Workshop/Card Browser first screen clearer on Windows so a player
can tell whether the card data is preparing, loaded, empty, or filtered without
reading debug-like controls.

## Problem

The player-facing Card Workshop area can still feel like a test harness:
search, dropdowns, cache controls, pagination, card grid, and deck controls
compete for attention. Earlier user feedback showed a blank/Loading-looking
screen where the next action was unclear.

## Minimum Slice

- Add a player-facing screen summary/header for Card Browser and Deck Builder
  modes.
- Add a stable card-pool readiness message that distinguishes:
  - preparing data
  - loaded with results
  - loaded with no results due to filters
  - card pack load failure
- Keep existing card query behavior unchanged.
- Keep deck validation and runtime pack data unchanged.
- Do not add comparator assets, icons, or copied layouts.

## Verification Plan

- Pure formatter tests for each readiness state.
- UI test that the runtime screen includes the player-facing summary/status.
- Unity compile and EditMode.
- Client smoke and Windows player smoke because this touches runtime UI.

## Closeout - 2026-06-28

Implemented:

- Added `CardWorkshopReadinessFormatter`.
- Added a `Card Workshop Summary Text` panel in Card Browser / Deck Builder.
- Summary now distinguishes:
  - preparing local card pack and filters
  - ready with visible results
  - ready with no matching results
  - card data unavailable
- The summary gives mode-specific next action text:
  - Deck Builder: select card, add to Main/Ride
  - Card Browser: select card to read details

Preserved:

- No card query behavior changes.
- No deck validation changes.
- No runtime pack data changes.
- No RulesCore changes.
- No comparator assets/code/data.

Verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m31_02_card_workshop_first_screen.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m31_02_card_workshop_first_screen.xml`
  - `1166/1166` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m31_02_card_workshop_first_screen.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m31_02_card_workshop_first_screen.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m31_02_card_workshop_first_screen.json`
  - `blockers=[]`

Next:

- `M31-03` should reduce Card Workshop toolbar density without changing search
  semantics.
