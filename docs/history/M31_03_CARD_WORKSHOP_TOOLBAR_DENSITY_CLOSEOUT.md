# M31-03 Card Workshop Toolbar Density Closeout

Date: 2026-06-28

## Scope

Reduce Card Workshop toolbar density without changing card search, filters,
pagination, deck validation, or runtime pack data.

## Implemented

- Added `CardWorkshopToolbarFormatter`.
- Toolbar status now uses compact text:
  `Cards <total> | Shown <count> | Filters <none/on>`.
- Detailed readiness and filter guidance stays in the Card Workshop summary
  panel added in `M31-02`.
- `Cache` remains available but now uses a secondary muted button style and a
  narrower width.
- Runtime UI test verifies the compact toolbar status and secondary cache
  styling.

## Preserved Boundaries

- No search/filter/page behavior changes.
- No card repository/query behavior changes.
- No deck validation changes.
- No runtime pack data changes.
- No RulesCore changes.
- No Android/mobile/release work.
- No comparator asset/code/data copying.

## Verification

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

## Result

The Card Workshop toolbar is less verbose and the maintenance-oriented cache
action no longer reads like a primary action. The next UI pass should capture or
review current Windows visual evidence before further layout changes.
