# Ninth Target Slice Selection Spec

Milestone: `M67-01`

## Purpose

`M67-01` consumes the `M66-04` eight-fixture scale decision and selects the
first remaining candidate as the ninth offline analysis slice.

This milestone selects only a planning target. It must not create recipe
drafts, runtime fixtures, saved decks, UI entries, bot/playbook data, G Zone
runtime, Stride runtime, Bloom/token runtime, Lock/Unlock runtime,
Legion/Mate runtime, or `GameState` mutations.

The implementation is scaffold-safe: tests may use an in-memory M66-04 scale
decision. Real CLI artifacts remain gated until the real M66-04 scale decision
artifact exists.

## Inputs

- `outputs/target_slice/m66_04_eight_fixture_scale_decision.json`

## Outputs

- `outputs/target_slice/m67_01_ninth_target_slice_selection.json`
- `outputs/target_slice/m67_01_ninth_target_slice_selection.md`

These outputs are written only when the CLI is run against an existing M66-04
real artifact.

## Selection Rules

The selector may choose a target only when:

- `summary.ready_for_m67 = true`
- `summary.ninth_slice_offline_pipeline_allowed = true`
- `decision.ninth_slice_offline_pipeline_allowed = true`
- `candidate_queue` is non-empty

When selection is allowed, the selector chooses the first candidate in the
candidate queue and stores a snapshot of the full queue for review.

## Boundary

This milestone must not:

- create recipe drafts
- create runtime fixtures
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_ninth_target_slice_selection
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M66-04 output exists:

```powershell
python tools\deck\build_ninth_target_slice_selection.py
```

## Done Rule

`M67-01` scaffold work is ready when the first M66-04 candidate is selected
from in-memory scale evidence, tests cover allowed and blocked paths, docs are
updated, and real artifacts remain gated until the M66-04 output file exists.
