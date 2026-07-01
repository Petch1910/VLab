# Second-Slice Runtime Readiness Closeout Spec

Milestone: `M40-closeout`

## Purpose

`M40-closeout` makes the runtime-readiness decision for the Oracle Think Tank
second-slice recipe pipeline.

This is a decision artifact only. It closes `M40` and selects the next queue
without mutating recipe drafts, recording human acceptance, creating runtime
fixtures, publishing saved decks, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m40_01_second_slice_review_packet.json`
- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`
- `outputs/target_slice/m40_03_second_slice_recipe_validation_report.json`
- `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.json`

## Outputs

- `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.json`
- `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.md`

## Decision Rules

`m40_complete=true` only when:

- all five M40 input artifacts exist
- M40-01 has `ready_for_m40_02=true`
- M40-02 has `ready_for_m40_03=true`
- M40-03 has `ready_for_m40_04=true`
- M40-04 has `ready_for_m40_05=true`
- M40-05 has `ready_for_m40_closeout=true`

`second_slice_runtime_ready_recipe_available=true` only when:

- `m40_complete=true`
- M40-03 has at least one runtime-ready recipe
- M40-04 has at least one promotion-allowed consistency check

`human_repair_review_allowed=true` only when:

- `m40_complete=true`
- runtime-ready recipe is not available
- M40-05 has at least one repair candidate ready for human review

## Expected M40 Result

For the current Oracle Think Tank slice:

- M40 is complete
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `25` recipes still have manual-review overlap
- `25` repair candidates are ready for human review
- next queue is `M41`

## Runtime Boundary

This milestone must not:

- modify M40-02 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_runtime_readiness_closeout.py
python -m unittest tests.test_second_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M40-closeout` is done when:

- the closeout artifact reports `m40_complete=true`
- the closeout artifact keeps second-slice runtime promotion disabled
- the closeout artifact selects `M41`
- tests cover success, repair-route fallback, boundary flags, and output
  round-trip
- project status docs point the active queue to `M41-01`
