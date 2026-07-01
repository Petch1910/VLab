# Third Target Slice Selection Spec

Milestone: `M43-01`

## Purpose

`M43-01` selects one third slice for offline analysis from the candidate queue
produced by `M42-04`.

This milestone only selects the target. It must not create recipe drafts,
runtime fixtures, saved decks, UI entries, or bot/playbook data.

## Inputs

- `outputs/target_slice/m42_04_multi_fixture_scale_decision.json`

## Outputs

- `outputs/target_slice/m43_01_third_target_slice_selection.json`
- `outputs/target_slice/m43_01_third_target_slice_selection.md`

## Selection Policy

- `M42-04` must allow the third-slice offline pipeline.
- The candidate queue must be non-empty.
- The first available candidate from the M42-04 queue is selected.
- Completed fixture groups are already excluded by M42-04.
- The selection is for offline analysis only.

## Boundary

This milestone must not:

- create recipe drafts
- create runtime fixtures
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_third_target_slice_selection.py
python -m unittest tests.test_third_target_slice_selection
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M43-01` is done when a third slice is selected for offline analysis, tests
cover allowed and blocked selection, and the next target is `M43-02`.
