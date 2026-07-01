# Deck Recipe Draft Model Spec

Milestone: `M36-02`

## Purpose

`M36-02` converts M35 advisory deck skeletons into explicit, reviewable deck
recipe drafts with card quantities and validation metadata. These drafts are
inputs for `M36-03`; they are not legal deck claims and must not be used as
runtime decks.

## Inputs

- `outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.json`
- `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json`
- `outputs/target_slice/m36_01_first_slice_review_packet.json`

## Outputs

- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m36_02_deck_recipe_draft_model.md`

## Draft Rules

- Use M35-D2 role groups as the source for draft quantities.
- Keep quantities advisory.
- Assume a conservative copy limit of `4` per card.
- Preserve rejected-line blockers from M36-01.
- Preserve accepted-seed blockers until M36-03 validation and human review.
- Allow slot gaps; M36-03 must validate and classify them.

## Runtime Boundary

This milestone must not:

- create a runtime deck
- publish a bot playbook
- enable bot integration
- parse live card text
- mutate `GameState`
- inject cards into player decks

## Verification

```powershell
python tools\deck\build_deck_recipe_draft_model.py
python -m unittest tests.test_deck_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M36-02` is done when:

- `25` recipe drafts are generated
- `1` accepted-seed recipe is preserved
- `24` rejected-line recipes remain blocked
- recipes report slot gaps and validation metadata
- `ready_for_m36_03=true`

