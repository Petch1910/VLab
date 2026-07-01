# Deck Recipe Validation Closeout Spec

Milestone: `M36-closeout`

## Purpose

`M36-closeout` closes the M36 human-review-assisted deck recipe validation
queue. It summarizes the review packet, draft recipe model, recipe validator,
combo-line consistency check, and second-slice readiness comparison.

This milestone is a coordination/reporting gate only. It decides the next
offline work queue and must not promote any deck recipe, combo line, or bot
playbook into runtime.

## Inputs

- `outputs/target_slice/m36_01_first_slice_review_packet.json`
- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
- `outputs/target_slice/m36_04_combo_recipe_consistency_report.json`
- `outputs/target_slice/m36_05_second_slice_readiness_comparison.json`

## Outputs

- `outputs/target_slice/m36_closeout_deck_recipe_validation.json`
- `outputs/target_slice/m36_closeout_deck_recipe_validation.md`

## Required Findings

- total review items, accepted seed items, rejected combo-line items, and
  manual card review items
- recipe draft count and accepted-seed recipe count
- runtime-ready recipe count
- invalid, blocked-by-review, missing-card, copy-limit, slot-gap, and
  trigger-count mismatch counts
- combo cards present and promotable combo-line counts
- second-slice future recipe readiness
- explicit recommendation for the next milestone

## Runtime Boundary

This milestone must not:

- create runtime decks
- enable bot runtime integration
- publish playbook hints to gameplay
- auto-fill deck slots from raw card text
- mutate `GameState`
- change Unity UI

## Verification

```powershell
python tools\deck\build_deck_recipe_validation_closeout.py
python -m unittest tests.test_deck_recipe_validation_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M36-closeout` is done when the report closes M36, keeps runtime promotion
disabled, records the remaining blockers, and points the next queue at
`M37-01` blocker-focused recipe repair.

