# M27-04 Windows Performance Gate Closeout

## Status

Complete.

## What Changed

- Added `WindowsPerformanceGate` to evaluate the M27-03 baseline against
  Windows-only thresholds.
- Added image-cache retention and `ClearMemory()` checks for `CardImageCache`.
- Added bounded headless performance profiling to the gate.
- Added `WindowsPerformanceGateRunner` for batch-mode JSON artifact output.
- Added EditMode tests for the gate report and JSON round-trip.

## Artifact

`client/unity/VanguardThaiSim/work/windows_performance_gate_m27_04.json`

Gate result:

- Accepted: `true`
- Blockers: `[]`
- Card count: `10,836`
- Repository load: `45ms`
- Card query: `21ms`
- Card detail: `4ms`
- Deck validation: `98ms`
- Deck code round-trip: `13ms`
- Cache retained thumbnails: `4/4`
- Cache retained full images: `2/2`
- Cache clear memory: `true`
- Headless profile: `3/3` accepted, average `149.75ms`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m27_04_windows_performance_gate.log`
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m27_04_windows_performance_gate_r2.xml`
  passed `1123/1123`
- Windows performance gate runner:
  `client/unity/VanguardThaiSim/work/windows_performance_gate_m27_04.log`

Windows player smoke was not rerun because M27-04 added a verification gate
and did not change player-facing runtime flow.

## Known Limits

The PlayTable `30fps` target is recorded as a contract in this gate. M27-04
does not capture GPU frame time or interactive scroll frame drops; those need a
later profiler/manual smoke pass if UI performance becomes a visible blocker.

## Next Target

`M27-05`: Graceful error handling.
