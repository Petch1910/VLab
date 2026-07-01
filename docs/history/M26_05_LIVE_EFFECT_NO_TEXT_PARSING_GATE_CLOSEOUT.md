# M26-05 Live Effect No Text Parsing Gate Closeout

## Status

Done.

## What Changed

- Added `LiveEffectResolutionTextParsingGuard`, a runtime policy verifier for
  live effect resolution paths.
- Added `AbilityEffectHandlerPolicy` metadata for custom ability handlers.
- Updated `AbilityEffectRegistry.Register(...)` so custom handlers are
  structured-command-only by default and cannot register policies that allow
  live text parsing or LLM resolution.
- Updated `AbilityCore.Resolve(...)` to validate custom-handler policy before
  resolving effects.
- Added `LIVE_EFFECT_NO_TEXT_PARSING_GATE_SPEC.md`.

## Runtime Boundary

Live effect resolution may use structured `AbilityEffectType` values,
RulesCore commands, registered custom handlers with no-live-text/no-LLM policy,
the M26-04 structured template test gate, and manual fallback for unsupported
effects.

Live effect resolution may not parse card text at match time, call an LLM/model
for live effect decisions, or turn labels, notes, manual summaries, or UI card
text into executable logic.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_05_no_live_text_parsing_gate.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_05_no_live_text_parsing_gate.xml`
  passed `1106/1106`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_05_no_live_text_parsing_gate.log`
  completed with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_05_no_live_text_parsing_gate.json`
  passed with `blockers=[]`.

## Next Target

`M26-06`: Solo Play entry flow from Home.
