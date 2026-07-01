# Accepted Seed Slot-Gap Completion Candidates Spec

Milestone: `M37-01`

## Purpose

`M37-01` proposes source-backed candidates for filling the accepted seed recipe
slot gaps found by M36. The accepted seed currently has normal-unit slots filled
but lacks trigger slots, so this milestone focuses on trigger package
candidates only.

The output is advisory. It must not modify the recipe draft, create a runtime
deck, or promote a bot/playbook line.

## Inputs

- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
- `outputs/target_slice/m36_closeout_deck_recipe_validation.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.json`
- `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.md`

## Candidate Rules

- Use only SQLite `cards` rows from the runtime pack.
- Use the accepted seed recipe's clan from SQLite, not a hard-coded string.
- Use only Classic Part 1 source series: `TD01`-`TD06`, `BT01`-`BT09`,
  `EB01`-`EB05`.
- Use only trigger units with available quantity under `deck_limit`.
- Respect copy limits and max 4 Heal triggers.
- Preserve the current recipe; do not write repaired quantities back into
  `m36_02_deck_recipe_draft_model.json`.

## Runtime Boundary

This milestone must not:

- mutate recipe drafts
- create runtime decks
- enable bot integration
- parse live card text
- mutate `GameState`
- auto-inject deck cards

## Verification

```powershell
python tools\deck\build_accepted_seed_slot_gap_candidates.py
python -m unittest tests.test_accepted_seed_slot_gap_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M37-01` is done when it emits complete advisory trigger-package candidates for
the accepted seed slot gap, keeps runtime promotion disabled, and points to
`M37-02`.

