# Combo-Line To Recipe Consistency Spec

Milestone: `M36-04`

## Purpose

`M36-04` checks whether each M35-D3 combo line is represented by the
corresponding M36-02 deck recipe draft, while carrying forward M36-03 validator
status and review blockers.

This prevents a recipe from being promoted just because combo cards are present.

## Inputs

- `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json`
- `outputs/target_slice/m36_01_first_slice_review_packet.json`
- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m36_04_combo_recipe_consistency_report.json`
- `outputs/target_slice/m36_04_combo_recipe_consistency_report.md`

## Checks

- combo line has a matching recipe draft by `source_line_id`
- every combo card appears in that recipe draft
- combo cards do not depend on unresolved manual-review cards
- recipe validation status is carried forward
- promotion remains blocked unless recipe validation passes and review blockers
  clear

## Runtime Boundary

This is an offline consistency check only. It must not:

- create a runtime deck
- publish a bot playbook
- enable bot integration
- mutate `GameState`
- inject cards into player decks

## Verification

```powershell
python tools\deck\check_combo_recipe_consistency.py
python -m unittest tests.test_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M36-04` is done when all combo lines are checked, missing combo-card counts are
reported, promotion counts are explicit, and `ready_for_m36_05=true`.

