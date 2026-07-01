# Revised Recipe Validation Rerun Spec

Milestone: `M37-05`

## Purpose

`M37-05` reruns deck recipe validation and combo consistency after applying the
M37-02 recommended trigger repair to the accepted seed recipe in memory only.

This milestone checks whether the trigger/deck-size blockers would clear. It
does not modify recipe source files and does not promote a runtime deck.

## Inputs

- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
- `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json`
- `outputs/target_slice/m36_01_first_slice_review_packet.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`
- `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.json`

## Outputs

- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`
- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.md`

## Required Findings

- accepted seed validation status before and after
- resolved blocker codes
- remaining blocker and review issue counts
- revised trigger counts
- revised grade counts
- revised combo consistency status
- runtime promotion decision

## Runtime Boundary

This milestone must not:

- write changes to the recipe draft source file
- create runtime decks
- accept rejected combo lines
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_revised_recipe_validation_rerun.py
python -m unittest tests.test_revised_recipe_validation_rerun
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M37-05` is done when the in-memory rerun shows the blocker delta, keeps runtime
promotion disabled without human acceptance, and points to `M37-closeout`.

