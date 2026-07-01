# Fourth-Slice Recipe Validator Spec

Milestone: `M48-04`

## Purpose

`M48-04` validates `M48-03` fourth-slice advisory recipe drafts before combo
consistency or repair work continues.

The validator is offline and read-only. It classifies drafts; it does not
promote any draft into a playable runtime deck.

## Inputs

- `outputs/target_slice/m48_03_fourth_slice_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m48_04_fourth_slice_recipe_validation_report.json`
- `outputs/target_slice/m48_04_fourth_slice_recipe_validation_report.md`

## Validation Rules

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- Grade 4 main-deck count must equal `0` until G Zone support exists.
- All cards must match the selected clan.
- Grade profile is review evidence, not a blocker.
- G Zone deferred is review evidence, not a blocker.
- Manual-review card overlap is a blocker.
- Human recipe selection remains a review item before runtime promotion.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_fourth_slice_recipe_drafts.py
python -m unittest tests.test_fourth_slice_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```
