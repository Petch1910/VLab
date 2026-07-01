# Fifth-Slice Combo-To-Recipe Consistency Spec

Milestone: `M52-05`

## Purpose

`M52-05` checks whether each `M52-03` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M52-04` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers.

## Inputs

- `outputs/target_slice/m52_03_fifth_slice_recipe_draft_model.json`
- `outputs/target_slice/m52_04_fifth_slice_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m52_05_fifth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m52_05_fifth_slice_combo_recipe_consistency_report.md`

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
python tools\deck\check_fifth_slice_combo_recipe_consistency.py
python -m unittest tests.test_fifth_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```
