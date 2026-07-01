# Fifth Target Slice Selection Spec

Milestone: `M51-01`

## Purpose

`M51-01` selects the fifth offline analysis target after the four-fixture scale
decision allows another offline-only slice.

It consumes the `M50-04` candidate queue, selects the first remaining feasible
candidate, and records the selected group/era for the next fixture-readiness
step.

## Inputs

- `outputs/target_slice/m50_04_four_fixture_scale_decision.json`

## Outputs

- `outputs/target_slice/m51_01_fifth_target_slice_selection.json`
- `outputs/target_slice/m51_01_fifth_target_slice_selection.md`

## Selection Rules

The selector must:

- require `fifth_slice_offline_pipeline_allowed = true`
- require at least one candidate
- select the first candidate in the M50-04 queue
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
python tools\deck\build_fifth_target_slice_selection.py
python -m unittest tests.test_fifth_target_slice_selection
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M51-01` is done when a fifth target is selected, tests cover allowed and
blocked paths, docs are updated, and the next target is `M51-02`.
