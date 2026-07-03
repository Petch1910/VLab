# Gated Fixture Artifact Materialization Audit Spec

Milestone: `M72-01`

## Purpose

`M72-01` audits whether the scaffold-safe fixture-consumption reports from the
first nine fixture chains have been materialized as real primary JSON CLI
artifacts before the project selects any tenth slice.

This is an audit only. It does not run upstream generators, write missing
fixture artifacts, publish decks, or enable runtime systems.

## Inputs

- `outputs/target_slice/m71_01_post_nine_fixture_queue_plan.json`
- The current filesystem state under `outputs/target_slice/`

Tests may pass an in-memory `M71-01` queue plan and an explicit set of
materialized primary artifact paths.

## Primary Artifacts Audited

The audit checks the primary JSON report for each fixture chain stage:

- schema validation
- deck text export
- headless load smoke
- scale decision

For the post-nine queue, the audit checks:

- `outputs/target_slice/m71_01_post_nine_fixture_queue_plan.json`

Companion artifacts such as markdown summaries, count-line text exports,
deck-code text files, Unity replay JSON, and Unity result JSON remain owned by
their generating milestone. `M72-01` uses primary JSON reports as the audit
contract so the gate stays deterministic and small.

## Outputs

- `outputs/target_slice/m72_01_gated_fixture_artifact_materialization_audit.json`
- `outputs/target_slice/m72_01_gated_fixture_artifact_materialization_audit.md`

These are audit artifacts only.

## Decision Rules

`M72-01` can run only when `M71-01` proves:

- version is `M71-01`
- `m71_01_ready` is true
- `ready_for_m72_01` is true
- `opens_m72_01` is true
- recommended milestone is `M72-01`

If `M71-01` is not ready, route to `M71-repair`.

If any required primary JSON artifact is missing, route to:

- `M72-02`: Materialize missing sixth-through-ninth fixture artifact chain

If every required primary JSON artifact is present, route to:

- `M73-01`: Tenth-slice selection gate

Even when every artifact is present, this audit must not select the tenth
slice itself.

## Current Expected Result

From the current worktree, real primary JSON artifacts exist for fixture chains
1-5 (`M39`, `M42`, `M46`, `M50`, `M54`).

The sixth through ninth fixture chains plus `M70-04` and `M71-01` real primary
JSON artifacts are still missing from `outputs/target_slice/`.

Therefore the expected `M72-01` decision is:

- audit ready
- not ready for `M73-01`
- ready for `M72-02`
- no runtime/UI/bot promotion

## Boundary

This milestone must not:

- select a tenth slice
- create runtime fixtures
- materialize missing artifacts
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable Bloom/token runtime
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- parse live card text
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_gated_fixture_artifact_materialization_audit
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact generation after the real `M71-01` file exists:

```powershell
python tools\deck\build_gated_fixture_artifact_materialization_audit.py
```

## Done Rule

`M72-01` is ready when the audit identifies materialized and missing primary
JSON fixture artifacts, routes missing evidence to `M72-02`, routes complete
evidence to `M73-01` without selecting a tenth slice, tests cover pass/fail
boundary behavior, docs are updated, and no runtime/UI/bot/GameState mutation
occurs.
