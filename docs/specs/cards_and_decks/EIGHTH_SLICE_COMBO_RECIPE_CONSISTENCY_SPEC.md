# Eighth-Slice Combo-To-Recipe Consistency Spec

Milestone: `M64-05`

## Purpose

`M64-05` checks whether each `M64-03` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M64-04` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers.

## Inputs

- `outputs/target_slice/m64_03_eighth_slice_recipe_draft_model.json`
- `outputs/target_slice/m64_04_eighth_slice_recipe_validation_report.json`

Tests may pass in-memory draft and validation reports until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m64_05_eighth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m64_05_eighth_slice_combo_recipe_consistency_report.md`

## Checks

- Every recipe has the candidate source card.
- Every recipe has the candidate target card.
- Missing pair-card ids are reported.
- Pair-level manual-review dependency is reported separately from recipe-level
  manual-review dependency.
- Lock runtime deferred status remains visible as review evidence.
- Legion runtime deferred status remains visible as review evidence.
- Recipe validation status is carried forward.
- Promotion remains blocked unless validation passes and runtime readiness is
  true.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report checks `25` recipe drafts. Pair cards are present
for all `25`, with no missing pair-card checks and no manual-review dependency
overlap. Promotion remains blocked because all `25` drafts are still pending
human selection, and Lock/Legion runtime support is deferred.

## Verification

```powershell
python -m unittest tests.test_eighth_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M64-03 and M64-04 outputs exist:

```powershell
python tools\deck\check_eighth_slice_combo_recipe_consistency.py
```
