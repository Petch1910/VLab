# Second-Slice Combo-To-Recipe Consistency Spec

Milestone: `M40-04`

## Purpose

`M40-04` checks whether each `M40-02` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M40-03` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers.

## Inputs

- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`
- `outputs/target_slice/m40_03_second_slice_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.md`

## Checks

- every recipe has the candidate source card
- every recipe has the candidate target card
- missing pair-card ids are reported
- pair-level manual-review dependency is reported separately from recipe-level
  manual-review dependency
- recipe validation status is carried forward
- promotion remains blocked unless validation passes and runtime readiness is
  true

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\check_second_slice_combo_recipe_consistency.py
python -m unittest tests.test_second_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M40-04` is done when:

- all `25` M40-02 drafts are checked
- candidate pair cards are present in all checked drafts
- missing pair-card count is `0`
- promotion allowed count is explicit
- recipe-level manual-review dependency remains visible
- `ready_for_m40_05=true`

