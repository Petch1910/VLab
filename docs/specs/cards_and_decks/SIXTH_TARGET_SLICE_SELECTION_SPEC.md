# Sixth Target Slice Selection Spec

Milestone: `M55-01`

## Purpose

`M55-01` selects the sixth offline analysis target after the five-fixture scale
decision allows another offline-only slice.

It consumes the `M54-04` candidate queue, selects the first remaining feasible
candidate, and records the selected group/era for the next fixture-readiness
step.

## Inputs

- `outputs/target_slice/m54_04_five_fixture_scale_decision.json`

## Outputs

- `outputs/target_slice/m55_01_sixth_target_slice_selection.json`
- `outputs/target_slice/m55_01_sixth_target_slice_selection.md`

## Selection Rules

The selector must:

- require `sixth_slice_offline_pipeline_allowed = true`
- require at least one candidate
- select the first candidate in the M54-04 queue
- preserve the full candidate queue snapshot for audit
- mark the target as offline analysis only

## Boundary

This milestone must not:

- create recipe drafts
- create runtime fixtures
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone or Stride runtime
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_sixth_target_slice_selection.py
python -m unittest tests.test_sixth_target_slice_selection
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M55-01` is done when a sixth target is selected, tests cover allowed and
blocked paths, docs are updated, and the next target is `M55-02`.
