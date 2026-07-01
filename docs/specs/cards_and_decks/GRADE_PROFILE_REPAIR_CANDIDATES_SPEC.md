# Grade Profile Repair Candidates Spec

Milestone: `M38-02`

## Purpose

`M38-02` proposes source-backed grade-profile repair candidates for accepted
seed recipe `recipe_003`.

The candidates are substitution previews: they add missing G0/G1/G2 cards and
remove surplus G3 cards to match the classic target profile. They do not mutate
the recipe draft or record human acceptance.

## Inputs

- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`
- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`
- `outputs/target_slice/m38_01_accepted_seed_human_review_packet.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m38_02_grade_profile_repair_candidates.json`
- `outputs/target_slice/m38_02_grade_profile_repair_candidates.md`

## Target Profile

- Grade 0: `17`
- Grade 1: `14`
- Grade 2: `11`
- Grade 3: `8`

## Runtime Boundary

This milestone must not:

- record human acceptance
- mutate recipe draft files
- create runtime decks
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_grade_profile_repair_candidates.py
python -m unittest tests.test_grade_profile_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M38-02` is done when it emits complete source-backed grade-profile repair
candidates, keeps runtime promotion disabled, and points to `M38-03`.

