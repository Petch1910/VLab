# Packed State Decision Gate Spec

## Milestone

`M17-08`

## Goal

Create an explicit go/no-go gate before any packed-state implementation. This
milestone does not migrate `GameState`, does not add flat arrays, and does not
change simulation behavior.

## Decision Inputs

`PackedStateDecisionInput`

- `correctness_gates_stable`
- `hidden_state_gates_stable`
- `observation_api_stable`
- `profiler_baseline_exists`
- `target_average_elapsed_ms`
- `profiler_result`

## Decision Output

`PackedStateDecisionReport`

- `schema_version`
- `decision`
- `live_game_state_policy`
- `packed_state_policy`
- `target_average_elapsed_ms`
- `observed_average_elapsed_ms`
- `blockers`
- `allowed_next_steps`

Supported decisions:

- `defer`
- `allow_prototype`

## Gate Rules

The gate must defer if:

- correctness gates are not stable
- hidden-state gates are not stable
- observation/action/reward API is not stable
- profiler baseline is missing
- profiler result is missing, rejected, or empty
- no positive target average timing exists
- current profile is already within target

The gate may return `allow_prototype` only when:

- all safety gates are stable
- a profiler baseline exists and is accepted
- observed average runtime is slower than the target

`allow_prototype` means only:

- prototype a converter outside live `GameState`
- add readable-to-packed golden tests
- keep readable `GameState` as source of truth

## Non-Goals

- No packed state implementation.
- No live state migration.
- No performance budget CI gate.
- No RL optimization.
- No UI or network behavior.

## Verification

- Missing/safety-failed inputs defer.
- Within-target performance defers.
- Stable but slow profile allows prototype only.
- Report JSON keeps readable-state and no-migration policies visible.
