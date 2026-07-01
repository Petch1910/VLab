# Third-Slice Combo-To-Recipe Consistency Spec

Milestone: `M44-05`

## Purpose

`M44-05` checks whether each `M44-03` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M44-04` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers.

## Inputs

- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.json`
- `outputs/target_slice/m44_04_third_slice_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m44_05_third_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m44_05_third_slice_combo_recipe_consistency_report.md`

## Checks

- Every recipe has the candidate source card.
- Every recipe has the candidate target card.
- Missing pair-card ids are reported.
- Pair-level manual-review dependency is reported separately from recipe-level
  manual-review dependency.
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

## Verification

```powershell
python tools\deck\check_third_slice_combo_recipe_consistency.py
python -m unittest tests.test_third_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M44-05` is done when:

- all `25` M44-03 drafts are checked
- candidate pair cards are present in all checked drafts
- missing pair-card count is `0`
- promotion allowed count is explicit
- recipe-level manual-review dependency remains visible
- `ready_for_m44_06=true`
