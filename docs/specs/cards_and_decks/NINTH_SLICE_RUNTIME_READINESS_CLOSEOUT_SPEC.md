# Ninth-Slice Runtime Readiness Closeout Spec

Milestone: `M68-closeout`

## Purpose

`M68-closeout` makes the runtime-readiness decision for the ninth-slice Aqua
Force / `g_series_first` recipe pipeline.

This is a decision artifact only. It closes the M68 scaffold evidence and
selects the next queue without mutating recipe drafts, recording human
acceptance, creating runtime fixtures, publishing saved decks, enabling G Zone
runtime, enabling Stride runtime, enabling Aqua Force battle-order runtime, or
enabling bot/playbook use.

## Inputs

- `outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.json`
- `outputs/target_slice/m68_02_ninth_slice_review_packet.json`
- `outputs/target_slice/m68_03_ninth_slice_recipe_draft_model.json`
- `outputs/target_slice/m68_04_ninth_slice_recipe_validation_report.json`
- `outputs/target_slice/m68_05_ninth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m68_06_ninth_slice_blocker_repair_candidates.json`

Tests may pass in-memory reports until real upstream artifacts exist. The
closeout distinguishes scaffold evidence from real artifact availability so AI
agents do not mistake in-memory evidence for generated files.

## Outputs

- `outputs/target_slice/m68_closeout_ninth_slice_runtime_readiness.json`
- `outputs/target_slice/m68_closeout_ninth_slice_runtime_readiness.md`

## Decision Rules

`m68_scaffold_complete=true` only when:

- M68-01 has `ready_for_m68_02=true`
- M68-02 has `ready_for_m68_03=true`
- M68-03 has `ready_for_m68_04=true`
- M68-04 has `ready_for_m68_05=true`
- M68-05 has `ready_for_m68_06=true`
- M68-06 has `ready_for_m68_closeout=true`

`real_artifacts_available=true` only when the closeout is loaded from real
artifact files and all six M68 input artifacts exist.

`ninth_slice_runtime_ready_recipe_available=true` only when:

- `m68_scaffold_complete=true`
- M68-04 has at least one runtime-ready recipe
- M68-05 has at least one promotion-allowed consistency check
- M68-04 has no manual-review overlap
- M68-04 has no grade-profile review evidence
- M68-04 has no G Zone deferred evidence
- M68-04 has no Stride deferred evidence
- M68-04 has no Aqua Force battle-order deferred evidence
- M68-04 has no human-selection pending evidence

`human_selection_review_allowed=true` only when:

- `m68_scaffold_complete=true`
- runtime-ready recipe is not available
- M68-06 has at least one repair candidate ready for human review

## Expected Current Result

For the current ninth slice:

- M68 scaffold evidence is complete
- real M68 artifacts are not generated yet
- runtime-ready recipe count is `0`
- promotion-allowed consistency count is `0`
- `25` recipes are blocked by manual-review overlap
- `23` recipes need grade-profile acceptance
- `25` recipes defer G Zone support
- `25` recipes defer Stride support
- `25` recipes defer Aqua Force battle-order support
- `25` repair candidates are ready for human review
- `25` manual-overlap repair previews are complete
- `23` grade-profile repair previews are complete
- next queue is `M69`

## Runtime Boundary

This milestone must not:

- modify M68-03 recipe draft files
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_ninth_slice_runtime_readiness_closeout
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M68-01 through M68-06 outputs exist:

```powershell
python tools\deck\build_ninth_slice_runtime_readiness_closeout.py
```

## Done Rule

`M68-closeout` is done when:

- the closeout artifact reports `m68_scaffold_complete=true`
- runtime promotion remains disabled while human acceptance, grade-profile
  acceptance, G Zone support, Stride support, and Aqua Force battle-order
  support are unresolved
- the closeout artifact selects `M69`
- tests cover success, repair-route fallback, manual/G Zone/Stride/Aqua runtime
  blocking, boundary flags, artifact metadata, and output round-trip
- project status docs point the active queue to `M69-01`
