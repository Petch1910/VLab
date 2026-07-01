# M27-04 Windows Performance Gate Spec

## Goal

Turn the M27-03 baseline into a repeatable Windows-only memory/performance gate
before continuing the stability pass.

## Scope

The gate checks:

- Card Browser / Deck Builder data-path timings from
  `WindowsPerformanceBaseline`.
- `CardImageCache` bounded retention and `ClearMemory()` behavior.
- A bounded headless performance profile for smoke-level simulation cost.
- A PlayTable target contract of `30fps` / `33.334ms` frame budget.

## Important Limits

This milestone does not capture GPU frame time, scroll frame drops, or exact
texture memory bytes. Those require a later interactive/profiler pass. M27-04
only blocks obvious regressions that can be checked reliably in EditMode or
Unity batch mode.

## Runtime Policy

- The gate does not mutate live `GameState`.
- The gate does not publish network payloads.
- The gate does not change card data, deck data, or player settings.
- Android/mobile/APK verification remains deferred.

## Artifact

`WindowsPerformanceGateRunner` writes:

`client/unity/VanguardThaiSim/work/windows_performance_gate_m27_04.json`

## Verification

- Unity compile.
- Unity EditMode tests.
- Optional Unity batch runner:
  `VanguardThaiSim.EditorTools.WindowsPerformanceGateRunner.RunFromCommandLine`.
