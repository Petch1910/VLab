# Seventh-Slice Recipe Validator Spec

Milestone: `M60-04`

## Purpose

`M60-04` validates `M60-03` seventh-slice advisory recipe drafts before combo
consistency or repair work continues.

The selected slice is Neo Nectar / `g_series_first`. Because this slice contains
G-era Stride, Grade 4, Bloom-like, and token-like evidence, the validator keeps
Grade 4/G-unit cards out of main-deck readiness and treats Bloom/token support
as review-only until dedicated rules modules exist.

The validator is offline and read-only. It classifies drafts; it does not
promote any draft into a playable runtime deck.

## Inputs

- `outputs/target_slice/m60_03_seventh_slice_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory draft reports until the real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m60_04_seventh_slice_recipe_validation_report.json`
- `outputs/target_slice/m60_04_seventh_slice_recipe_validation_report.md`

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
- Bloom/token support deferred is review evidence, not a blocker.
- Human recipe selection remains a review item before runtime promotion.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report validates `23` seventh-slice drafts. All `23` are
blocked from runtime readiness by manual-review card overlap. They preserve
50-card main decks, 16 triggers, no Grade 4 main-deck cards, no missing cards,
and no copy-limit violations. `21` drafts carry grade-profile review evidence;
all `23` carry G Zone and Bloom/token deferred review evidence.

## Verification

```powershell
python -m unittest tests.test_seventh_slice_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M60-03 output exists:

```powershell
python tools\deck\validate_seventh_slice_recipe_drafts.py
```
