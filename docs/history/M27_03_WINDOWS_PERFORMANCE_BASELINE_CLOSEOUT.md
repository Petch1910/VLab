# M27-03 Windows Performance Baseline Closeout

## Status

Complete.

## What Changed

- Added `WindowsPerformanceBaseline` for Windows Card Browser and Deck Builder
  data-path timing.
- Added `WindowsPerformanceBaselineRunner` editor command for writing a JSON
  baseline artifact from Unity batch mode.
- Added EditMode tests for accepted metrics and JSON round-trip.

## Baseline Artifact

`client/unity/VanguardThaiSim/work/windows_performance_baseline_m27_03.json`

Recorded metrics:

- Cards: `10,836`
- Query size: `120`
- Repository load: `36ms`
- Card query: `24ms`
- Card detail: `5ms`
- Deck validation: `99ms`
- Deck code round-trip: `17ms`
- Blockers: `[]`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m27_03_windows_performance_baseline_r3.log`
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m27_03_windows_performance_baseline_r2.xml`
  passed `1121/1121`
- Baseline runner:
  `client/unity/VanguardThaiSim/work/windows_performance_baseline_m27_03.log`

Windows player smoke was not rerun because M27-03 added a baseline runner and
did not change the player smoke runtime path after M27-01.

## Next Target

`M27-04`: Memory / performance gate.
