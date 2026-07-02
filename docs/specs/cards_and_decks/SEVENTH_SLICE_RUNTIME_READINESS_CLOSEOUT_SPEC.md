# Seventh-Slice Runtime Readiness Closeout Spec

Milestone: `M60-closeout`

## Purpose

`M60-closeout` makes the runtime-readiness decision for the seventh-slice Neo
Nectar / G Series first recipe pipeline.

This is a decision artifact only. It closes the M60 scaffold evidence and
selects the next queue without mutating recipe drafts, recording human
acceptance, creating runtime fixtures, publishing saved decks, enabling G Zone
or Stride runtime, or enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m60_01_seventh_slice_fixture_scaffold.json`
- `outputs/target_slice/m60_02_seventh_slice_review_packet.json`
- `outputs/target_slice/m60_03_seventh_slice_recipe_draft_model.json`
- `outputs/target_slice/m60_04_seventh_slice_recipe_validation_report.json`
- `outputs/target_slice/m60_05_seventh_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m60_06_seventh_slice_blocker_repair_candidates.json`

Tests may pass in-memory reports until real upstream artifacts exist. The
closeout distinguishes scaffold evidence from real artifact availability so AI
agents do not mistake in-memory evidence for generated files.

## Outputs

- `outputs/target_slice/m60_closeout_seventh_slice_runtime_readiness.json`
- `outputs/target_slice/m60_closeout_seventh_slice_runtime_readiness.md`

## Decision Rules

`m60_scaffold_complete=true` only when:

- M60-01 has `ready_for_m60_02=true`
- M60-02 has `ready_for_m60_03=true`
- M60-03 has `ready_for_m60_04=true`
- M60-04 has `ready_for_m60_05=true`
- M60-05 has `ready_for_m60_06=true`
- M60-06 has `ready_for_m60_closeout=true`

`real_artifacts_available=true` only when the closeout is loaded from real
artifact files and all six M60 input artifacts exist.

`seventh_slice_runtime_ready_recipe_available=true` only when:

- `m60_scaffold_complete=true`
- M60-04 has at least one runtime-ready recipe
- M60-05 has at least one promotion-allowed consistency check
- M60-04 has no manual-review overlap
- M60-04 has no G Zone deferred evidence
- M60-04 has no Bloom/token deferred evidence

`human_selection_review_allowed=true` only when:

- `m60_scaffold_complete=true`
- runtime-ready recipe is not available
- M60-06 has at least one repair candidate ready for human review

## Expected Current Result

For the current seventh slice:

- M60 scaffold evidence is complete
- real M60 artifacts are not generated yet
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `23` recipes are blocked by manual-review overlap
- `23` recipes defer G Zone/Stride support
- `23` recipes defer Bloom/token-like support
- `23` manual repair previews are complete
- `21` grade-profile repair previews are complete
- next queue is `M61`

## Runtime Boundary

This milestone must not:

- modify M60-03 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone or Stride runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_seventh_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M60-01 through M60-06 outputs exist:

```powershell
python tools\deck\build_seventh_slice_runtime_readiness_closeout.py
```

## Done Rule

`M60-closeout` is done when:

- the closeout artifact reports `m60_scaffold_complete=true`
- runtime promotion remains disabled while manual review, human acceptance,
  G Zone/Stride decisions, and Bloom/token decisions are unresolved
- the closeout artifact selects `M61`
- tests cover success, repair-route fallback, manual/G Zone/Bloom runtime
  blocking, boundary flags, artifact metadata, and output round-trip
- project status docs point the active queue to `M61-01`
