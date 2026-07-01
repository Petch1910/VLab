# Deck Recipe Validator Spec

Milestone: `M36-03`

## Purpose

`M36-03` validates the M36-02 advisory deck recipe drafts against runtime
SQLite card metadata and the current review blockers. It separates numeric deck
problems from review-status blockers so a full 50-card draft is not mistaken
for a usable deck.

## Inputs

- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.md`

## Checks

- card id exists in SQLite
- quantity is positive
- quantity does not exceed `cards.deck_limit`
- main deck explicit quantity equals `50`
- unfilled slot count is `0`
- trigger count equals `16`
- heal trigger count does not exceed `4`
- cards belong to the selected clan
- grade profile is reported as review metadata
- rejected-line review status remains blocking
- human acceptance remains a review requirement

## Runtime Boundary

The validator is offline only. It must not:

- create a runtime deck
- publish a bot playbook
- enable bot integration
- parse live card text
- mutate `GameState`
- inject cards into player decks

## Verification

```powershell
python tools\deck\validate_deck_recipe_drafts.py
python -m unittest tests.test_deck_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M36-03` is done when the validation report exists, all `25` recipe drafts are
checked, missing-card and copy-limit counts are reported, runtime-ready count is
explicit, and `ready_for_m36_04=true`.

