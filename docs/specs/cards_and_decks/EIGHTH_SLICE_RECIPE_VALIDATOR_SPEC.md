# Eighth-Slice Recipe Validator Spec

Milestone: `M64-04`

## Purpose

`M64-04` validates `M64-03` eighth-slice advisory recipe drafts before combo
consistency or repair work continues.

The selected slice is Kagero / `link_joker_legion_mate`. Because this slice
contains Link Joker / Legion Mate era evidence, the validator keeps Grade 4
cards out of main-deck readiness and treats Lock/Unlock and Legion/Mate support
as review-only until dedicated rules modules exist.

The validator is offline and read-only. It classifies drafts; it does not
promote any draft into a playable runtime deck.

## Inputs

- `outputs/target_slice/m64_03_eighth_slice_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory draft reports until the real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m64_04_eighth_slice_recipe_validation_report.json`
- `outputs/target_slice/m64_04_eighth_slice_recipe_validation_report.md`

## Validation Rules

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- Grade 4 main-deck count must equal `0` until dedicated format support exists.
- All cards must match the selected clan.
- Grade profile is review evidence, not a blocker.
- Manual-review card overlap is a blocker.
- Lock runtime deferred is review evidence, not a blocker.
- Legion runtime deferred is review evidence, not a blocker.
- Human recipe selection remains a review item before runtime promotion.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report validates `25` eighth-slice drafts. All `25` pass
count, trigger, copy-limit, missing-card, Grade 4, and clan checks, but remain
pending human selection before runtime readiness. All `25` carry grade-profile
review evidence and Lock/Legion deferred review evidence.

## Verification

```powershell
python -m unittest tests.test_eighth_slice_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M64-03 output exists:

```powershell
python tools\deck\validate_eighth_slice_recipe_drafts.py
```
