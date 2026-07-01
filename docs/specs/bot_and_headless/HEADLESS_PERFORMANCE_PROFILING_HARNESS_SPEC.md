# Headless Performance Profiling Harness Spec

## Milestone

`M17-07`

## Goal

Add a bounded local profiling harness for headless simulation runs. The profiler
records timing summaries for engineering diagnostics without changing
simulation behavior, action choice, reward, UI, network payloads, or live
`GameState`.

## API

`HeadlessPerformanceProfiler.Run(request)`

- runs `HeadlessSimulationRunner.Run` for sequential seeds
- measures elapsed milliseconds per run with `Stopwatch`
- returns compact summary metrics and per-run smoke results
- rejects `run_count` outside the bounded range `1..50`

`HeadlessPerformanceProfileRequest`

- `start_seed`
- `run_count`
- `ruleset`
- `deck_code`

`HeadlessPerformanceProfileResult`

- `schema_version`
- `accepted`
- `rejection_reason`
- `profiler_id`
- `start_seed`
- `run_count`
- `accepted_count`
- `blocked_count`
- `ruleset`
- `total_elapsed_ms`
- `average_elapsed_ms`
- `min_elapsed_ms`
- `max_elapsed_ms`
- `behavior_policy`
- `runs`

`HeadlessPerformanceRunRecord`

- `index`
- `seed`
- `accepted`
- `failure_reason`
- `elapsed_ms`
- `actions_executed`
- `event_count`
- `final_phase`

## Behavior Policy

The profiler uses:

```text
timing_summary_only_no_simulation_changes
```

Timing data is diagnostic only. It must not become:

- a legal action selector
- a reward signal
- a state mutation source
- a network payload
- a packed-state migration

## Non-Goals

- No distributed workers.
- No RL training.
- No packed state.
- No Unity UI surface.
- No CI performance budget gate yet.

## Verification

- Bounded profile request records accepted headless run summaries.
- Out-of-range `run_count` rejects before simulation.
- Simulation summary fields remain deterministic for the same seed; elapsed
  timing is allowed to vary.
