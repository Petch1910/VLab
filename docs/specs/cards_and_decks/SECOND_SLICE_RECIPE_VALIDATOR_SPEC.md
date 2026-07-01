# Second-Slice Recipe Validator Spec

Milestone: `M40-03`

## Purpose

`M40-03` validates `M40-02` second-slice advisory recipe drafts before combo
consistency or repair work continues.

The validator is offline and read-only. It classifies drafts; it does not
promote a deck.

## Inputs

- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m40_03_second_slice_recipe_validation_report.json`
- `outputs/target_slice/m40_03_second_slice_recipe_validation_report.md`

## Validation Rules

- card ids must exist in runtime SQLite
- quantity must be positive
- quantity must not exceed SQLite `deck_limit`
- main deck explicit count must equal `50`
- trigger count must equal `16`
- Heal trigger count must not exceed `4`
- all cards must match the selected clan
- grade profile is review evidence, not a blocker
- manual-review card overlap is a blocker
- human recipe selection remains a review item before runtime promotion

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_second_slice_recipe_drafts.py
python -m unittest tests.test_second_slice_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M40-03` is done when:

- all `25` M40-02 drafts are validated
- missing-card recipe count is `0`
- copy-limit violation recipe count is `0`
- slot-gap recipe count is `0`
- trigger-count mismatch recipe count is `0`
- unresolved manual-review overlap is reported
- runtime-ready recipe count remains separate from validator availability
- `ready_for_m40_04=true`

