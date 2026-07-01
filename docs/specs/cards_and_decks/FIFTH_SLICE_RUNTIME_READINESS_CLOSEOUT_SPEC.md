# Fifth-Slice Runtime Readiness Closeout Spec

Milestone: `M52-closeout`

## Purpose

`M52-closeout` makes the runtime-readiness decision for the fifth-slice Gold
Paladin / Link Joker-Legion Mate recipe pipeline.

This is a decision artifact only. It closes `M52` and selects the next queue
without mutating recipe drafts, recording human acceptance, creating runtime
fixtures, publishing saved decks, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m52_01_fifth_slice_fixture_scaffold.json`
- `outputs/target_slice/m52_02_fifth_slice_review_packet.json`
- `outputs/target_slice/m52_03_fifth_slice_recipe_draft_model.json`
- `outputs/target_slice/m52_04_fifth_slice_recipe_validation_report.json`
- `outputs/target_slice/m52_05_fifth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m52_06_fifth_slice_blocker_repair_candidates.json`

## Outputs

- `outputs/target_slice/m52_closeout_fifth_slice_runtime_readiness.json`
- `outputs/target_slice/m52_closeout_fifth_slice_runtime_readiness.md`

## Decision Rules

`m52_complete=true` only when:

- all six M52 input artifacts exist
- M52-01 has `ready_for_m52_02=true`
- M52-02 has `ready_for_m52_03=true`
- M52-03 has `ready_for_m52_04=true`
- M52-04 has `ready_for_m52_05=true`
- M52-05 has `ready_for_m52_06=true`
- M52-06 has `ready_for_m52_closeout=true`

`fifth_slice_runtime_ready_recipe_available=true` only when:

- `m52_complete=true`
- M52-04 has at least one runtime-ready recipe
- M52-05 has at least one promotion-allowed consistency check
- M52-06 has no remaining human-selection requirement

`human_selection_review_allowed=true` only when:

- `m52_complete=true`
- runtime-ready recipe is not available
- M52-06 has at least one repair candidate ready for human review

## Expected M52 Result

For the current fifth slice:

- M52 is complete
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `25` recipes still require human selection
- `25` grade-profile repair previews are complete
- `25` repair candidates are ready for human review
- next queue is `M53`

## Runtime Boundary

This milestone must not:

- modify M52-03 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fifth_slice_runtime_readiness_closeout.py
python -m unittest tests.test_fifth_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M52-closeout` is done when:

- the closeout artifact reports `m52_complete=true`
- the closeout artifact keeps fifth-slice runtime promotion disabled while
  human selection and acceptance are unresolved
- the closeout artifact selects `M53`
- tests cover success, repair-route fallback, human-selection runtime blocking,
  boundary flags, and output round-trip
- project status docs point the active queue to `M53-01`
