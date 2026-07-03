# Missing Fixture Artifact Materialization Plan Spec

Milestone: `M72-02`

## Purpose

`M72-02` turns the `M72-01` audit result into a safe ordered checklist for
materializing the missing sixth-through-ninth fixture primary JSON artifact
chain.

This milestone is a plan/checklist only. It must not run the commands, write
the missing artifacts, select a tenth slice, publish decks, or enable runtime
systems.

## Inputs

- `outputs/target_slice/m72_01_gated_fixture_artifact_materialization_audit.json`

Tests may pass an in-memory `M72-01` audit report.

## Outputs

- `outputs/target_slice/m72_02_missing_fixture_artifact_materialization_plan.json`
- `outputs/target_slice/m72_02_missing_fixture_artifact_materialization_plan.md`

These are planning artifacts only.

## Planned Chains

When `M72-01` reports the current missing-artifact state, `M72-02` should plan
these primary JSON artifact chains in order:

1. `M58-01` through `M58-04`: sixth fixture / Shadow Paladin
2. `M62-01` through `M62-04`: seventh fixture / Neo Nectar
3. `M66-01` through `M66-04`: eighth fixture / Kagero
4. `M70-01` through `M70-04`: ninth fixture / Aqua Force
5. `M71-01`: post-nine queue plan

Each planned step must include:

- missing artifact id
- milestone
- fixture chain
- role
- review-only command
- expected output path
- prerequisite notes
- explicit flags that execution and materialization are not allowed in this
  slice

## Decision Rules

If `M72-01` is not ready, route to:

- `M72-01-repair`

If `M72-01` has known missing artifacts, route to:

- `M72-03`: Materialize sixth fixture primary JSON artifacts

If `M72-01` reports no missing artifacts, route to:

- `M73-01`: Tenth-slice selection gate

Even when no artifacts are missing, `M72-02` must not select the tenth slice.

## Boundary

This milestone must not:

- execute materialization commands
- materialize missing artifacts
- select a tenth slice
- create runtime fixtures
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
python -m unittest tests.test_missing_fixture_artifact_materialization_plan
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact generation after the real `M72-01` file exists:

```powershell
python tools\deck\build_missing_fixture_artifact_materialization_plan.py
```

## Done Rule

`M72-02` is ready when it maps all known missing primary JSON artifacts from
`M72-01` to ordered review-only materialization steps, routes incomplete audits
to repair, routes complete audits to `M73-01` without selecting a tenth slice,
tests cover pass/fail/no-missing behavior, docs are updated, and no
runtime/UI/bot/GameState mutation occurs.
