# Sixth-Slice Recipe Validator Spec

Milestone: `M56-04`

## Purpose

`M56-04` validates `M56-03` sixth-slice advisory recipe drafts before combo
consistency or repair work continues.

The selected slice is Shadow Paladin / `g_next_z`. Because this slice contains
G-era Stride and Grade 4 evidence, the validator explicitly keeps Grade 4/G-unit
cards out of main-deck readiness until G Zone support exists.

The validator is offline and read-only. It classifies drafts; it does not
promote any draft into a playable runtime deck.

## Inputs

- `outputs/target_slice/m56_03_sixth_slice_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m56_04_sixth_slice_recipe_validation_report.json`
- `outputs/target_slice/m56_04_sixth_slice_recipe_validation_report.md`

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
- Manual-review card overlap is a blocker.
- G Zone support deferred is review evidence, not a blocker.
- Human recipe selection remains a review item before runtime promotion.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current report validates `12` sixth-slice drafts. All `12` are blocked from
runtime readiness by manual-review card overlap. They preserve 50-card main
decks, 16 triggers, no Grade 4 main-deck cards, and no copy-limit violations.

## Verification

```powershell
python tools\deck\validate_sixth_slice_recipe_drafts.py
python -m unittest tests.test_sixth_slice_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```
