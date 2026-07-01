# Sixth-Slice Combo-To-Recipe Consistency Spec

Milestone: `M56-05`

## Purpose

`M56-05` checks whether each `M56-03` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M56-04` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers.

## Inputs

- `outputs/target_slice/m56_03_sixth_slice_recipe_draft_model.json`
- `outputs/target_slice/m56_04_sixth_slice_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m56_05_sixth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m56_05_sixth_slice_combo_recipe_consistency_report.md`

## Checks

- Every recipe has the candidate source card.
- Every recipe has the candidate target card.
- Missing pair-card ids are reported.
- Pair-level manual-review dependency is reported separately from recipe-level
  manual-review dependency.
- G Zone deferred status remains visible as review evidence.
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

The current report checks `12` recipe drafts. Pair cards are present for all
`12`, with no missing pair-card checks. Promotion remains blocked because all
`12` drafts are still blocked by manual-review dependencies and G Zone support
is deferred.

## Verification

```powershell
python tools\deck\check_sixth_slice_combo_recipe_consistency.py
python -m unittest tests.test_sixth_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```
