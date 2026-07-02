# Ninth-Slice Recipe Validator Spec

Milestone: `M68-04`

## Purpose

`M68-04` validates `M68-03` ninth-slice advisory recipe drafts before combo
consistency or repair work continues.

The selected slice is Aqua Force / `g_series_first`. Because this slice
contains G-era evidence, the validator keeps Grade 4 / G-unit cards out of
main-deck readiness and treats G Zone, Stride/G-unit, Generation Break, and
Aqua Force battle-count / attack-order support as review-only until dedicated
rules modules exist.

The validator is offline and read-only. It classifies drafts; it does not
promote any draft into a playable runtime deck.

## Inputs

- `outputs/target_slice/m68_03_ninth_slice_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory draft reports until the real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m68_04_ninth_slice_recipe_validation_report.json`
- `outputs/target_slice/m68_04_ninth_slice_recipe_validation_report.md`

## Validation Rules

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Trigger profile must equal `Critical=4`, `Draw=4`, `Heal=4`, `Stand=4`.
- Heal trigger count must not exceed `4`.
- Required grade coverage must include grades `0`, `1`, `2`, and `3`.
- Grade 4 main-deck count must equal `0` until dedicated G Zone support exists.
- All cards must match the selected clan.
- All cards must stay inside the ninth-slice source scope:
  `G-BT02`, `G-CB02`, `G-TD04`.
- Grade profile is review evidence, not a blocker.
- Manual-review card overlap is a blocker.
- G Zone deferred is review evidence, not a blocker.
- Stride/G-unit deferred is review evidence, not a blocker.
- Aqua Force battle-order deferred is review evidence, not a blocker.
- Human recipe selection remains a review item before runtime promotion.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report validates `25` ninth-slice drafts. All `25` pass
main deck, trigger, trigger-profile, required-grade coverage, set-scope,
copy-limit, missing-card, Grade 4, and clan checks. All `25` remain blocked by
manual-review card overlap, and carry G Zone, Stride, Aqua Force battle-order,
and human-selection review evidence. `23` of the `25` also carry grade-profile
review evidence.

## Verification

```powershell
python -m unittest tests.test_ninth_slice_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M68-03 output exists:

```powershell
python tools\deck\validate_ninth_slice_recipe_drafts.py
```
