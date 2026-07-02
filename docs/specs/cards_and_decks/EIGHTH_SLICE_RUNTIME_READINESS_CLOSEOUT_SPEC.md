# Eighth-Slice Runtime Readiness Closeout Spec

Milestone: `M64-closeout`

## Purpose

`M64-closeout` makes the runtime-readiness decision for the eighth-slice Kagero
/ `link_joker_legion_mate` recipe pipeline.

This is a decision artifact only. It closes the M64 scaffold evidence and
selects the next queue without mutating recipe drafts, recording human
selection, creating runtime fixtures, publishing saved decks, enabling
Lock/Unlock runtime, enabling Legion/Mate runtime, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m64_01_eighth_slice_fixture_scaffold.json`
- `outputs/target_slice/m64_02_eighth_slice_review_packet.json`
- `outputs/target_slice/m64_03_eighth_slice_recipe_draft_model.json`
- `outputs/target_slice/m64_04_eighth_slice_recipe_validation_report.json`
- `outputs/target_slice/m64_05_eighth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m64_06_eighth_slice_blocker_repair_candidates.json`

Tests may pass in-memory reports until real upstream artifacts exist. The
closeout distinguishes scaffold evidence from real artifact availability so AI
agents do not mistake in-memory evidence for generated files.

## Outputs

- `outputs/target_slice/m64_closeout_eighth_slice_runtime_readiness.json`
- `outputs/target_slice/m64_closeout_eighth_slice_runtime_readiness.md`

## Decision Rules

`m64_scaffold_complete=true` only when:

- M64-01 has `ready_for_m64_02=true`
- M64-02 has `ready_for_m64_03=true`
- M64-03 has `ready_for_m64_04=true`
- M64-04 has `ready_for_m64_05=true`
- M64-05 has `ready_for_m64_06=true`
- M64-06 has `ready_for_m64_closeout=true`

`real_artifacts_available=true` only when the closeout is loaded from real
artifact files and all six M64 input artifacts exist.

`eighth_slice_runtime_ready_recipe_available=true` only when:

- `m64_scaffold_complete=true`
- M64-04 has at least one runtime-ready recipe
- M64-05 has at least one promotion-allowed consistency check
- M64-04 has no manual-review overlap
- M64-04 has no grade-profile review evidence
- M64-04 has no Lock runtime deferred evidence
- M64-04 has no Legion runtime deferred evidence
- M64-04 has no human-selection pending evidence

`human_selection_review_allowed=true` only when:

- `m64_scaffold_complete=true`
- runtime-ready recipe is not available
- M64-06 has at least one repair candidate ready for human review

## Expected Current Result

For the current eighth slice:

- M64 scaffold evidence is complete
- real M64 artifacts are not generated yet
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- manual-review overlap count is `0`
- `25` recipes need grade-profile acceptance
- `25` recipes defer Lock/Unlock support
- `25` recipes defer Legion/Mate support
- `25` human-selection candidates exist
- `25` grade-profile repair previews are complete
- next queue is `M65`

## Runtime Boundary

This milestone must not:

- modify M64-03 recipe draft files
- record human selection
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_eighth_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M64-01 through M64-06 outputs exist:

```powershell
python tools\deck\build_eighth_slice_runtime_readiness_closeout.py
```

## Done Rule

`M64-closeout` is done when:

- the closeout artifact reports `m64_scaffold_complete=true`
- runtime promotion remains disabled while human selection, grade-profile
  acceptance, Lock/Unlock decisions, and Legion/Mate decisions are unresolved
- the closeout artifact selects `M65`
- tests cover success, repair-route fallback, grade/Lock/Legion runtime
  blocking, boundary flags, artifact metadata, and output round-trip
- project status docs point the active queue to `M65-01`
