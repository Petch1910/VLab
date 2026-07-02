# Seventh-Slice Combo-To-Recipe Consistency Spec

Milestone: `M60-05`

## Purpose

`M60-05` checks whether each `M60-03` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M60-04` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers.

## Inputs

- `outputs/target_slice/m60_03_seventh_slice_recipe_draft_model.json`
- `outputs/target_slice/m60_04_seventh_slice_recipe_validation_report.json`

Tests may pass in-memory draft and validation reports until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m60_05_seventh_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m60_05_seventh_slice_combo_recipe_consistency_report.md`

## Checks

- Every recipe has the candidate source card.
- Every recipe has the candidate target card.
- Missing pair-card ids are reported.
- Pair-level manual-review dependency is reported separately from recipe-level
  manual-review dependency.
- G Zone deferred status remains visible as review evidence.
- Bloom/token deferred status remains visible as review evidence.
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

The current in-memory report checks `23` recipe drafts. Pair cards are present
for all `23`, with no missing pair-card checks. Promotion remains blocked
because all `23` drafts are still blocked by recipe-level manual-review
dependencies, G Zone support is deferred, and Bloom/token support is deferred.

## Verification

```powershell
python -m unittest tests.test_seventh_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M60-03 and M60-04 outputs exist:

```powershell
python tools\deck\check_seventh_slice_combo_recipe_consistency.py
```
