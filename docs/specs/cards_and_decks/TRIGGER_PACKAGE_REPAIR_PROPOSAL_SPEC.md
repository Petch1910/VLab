# Trigger Package Repair Proposal Spec

Milestone: `M37-02`

## Purpose

`M37-02` turns the M37-01 advisory trigger package candidates into a
reviewable repair proposal for the accepted seed recipe.

It simulates which validation blockers would be resolved if a candidate
package were accepted, but it does not modify the recipe draft and does not
promote any runtime deck.

## Inputs

- `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.md`

## Required Findings

- original accepted seed validation status and issue codes
- package simulation count
- package count that resolves trigger/deck-size blockers
- recommended package id and profile id
- quantity delta with source-backed card ids
- resolved blockers
- remaining review issues
- final trigger counts and grade counts after simulated repair

## Runtime Boundary

This milestone must not:

- mutate recipe drafts
- create runtime decks
- enable bot integration
- parse live card text
- auto-inject deck cards
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_trigger_package_repair_proposal.py
python -m unittest tests.test_trigger_package_repair_proposal
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M37-02` is done when it recommends a source-backed trigger repair package,
shows the blocker delta, keeps runtime promotion disabled, and points to
`M37-03`.

