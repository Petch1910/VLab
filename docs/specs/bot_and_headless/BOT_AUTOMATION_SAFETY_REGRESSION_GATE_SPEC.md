# M26-07 Bot Automation Safety Regression Gate Spec

## Goal

Before continuing CPU/bot automation, keep three safety guarantees visible in
one regression gate:

- hidden state does not leak to player, spectator, or bot-safe views
- snapshot simulation does not mutate live state
- replay determinism reproduces the final state from event logs

## Scope

- Add a pure report model for the M26-07 gate.
- Reuse existing verifier APIs as the source of truth:
  - `HiddenStateViewHardeningVerifier`
  - `SnapshotSimulationPath`
  - `ReplayDeterminismVerifier`
- Add EditMode tests that fail if any check fails or goes missing.

## Non-Goals

- Do not add new bot gameplay decisions.
- Do not add automatic bot turns.
- Do not change `GameState`, `RulesCore`, RNG, replay, or hidden-state
  behavior.
- Do not run Android/mobile/app packaging verification.

## Done Rule

M26-07 is done when the report passes all three checks, round-trips JSON, docs
are updated, and Unity compile/EditMode verification passes.
