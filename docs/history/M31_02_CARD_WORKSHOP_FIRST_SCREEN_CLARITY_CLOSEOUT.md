# M31-02 Card Workshop First-Screen Clarity Closeout

Date: 2026-06-28

## Scope

Clarify the first Card Browser / Deck Builder screen so the player sees card
pool readiness and next action guidance instead of only toolbar controls.

## Implemented

- Added `CardWorkshopReadinessFormatter`.
- Added player-facing `Card Workshop Summary Text` to the left detail panel.
- Summary reports preparing, ready, no-results, and load-failure states.
- Summary gives mode-specific next actions for Deck Builder and Card Browser.
- Added formatter tests and a runtime UI EditMode test that checks the summary
  appears after Card Browser bootstrap loads the runtime pack.

## Preserved Boundaries

- No card repository/query behavior changes.
- No deck validation changes.
- No runtime pack data changes.
- No RulesCore changes.
- No Android/mobile/release work.
- No comparator asset/code/data copying.

## Verification

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

## Result

The Card Workshop first screen now has a stable player-facing readiness summary.
The next UI issue is toolbar density: search, filter, cache, pagination, and
mode/status controls still compete in one row.
