# Sixth-Slice Runtime Readiness Closeout Spec

Milestone: `M56-closeout`

## Purpose

`M56-closeout` makes the runtime-readiness decision for the sixth-slice Shadow
Paladin / G NEXT-G Z recipe pipeline.

This is a decision artifact only. It closes `M56` and selects the next queue
without mutating recipe drafts, recording human acceptance, creating runtime
fixtures, publishing saved decks, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m56_01_sixth_slice_fixture_scaffold.json`
- `outputs/target_slice/m56_02_sixth_slice_review_packet.json`
- `outputs/target_slice/m56_03_sixth_slice_recipe_draft_model.json`
- `outputs/target_slice/m56_04_sixth_slice_recipe_validation_report.json`
- `outputs/target_slice/m56_05_sixth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m56_06_sixth_slice_blocker_repair_candidates.json`

## Outputs

- `outputs/target_slice/m56_closeout_sixth_slice_runtime_readiness.json`
- `outputs/target_slice/m56_closeout_sixth_slice_runtime_readiness.md`

## Decision Rules

`m56_complete=true` only when:

- all six M56 input artifacts exist
- M56-01 has `ready_for_m56_02=true`
- M56-02 has `ready_for_m56_03=true`
- M56-03 has `ready_for_m56_04=true`
- M56-04 has `ready_for_m56_05=true`
- M56-05 has `ready_for_m56_06=true`
- M56-06 has `ready_for_m56_closeout=true`

`sixth_slice_runtime_ready_recipe_available=true` only when:

- `m56_complete=true`
- M56-04 has at least one runtime-ready recipe
- M56-05 has at least one promotion-allowed consistency check
- M56-04 has no manual-review overlap
- M56-04 has no G Zone deferred blocker

`human_selection_review_allowed=true` only when:

- `m56_complete=true`
- runtime-ready recipe is not available
- M56-06 has at least one repair candidate ready for human review

## Expected M56 Result

For the current sixth slice:

- M56 is complete
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `12` recipes are blocked by manual-review overlap
- `12` recipes defer G Zone/Stride support
- `12` manual repair previews are complete
- `12` grade-profile repair previews are complete
- next queue is `M57`

## Runtime Boundary

This milestone must not:

- modify M56-03 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_sixth_slice_runtime_readiness_closeout.py
python -m unittest tests.test_sixth_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M56-closeout` is done when:

- the closeout artifact reports `m56_complete=true`
- runtime promotion remains disabled while manual review, human acceptance, and
  G Zone/Stride decisions are unresolved
- the closeout artifact selects `M57`
- tests cover success, repair-route fallback, manual/G Zone runtime blocking,
  boundary flags, and output round-trip
- project status docs point the active queue to `M57-01`
