# Live Effect No Text Parsing Gate Spec

## Status

Implemented in `M26-05`.

## Purpose

Prevent live effect resolution from using LLMs, free-text card parsing, or
runtime skill-text interpretation during a match.

The runtime may display card text to players, store notes, and preserve manual
fallback reasons. It must not execute effects by parsing that text at match
time.

## Runtime Surface

`LiveEffectResolutionTextParsingGuard` provides:

- a current policy report for live effect resolution paths
- validation that all policy entries forbid live text parsing and LLM
  resolution
- validation for `AbilityDefinition` before `AbilityCore.Resolve` executes

`AbilityEffectRegistry.Register(...)` now stores an
`AbilityEffectHandlerPolicy` for custom handlers. The existing overload remains
available and registers handlers as structured-command-only by default.

## Allowed Live Resolution Paths

- `AbilityCore` enum-backed effects:
  `Draw`, `AddGiftMarker`, `SetPhase`, `MoveFirstFromZoneToZone`
- `AbilityEffectRegistry` custom handlers only when their policy has:
  `allows_live_text_parsing = false`
  and `allows_llm_resolution = false`
- `StructuredAbilityFixtureRunner` through the M26-04 template test gate
- manual fallback records for unsupported effects

## Forbidden Runtime Behavior

- Parse Thai, English, or Japanese card text during a match to decide an effect.
- Call an LLM/model to resolve live effects.
- Let notes, labels, manual summaries, or UI card text become executable logic.
- Register custom effect handlers that declare live text parsing or LLM
  resolution.

## Boundaries

M26-05 does not:

- add new effect templates
- expand structured ability coverage
- remove card text display from UI
- replace manual fallback with automation
- implement offline ability conversion tooling

Offline conversion from reviewed card text into structured ability data remains
a future data-tool task and must validate into schema/templates before runtime
use.

## Verification

EditMode coverage verifies:

- current policy report validates
- reports with live text or LLM resolution enabled reject
- default custom handler registration receives a structured-command-only policy
- custom handlers with live-text/LLM policies cannot register
- missing custom handlers still return manual fallback without mutating state
- policy outputs round-trip JSON

## Next Work

`M26-06`: Solo Play entry flow from Home.
