# Fourth-Slice Runtime Readiness Closeout Spec

Milestone: `M48-closeout`

## Purpose

`M48-closeout` makes the runtime-readiness decision for the fourth-slice G-era
expanded Royal Paladin recipe pipeline.

This is a decision artifact only. It closes `M48` and selects the next queue
without mutating recipe drafts, recording human acceptance, creating runtime
fixtures, publishing saved decks, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m48_01_fourth_slice_fixture_scaffold.json`
- `outputs/target_slice/m48_02_fourth_slice_review_packet.json`
- `outputs/target_slice/m48_03_fourth_slice_recipe_draft_model.json`
- `outputs/target_slice/m48_04_fourth_slice_recipe_validation_report.json`
- `outputs/target_slice/m48_05_fourth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m48_06_fourth_slice_blocker_repair_candidates.json`

## Outputs

- `outputs/target_slice/m48_closeout_fourth_slice_runtime_readiness.json`
- `outputs/target_slice/m48_closeout_fourth_slice_runtime_readiness.md`

## Decision Rules

`m48_complete=true` only when:

- all six M48 input artifacts exist
- M48-01 has `ready_for_m48_02=true`
- M48-02 has `ready_for_m48_03=true`
- M48-03 has `ready_for_m48_04=true`
- M48-04 has `ready_for_m48_05=true`
- M48-05 has `ready_for_m48_06=true`
- M48-06 has `ready_for_m48_closeout=true`

`fourth_slice_runtime_ready_recipe_available=true` only when:

- `m48_complete=true`
- M48-04 has at least one runtime-ready recipe
- M48-05 has at least one promotion-allowed consistency check
- M48-04 has `g_zone_deferred_recipe_count=0`

`human_g_zone_review_allowed=true` only when:

- `m48_complete=true`
- runtime-ready recipe is not available
- M48-06 has at least one repair candidate ready for human review

## Expected M48 Result

For the current fourth slice:

- M48 is complete
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `25` recipes still have manual-review overlap
- `25` recipes have G Zone support deferred
- `25` repair candidates are ready for human review
- next queue is `M49`

## Runtime Boundary

This milestone must not:

- modify M48-03 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_runtime_readiness_closeout.py
python -m unittest tests.test_fourth_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M48-closeout` is done when:

- the closeout artifact reports `m48_complete=true`
- the closeout artifact keeps fourth-slice runtime promotion disabled while
  G Zone support is deferred
- the closeout artifact selects `M49`
- tests cover success, repair-route fallback, G Zone runtime blocking, boundary
  flags, and output round-trip
- project status docs point the active queue to `M49-01`
