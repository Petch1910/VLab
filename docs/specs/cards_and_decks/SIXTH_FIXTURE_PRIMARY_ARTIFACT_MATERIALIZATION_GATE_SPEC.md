# Sixth Fixture Primary Artifact Materialization Gate Spec

Milestone: `M72-03`

## Purpose

`M72-03` gates the real materialization path for the sixth fixture primary JSON
artifact chain:

1. `M58-01` sixth fixture schema validation
2. `M58-02` sixth fixture deck text export
3. `M58-03` sixth fixture headless load smoke
4. `M58-04` six-fixture scale decision

This slice must not fake the earlier `M57` human-gated decisions. If the real
`M57-06` runtime fixture is missing, the gate reports the blocker and routes
back to the explicit `M57-02` human selection prerequisite.

## Inputs

Default real prerequisite:

- `outputs/target_slice/runtime_fixtures/m56_recipe_001_shadow_paladin_m57_06.json`

Optional Unity evidence:

- `outputs/target_slice/m58_03_sixth_fixture_unity_result.json`
- `outputs/target_slice/m58_03_sixth_fixture_unity_replay.json`

Tests may pass an in-memory fixture and temporary Unity evidence to prove the
M58 chain can be built without writing live artifacts.

## Outputs

The gate writes only:

- `outputs/target_slice/m72_03_sixth_fixture_primary_artifact_materialization_gate.json`
- `outputs/target_slice/m72_03_sixth_fixture_primary_artifact_materialization_gate.md`

It does not write `M58` artifacts in this slice.

## Decisions

If the real `M57-06` fixture is missing:

- Recommended milestone: `M57-02`
- Reason: explicit sixth-slice human recipe selection/acceptance chain is still
  required before real `M58` materialization.

If a fixture exists but no accepted Unity headless deck-code evidence is
provided:

- Recommended milestone: `M58-03-unity-headless-smoke`
- Reason: `M58-04` must not run until `M58-03` has accepted Unity evidence.

If the fixture and Unity evidence are both valid:

- Recommended milestone: `M72-04`
- Reason: the sixth fixture primary artifact chain is ready in memory and the
  next bounded gate can focus on the seventh fixture chain.

## Boundary

This milestone must not:

- fabricate `M57-02` selection or `M57-03` acceptance
- materialize missing `M58` artifacts without the real prerequisite chain
- create saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone or Stride runtime
- select a tenth slice
- parse live card text
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_fixture_primary_artifact_materialization_gate
python -m unittest discover -s tests -p "test_*.py"
```

Real gate report generation:

```powershell
python tools\deck\build_sixth_fixture_primary_artifact_materialization_gate.py
```

## Done Rule

`M72-03` is complete when the gate:

- lists the `M58-01` through `M58-04` sixth fixture chain
- reports missing real `M57-06` as a blocker instead of faking human-gated
  prerequisites
- proves with tests that a supplied in-memory fixture plus Unity evidence can
  build all four M58 reports
- stops before `M58-04` when Unity evidence is missing
- keeps saved-deck, UI, bot, G Zone, Stride, live parsing, tenth-slice, and
  `GameState` mutation disabled
- updates current docs/status with the real next target
