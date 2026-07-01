# Third-Slice Runtime Readiness Closeout Spec

Milestone: `M44-closeout`

## Purpose

`M44-closeout` makes the runtime-readiness decision for the third-slice recipe
pipeline.

This is a decision artifact only. It closes `M44` and selects the next queue
without mutating recipe drafts, recording human acceptance, creating runtime
fixtures, publishing saved decks, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m44_01_third_slice_fixture_scaffold.json`
- `outputs/target_slice/m44_02_third_slice_review_packet.json`
- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.json`
- `outputs/target_slice/m44_04_third_slice_recipe_validation_report.json`
- `outputs/target_slice/m44_05_third_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m44_06_third_slice_blocker_repair_candidates.json`

## Outputs

- `outputs/target_slice/m44_closeout_third_slice_runtime_readiness.json`
- `outputs/target_slice/m44_closeout_third_slice_runtime_readiness.md`

## Decision Rules

`m44_complete=true` only when:

- all six M44 input artifacts exist
- M44-01 has `ready_for_m44_02=true`
- M44-02 has `ready_for_m44_03=true`
- M44-03 has `ready_for_m44_04=true`
- M44-04 has `ready_for_m44_05=true`
- M44-05 has `ready_for_m44_06=true`
- M44-06 has `ready_for_m44_closeout=true`

`third_slice_runtime_ready_recipe_available=true` only when:

- `m44_complete=true`
- M44-04 has at least one runtime-ready recipe
- M44-05 has at least one promotion-allowed consistency check

`human_repair_review_allowed=true` only when:

- `m44_complete=true`
- runtime-ready recipe is not available
- M44-06 has at least one repair candidate ready for human review

## Expected M44 Result

For the current third slice:

- M44 is complete
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `25` recipes still have manual-review overlap
- `25` repair candidates are ready for human review
- next queue is `M45`

## Runtime Boundary

This milestone must not:

- modify M44-03 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_third_slice_runtime_readiness_closeout.py
python -m unittest tests.test_third_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M44-closeout` is done when:

- the closeout artifact reports `m44_complete=true`
- the closeout artifact keeps third-slice runtime promotion disabled
- the closeout artifact selects `M45`
- tests cover success, repair-route fallback, boundary flags, and output
  round-trip
- project status docs point the active queue to `M45-01`
